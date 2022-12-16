using Buildenator.Abstraction;
using Buildenator.Generators;

namespace Buildenator.Configuration
{
    internal readonly struct MockingProperties : IAdditionalNamespacesProvider
    {
        
        public static readonly MockingProperties Empty = new (MockingInterfacesStrategy.Null, null!, null!, null!, null!);

        public MockingProperties(
            MockingInterfacesStrategy strategy,
            string typeDeclarationFormat,
            string fieldDeafultValueAssigmentFormat,
            string returnObjectFormat,
            string[] additionalNamespaces)
        {
            Strategy = strategy;
            TypeDeclarationFormat = typeDeclarationFormat;
            FieldDeafultValueAssigmentFormat = fieldDeafultValueAssigmentFormat;
            ReturnObjectFormat = returnObjectFormat;
            AdditionalNamespaces = additionalNamespaces;
        }

        public MockingInterfacesStrategy Strategy { get; }
        public string TypeDeclarationFormat { get; }
        public string FieldDeafultValueAssigmentFormat { get; }
        public string ReturnObjectFormat { get; }
        public string[] AdditionalNamespaces { get; }

        public bool IsEmpty => Strategy == MockingInterfacesStrategy.Null;

    }
}
