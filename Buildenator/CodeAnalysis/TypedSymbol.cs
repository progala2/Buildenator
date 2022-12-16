using Buildenator.Abstraction;
using Buildenator.Extensions;
using Microsoft.CodeAnalysis;
using System.Linq;
using Buildenator.Configuration;

namespace Buildenator.CodeAnalysis
{
    internal readonly struct TypedSymbol
    {
        public TypedSymbol(ISymbol symbol, ITypeSymbol type, in MockingProperties mockingInterfaceStrategy,
            FixtureInterfacesStrategy fixtureConfiguration)
        {
            Symbol = symbol;
            Type = type;
            _mockingProperties = mockingInterfaceStrategy;
            UnderScoreName ??= Symbol.UnderScoreName();
            TypeFullName ??= Type.ToDisplayString();
            IsMockable = _mockingProperties.Strategy switch
            {
                MockingInterfacesStrategy.All
                    when Type.TypeKind == TypeKind.Interface => true,
                MockingInterfacesStrategy.WithoutGenericCollection
                    when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => true,
                _ => false
            };

            IsFakeable = fixtureConfiguration switch
            {
                FixtureInterfacesStrategy.Null => false,
                FixtureInterfacesStrategy.None
                    when Type.TypeKind == TypeKind.Interface => false,
                FixtureInterfacesStrategy.OnlyGenericCollections
                    when Type.TypeKind == TypeKind.Interface && Type.AllInterfaces.All(x => x.SpecialType != SpecialType.System_Collections_IEnumerable) => false,
                _ => true
            };
        }
        public TypedSymbol(IPropertySymbol symbol, in MockingProperties mockingInterfaceStrategy, FixtureInterfacesStrategy fixtureConfiguration)
        : this(symbol, symbol.Type, mockingInterfaceStrategy, fixtureConfiguration)
        {
        }

        public TypedSymbol(IParameterSymbol symbol, in MockingProperties mockingInterfaceStrategy, FixtureInterfacesStrategy fixtureConfiguration)
            : this(symbol, symbol.Type, mockingInterfaceStrategy, fixtureConfiguration)
        {
        }

        internal bool NeedsFieldInit() => IsMockable;

        private ISymbol Symbol { get; }
        private ITypeSymbol Type { get; }

        public string UnderScoreName { get; }
        public string TypeFullName { get; }

        public string TypeName => Type.Name;

        public string SymbolPascalName => Symbol.PascalCaseName();
        public string SymbolName => Symbol.Name;

        private readonly MockingProperties _mockingProperties;
        public bool IsMockable { get; }
        public bool IsFakeable { get; }

        public string GenerateFieldInitialization()
            => $"{UnderScoreName} = {string.Format(_mockingProperties.FieldDeafultValueAssigmentFormat, TypeFullName)};";
    }
}