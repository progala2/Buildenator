using System.Text;
using Buildenator.Configuration;

namespace Buildenator.Generators
{
    internal static class ConstructorsGenerator
    {
        internal static string GenerateConstructor(
            string builderName,
            in EntityToBuild entity,
            in FixtureProperties? fixtureConfiguration)
        {
            var hasAnyBody = false;
            var parameters = entity.GetAllUniqueSettablePropertiesAndParameters();

            var output = new StringBuilder();
            output.AppendLine($@"{CommentsGenerator.GenerateSummaryOverrideComment()}
        public {builderName}()
        {{");
            foreach (var typedSymbol in parameters)
            {
                if (!typedSymbol.NeedsFieldInit()) continue;

                output.AppendLine($@"            {typedSymbol.GenerateFieldInitialization()}");
                hasAnyBody = true;
            }

            if (fixtureConfiguration is not null && fixtureConfiguration.Value.NeedsAdditionalConfiguration())
            {
                output.AppendLine($@"            {fixtureConfiguration.Value.GenerateAdditionalConfiguration()};");
                hasAnyBody = true;
            }

            output.AppendLine($@"
        }}");

            return hasAnyBody ? output.ToString() : string.Empty;
        }
    }
}