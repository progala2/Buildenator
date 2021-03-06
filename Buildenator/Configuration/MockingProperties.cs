using Buildenator.Abstraction;
using Buildenator.Configuration.Contract;

namespace Buildenator.Configuration
{
    internal sealed class MockingProperties : IMockingProperties
    {
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

    }
}
