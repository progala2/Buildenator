using Buildenator.Abstraction;
using Buildenator.CodeAnalysis;
using Buildenator.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buildenator.Configuration;
using static Buildenator.Generators.NamespacesGenerator;
using static Buildenator.Generators.ConstructorsGenerator;

namespace Buildenator.Generators
{
    internal readonly ref struct BuilderSourceStringGenerator
    {
        private readonly BuilderProperties _builder;
        private readonly EntityToBuild _entity;
        private readonly FixtureProperties _fixtureConfiguration;
        private readonly MockingProperties _mockingConfiguration;
        private const string SetupActionLiteral = "setupAction";
        private const string ValueLiteral = "value";
        private const string FixtureLiteral = "_fixture";

        public BuilderSourceStringGenerator(
            in BuilderProperties builder,
            in EntityToBuild entity,
            in FixtureProperties fixtureConfiguration,
            in MockingProperties mockingConfiguration)
        {
            _builder = builder;
            _entity = entity;
            _fixtureConfiguration = fixtureConfiguration;
            _mockingConfiguration = mockingConfiguration;
        }

        public string CreateBuilderCode()
             => $@"{AutoGenerationComment}
{GenerateNamespaces(_entity.AdditionalNamespaces, _fixtureConfiguration, _mockingConfiguration)}

namespace {_builder.ContainingNamespace}
{{
{GenerateGlobalNullable()}{GenerateBuilderDefinition()}
    {{
{(_fixtureConfiguration.IsEmpty ? string.Empty : $"        private readonly {_fixtureConfiguration.Name} {FixtureLiteral} = new {_fixtureConfiguration.Name}({_fixtureConfiguration.ConstructorParameters});")}
{(_builder.IsDefaultContructorOverriden ? string.Empty : GenerateConstructor(_builder.Name, _entity, _fixtureConfiguration))}
{GeneratePropertiesCode()}
{GenerateBuildsCode()}
{GenerateBuildManyCode()}
{(_builder.StaticCreator ? GenerateStaticBuildsCode() : string.Empty)}
{GeneratePostBuildMethod()}
    }}
}}";

        private object GeneratePostBuildMethod()
            => _builder.IsPostBuildMethodOverriden
            ? string.Empty
            : @$"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public void PostBuild({_entity.FullName} buildResult) {{ }}";

        private string GenerateGlobalNullable()
            => _builder.NullableStrategy switch
            {
                NullableStrategy.Enabled => "#nullable enable\n",
                NullableStrategy.Disabled => "#nullable disable\n",
                _ => string.Empty
            };

        private string GenerateBuilderDefinition()
            => @$"    public partial class {_entity.FullNameWithConstraints.Replace(_entity.Name, _builder.Name)}";

        private string GeneratePropertiesCode()
        {
            var properties = _entity.GetAllUniqueSettablePropertiesAndParameters();

            if (_builder.ShouldGenerateMethodsForUnreachableProperties)
            {
                properties = properties.Concat(_entity.GetAllUniqueNotSettablePropertiesWithoutConstructorsParametersMatch());
            }

            var output = new StringBuilder();

            foreach (var typedSymbol in properties)
            {
                if (!IsNotYetDeclaredField(_builder, typedSymbol))
                    continue;
                output.AppendLine($@"        private {GenerateLazyFieldType(typedSymbol)} {typedSymbol.UnderScoreName};");
            }

            foreach (var typedSymbol in properties)
            {
                if (!IsNotYetDeclaredMethod(this, _builder, typedSymbol))
                    continue;
                output.AppendLine($@"

        {GenerateMethodDefinition(typedSymbol)}");

            }

            return output.ToString();

            static bool IsNotYetDeclaredField(in BuilderProperties builder, in TypedSymbol x) => !builder.Fields.TryGetValue(x.UnderScoreName, out _);

            static bool IsNotYetDeclaredMethod(in BuilderSourceStringGenerator generator, in BuilderProperties builder, in TypedSymbol x)
                => !builder.BuildingMethods.TryGetValue(generator.CreateMethodName(x), out var method)
                                 || !(method.Parameters.Length == 1 && method.Parameters[0].Type.Name == x.TypeName);
        }

        private string GenerateMethodDefinition(in TypedSymbol typedSymbol)
            => $@"{GenerateMethodDefinitionHeader(typedSymbol)}
        {{
            {GenerateValueAssigment(typedSymbol)};
            return this;
        }}";

        private string GenerateValueAssigment(in TypedSymbol typedSymbol)
            => typedSymbol.IsMockable
                ? $"{SetupActionLiteral}({typedSymbol.UnderScoreName})"
                : $"{typedSymbol.UnderScoreName} = new Nullbox<{typedSymbol.TypeFullName}>({ValueLiteral})";

        private string CreateMethodName(in TypedSymbol property) => $"{_builder.BuildingMethodsPrefix}{property.SymbolPascalName}";

        private string GenerateMethodDefinitionHeader(in TypedSymbol typedSymbol)
            => $"public {_builder.FullName} {CreateMethodName(typedSymbol)}({GenerateMethodParameterDefinition(typedSymbol)})";

        private string GenerateMethodParameterDefinition(in TypedSymbol typedSymbol)
            => typedSymbol.IsMockable ? $"Action<{CreateMockableFieldType(typedSymbol)}> {SetupActionLiteral}" : $"{typedSymbol.TypeFullName} {ValueLiteral}";

        private string GenerateLazyFieldType(in TypedSymbol typedSymbol)
            => typedSymbol.IsMockable ? CreateMockableFieldType(typedSymbol) : $"Nullbox<{typedSymbol.TypeFullName}>?";

        private string GenerateFieldType(in TypedSymbol typedSymbol)
            => typedSymbol.IsMockable ? CreateMockableFieldType(typedSymbol) : typedSymbol.TypeFullName;

        private string CreateMockableFieldType(in TypedSymbol type) => string.Format(_mockingConfiguration.TypeDeclarationFormat, type.TypeFullName);

        private string GenerateBuildsCode()
        {
            var (parameters, properties) = GetParametersAndProperties();

            var disableWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning disable CS8604\n"
                : string.Empty;
            var restoreWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning restore CS8604\n"
                : string.Empty;

            return $@"{disableWarning}        public {_entity.FullName} Build()
        {{
            {GenerateLazyBuildEntityString(parameters, properties)}
        }}
{restoreWarning}
        public static {_builder.FullName} {_entity.Name} => new {_builder.FullName}();
";

        }
        private string GenerateBuildManyCode()
        {
            return $@"        public System.Collections.Generic.IEnumerable<{_entity.FullName}> BuildMany(int count = 3)
        {{
            return Enumerable.Range(0, count).Select(_ => Build());
        }}
";

        }
        private string GenerateStaticBuildsCode()
        {
            var (parameters, properties) = GetParametersAndProperties();

            var methodParameters = GenerateStaticComaListFieldList(parameters
                .Concat(properties));
            var disableWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning disable CS8625\n"
                : string.Empty;
            var restoreWarning = _builder.NullableStrategy == NullableStrategy.Enabled
                ? "#pragma warning restore CS8625\n"
                : string.Empty;

            return $@"{disableWarning}        public static {_entity.FullName} BuildDefault({methodParameters})
        {{
            {GenerateBuildEntityString(parameters, properties)}
        }}
{restoreWarning}";

        }

        private string GenerateStaticComaListFieldList(IEnumerable<TypedSymbol> symbols)
        {
            List<string> list = new();
            foreach (var s in symbols)
            {
                var fieldType = GenerateFieldType(s);
                list.Add($"{fieldType} {s.UnderScoreName} = default({fieldType})");
            }
            return list.ComaJoin();
        }

        private (IReadOnlyList<TypedSymbol> Parameters, IReadOnlyList<TypedSymbol> Properties) GetParametersAndProperties()
        {
            var parameters = _entity.ConstructorParameters;
            var properties = new List<TypedSymbol>();
            foreach (var x in _entity.SettableProperties)
            {
                if (parameters.ContainsKey(x.SymbolName)) continue;
                properties.Add(x);
            }
            return (parameters.Values.ToList(), properties.ToList());
        }

        private string GenerateLazyBuildEntityString(IReadOnlyCollection<TypedSymbol> parameters, IReadOnlyCollection<TypedSymbol> properties)
        {
            var propertiesAssignment = GenerateLazyPropertiesAssignment(properties);
            return @$"var result = new {_entity.FullName}({GenerateLazyParametersComaList(parameters)})
            {{
{(string.IsNullOrEmpty(propertiesAssignment) ? string.Empty : $"                {propertiesAssignment}")}
            }};
            {(_builder.ShouldGenerateMethodsForUnreachableProperties ? GenerateUnreachableProperties(this) : "")}
            PostBuild(result);
            return result;";

            static string GenerateUnreachableProperties(BuilderSourceStringGenerator generator)
            {
                var output = new StringBuilder();
                output.AppendLine($"var t = typeof({generator._entity.FullName});");
                foreach (var a in generator._entity.GetAllUniqueNotSettablePropertiesWithoutConstructorsParametersMatch())
                {
                    output.AppendLine($"t.GetProperty(\"{a.SymbolName}\").SetValue(result, {generator.GenerateLazyFieldValueReturn(a)}, System.Reflection.BindingFlags.NonPublic, null, null, null);");
                }
                return output.ToString();
            }
        }

        private string GenerateLazyParametersComaList(IReadOnlyCollection<TypedSymbol> parameters)
        {
            List<string> list = new(parameters.Count);
            foreach (var property in parameters)
            {
                list.Add(GenerateLazyFieldValueReturn(property));
            }
            return list.ComaJoin();
        }

        private string GenerateLazyPropertiesAssignment(IReadOnlyCollection<TypedSymbol> properties)
        {
            List<string> list = new(properties.Count);
            foreach (var property in properties)
            {
                list.Add($"{property.SymbolName} = {GenerateLazyFieldValueReturn(property)}");
            }
            return list.ComaJoin();
        }

        private string GenerateBuildEntityString(IReadOnlyCollection<TypedSymbol> parameters, IReadOnlyCollection<TypedSymbol> properties)
        {
            var propertiesAssignment = GeneratePropertiesAssignment(properties);
            return @$"return new {_entity.FullName}({GenerateParametersComaList(parameters)})
            {{
{(string.IsNullOrEmpty(propertiesAssignment) ? string.Empty : $"                {propertiesAssignment}")}
            }};";
        }

        private string GenerateParametersComaList(IReadOnlyCollection<TypedSymbol> parameters)
        {
            List<string> list = new(parameters.Count);
            foreach (var property in parameters)
            {
                list.Add(GenerateFieldValueReturn(property));
            }
            return list.ComaJoin();
        }

        private string GeneratePropertiesAssignment(IReadOnlyCollection<TypedSymbol> properties)
        {
            List<string> list = new(properties.Count);
            foreach (var property in properties)
            {
                list.Add($"{property.SymbolName} = {GenerateFieldValueReturn(property)}");
            }
            return list.ComaJoin();
        }

        private string GenerateLazyFieldValueReturn(in TypedSymbol typedSymbol)
            => typedSymbol.IsMockable
                ? string.Format(_mockingConfiguration.ReturnObjectFormat, typedSymbol.UnderScoreName)
                : @$"({typedSymbol.UnderScoreName}.HasValue ? {typedSymbol.UnderScoreName}.Value : new Nullbox<{typedSymbol.TypeFullName}>({(typedSymbol.IsFakeable
                    ? $"{string.Format(_fixtureConfiguration.CreateSingleFormat, typedSymbol.TypeFullName, typedSymbol.SymbolName, FixtureLiteral)}"
                    : $"default({typedSymbol.TypeFullName})")})).Object";

        private string GenerateFieldValueReturn(in TypedSymbol typedSymbol)
            => typedSymbol.IsMockable
                ? string.Format(_mockingConfiguration.ReturnObjectFormat, typedSymbol.UnderScoreName)
                : typedSymbol.UnderScoreName;

        private const string AutoGenerationComment = @"
// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a source generator named Buildenator (https://github.com/progala2/Buildenator)
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------";
    }

}