using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Buildenator.Configuration
{
    internal readonly struct BuilderProperties
    {
        private readonly Dictionary<string, IMethodSymbol> _buildingMethods;
        private readonly Dictionary<string, IFieldSymbol> _fields;

        public BuilderProperties(INamedTypeSymbol builderSymbol, in MakeBuilderAttributeInternal attributeData)
        {
            ContainingNamespace = builderSymbol.ContainingNamespace.ToDisplayString();
            Name = builderSymbol.Name;
            FullName = builderSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
            BuildingMethodsPrefix = attributeData.BuildingMethodsPrefix ?? DefaultConstants.BuildingMethodsPrefix;
            NullableStrategy = attributeData.NullableStrategy ?? NullableStrategy.Default;
            StaticCreator = attributeData.DefaultStaticCreator ?? true;
            ShouldGenerateMethodsForUnreachableProperties = attributeData.GenerateMethodsForUnreachableProperties ?? false;

            if (string.IsNullOrWhiteSpace(BuildingMethodsPrefix))
                throw new ArgumentNullException(nameof(attributeData), "Prefix name shouldn't be empty!");

            _buildingMethods = new Dictionary<string, IMethodSymbol>();
            _fields = new Dictionary<string, IFieldSymbol>();
            var members = builderSymbol.GetMembers();
            foreach (var member in members)
            {
                if (member is not IMethodSymbol method)
                {
                    if (member is not IFieldSymbol field) continue;

                    _fields.Add(field.Name, field);

                    continue;
                }

                if (method.Name.StartsWith(BuildingMethodsPrefix))
                    _buildingMethods.Add(method.Name, method);
                else if (method.Name == DefaultConstants.PostBuildMethodName)
                    IsPostBuildMethodOverriden = true;
                else if (method is { MethodKind: MethodKind.Constructor, Parameters.Length: 0, IsImplicitlyDeclared: false })
                    IsDefaultContructorOverriden = true;
            }
        }

        public string ContainingNamespace { get; }
        public string Name { get; }
        public string FullName { get; }
        public string BuildingMethodsPrefix { get; }
        public NullableStrategy NullableStrategy { get; }
        public bool StaticCreator { get; }
        public bool IsPostBuildMethodOverriden { get; }
        public bool IsDefaultContructorOverriden { get; }
        public bool ShouldGenerateMethodsForUnreachableProperties { get; }

        public IReadOnlyDictionary<string, IMethodSymbol> BuildingMethods => _buildingMethods;
        public IReadOnlyDictionary<string, IFieldSymbol> Fields => _fields;

    }
}