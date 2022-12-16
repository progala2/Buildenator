using System;
using Buildenator.Abstraction;
using Buildenator.CodeAnalysis;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Buildenator.Configuration
{
    internal readonly ref struct EntityToBuild
    {
        public string ContainingNamespace { get; }
        public string Name { get; }
        public string FullName { get; }
        public string FullNameWithConstraints { get; }
        public IReadOnlyDictionary<string, TypedSymbol> ConstructorParameters { get; }
        public ReadOnlySpan<TypedSymbol> SettableProperties { get; }
        public ReadOnlySpan<TypedSymbol> UnsettableProperties { get; }
        public ReadOnlySpan<string> AdditionalNamespaces { get; }

        public EntityToBuild(INamedTypeSymbol typeForBuilder, in MockingProperties mockingConfiguration, in FixtureProperties fixtureConfiguration)
        {
            INamedTypeSymbol? entityToBuildSymbol;
            var additionalNamespaces = Enumerable.Empty<string>();
            if (typeForBuilder.IsGenericType)
            {
                entityToBuildSymbol = typeForBuilder.ConstructedFrom;
                additionalNamespaces = entityToBuildSymbol.TypeParameters.Where(a => a.ConstraintTypes.Any())
                    .SelectMany(a => a.ConstraintTypes).Select(a => a.ContainingNamespace.ToDisplayString())
                    .ToArray();
            }
            else
            {
                entityToBuildSymbol = typeForBuilder;
            }
            ContainingNamespace = entityToBuildSymbol.ContainingNamespace.ToDisplayString();
            AdditionalNamespaces = additionalNamespaces.Concat(new[] { ContainingNamespace }).ToArray();
            Name = entityToBuildSymbol.Name;
            FullName = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
            FullNameWithConstraints = entityToBuildSymbol.ToDisplayString(new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance));
            _mockingConfiguration = mockingConfiguration;
            _fixtureConfiguration = fixtureConfiguration;
            var constructorParameters = 
                ConstructorParameters = GetConstructorParameters(entityToBuildSymbol);
            ConstructorParameters = constructorParameters;

            var twoSets = DividePropertiesBySetability(entityToBuildSymbol, mockingConfiguration, fixtureConfiguration.Strategy);
            UnsettableProperties = twoSets.NotSettable;
            SettableProperties = twoSets.Settable;
            var uniqueTypedSymbols = new List<TypedSymbol>();
            foreach (var x in SettableProperties)
            {
                if (constructorParameters.ContainsKey(x.SymbolName)) continue;
                uniqueTypedSymbols.Add(x);
            }

            uniqueTypedSymbols.AddRange(ConstructorParameters.Values);
            _uniqueTypedSymbols = uniqueTypedSymbols.ToArray();
            
            var uniqueUnsettableTypedSymbols = new List<TypedSymbol>();
            foreach (var x in UnsettableProperties)
            {
                if (constructorParameters.ContainsKey(x.SymbolName)) continue;
                uniqueUnsettableTypedSymbols.Add(x);
            }

            _uniqueUnsettableTypedSymbols = uniqueUnsettableTypedSymbols.ToArray();
        }

        public ReadOnlySpan<TypedSymbol> GetAllUniqueSettablePropertiesAndParameters()
        {
            return _uniqueTypedSymbols.ToArray();
        }

        public ReadOnlySpan<TypedSymbol> GetAllUniqueNotSettablePropertiesWithoutConstructorsParametersMatch()
        {
            return _uniqueUnsettableTypedSymbols.ToArray();
        }

        private IReadOnlyDictionary<string, TypedSymbol> GetConstructorParameters(INamedTypeSymbol entityToBuildSymbol)
        {
            var parameterSymbols = entityToBuildSymbol.Constructors.OrderByDescending(x => x.Parameters.Length).First().Parameters;
            var dict = new Dictionary<string, TypedSymbol>();
            foreach (var parameter in parameterSymbols)
            {
                dict.Add(parameter.PascalCaseName(), new TypedSymbol(parameter, _mockingConfiguration, _fixtureConfiguration.Strategy));
            }

            return dict;
        }

        private static TwoSets DividePropertiesBySetability(
            INamedTypeSymbol entityToBuildSymbol, in MockingProperties mockingConfiguration, FixtureInterfacesStrategy fixtureConfiguration)
        {
            var properties = entityToBuildSymbol.DividePublicPropertiesBySetability();
            var settable = new List<TypedSymbol>();
            foreach (var a in properties.Settable)
            {
                settable.Add(new TypedSymbol(a, mockingConfiguration, fixtureConfiguration));
            }
            var unsettable = new List<TypedSymbol>();
            foreach (var a in properties.NotSettable)
            {
                unsettable.Add(new TypedSymbol(a, mockingConfiguration, fixtureConfiguration));
            }
            return new TwoSets(settable.ToArray(), unsettable.ToArray());
        }

        private readonly ReadOnlySpan<TypedSymbol> _uniqueTypedSymbols;
        private readonly ReadOnlySpan<TypedSymbol> _uniqueUnsettableTypedSymbols;
        private readonly MockingProperties _mockingConfiguration;
        private readonly FixtureProperties _fixtureConfiguration;

        private readonly ref struct TwoSets
        {
            public TwoSets(ReadOnlySpan<TypedSymbol> settable, ReadOnlySpan<TypedSymbol> notSettable)
            {
                Settable = settable;
                NotSettable = notSettable;
            }

            public ReadOnlySpan<TypedSymbol> Settable { get; }
            public ReadOnlySpan<TypedSymbol> NotSettable { get; }
        }
    }
}