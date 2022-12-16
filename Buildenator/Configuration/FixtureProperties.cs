using Buildenator.Abstraction;
using Buildenator.Generators;

namespace Buildenator.Configuration
{
    internal readonly struct FixtureProperties : IAdditionalNamespacesProvider
    {
        private const string FixtureLiteral = "_fixture";

        public static readonly FixtureProperties Empty =
            new (null!, null!, null!, null!, FixtureInterfacesStrategy.Null, null!);

        public FixtureProperties(
            string name,
            string createSingleFormat,
            string? constructorParameters,
            string? additionalConfiguration,
            FixtureInterfacesStrategy strategy,
            string[] additionalNamespaces)
        {
            Name = name;
            CreateSingleFormat = createSingleFormat;
            ConstructorParameters = constructorParameters;
            AdditionalConfiguration = additionalConfiguration;
            Strategy = strategy;
            AdditionalNamespaces = additionalNamespaces;
        }

        public string Name { get; }
        public string CreateSingleFormat { get; }
        public string? ConstructorParameters { get; }
        public string? AdditionalConfiguration { get; }
        public FixtureInterfacesStrategy Strategy { get; }
        public string[] AdditionalNamespaces { get; }

        public string GenerateAdditionalConfiguration()
            => AdditionalConfiguration is null ? string.Empty : string.Format(AdditionalConfiguration, FixtureLiteral, Name);

        public bool NeedsAdditionalConfiguration() => AdditionalConfiguration is not null;

        public bool IsEmpty => Strategy == FixtureInterfacesStrategy.Null;
    }
}
