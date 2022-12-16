﻿using System;
using System.Collections.Generic;
using Buildenator.Abstraction;
using Buildenator.Configuration;
using Buildenator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Buildenator
{
    [Generator]
    public class BuildersGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Debugger.Launch();
            var classSymbols = GetBuilderSymbolAndItsAttribute(context);

            var compilation = context.Compilation;
            var assembly = compilation.Assembly;
            var fixtureConfigurationBuilder = new FixturePropertiesBuilder(assembly);
            var mockingConfigurationBuilder = new MockingPropertiesBuilder(assembly);
            var builderPropertiesBuilder = new BuilderPropertiesBuilder(assembly);

            foreach (var a in classSymbols)
            {
                var builder = a.Builder;
                var attribute = a.Attribute;
                var mockingConfiguration = mockingConfigurationBuilder.Build(builder);
                var fixtureConfiguration = fixtureConfigurationBuilder.Build(builder);
                var generator = new BuilderSourceStringGenerator(
                builderPropertiesBuilder.Build(builder, attribute),
                new EntityToBuild(attribute.TypeForBuilder, mockingConfiguration, fixtureConfiguration),
                    fixtureConfiguration,
                    mockingConfiguration);

                context.AddSource($"{builder.Name}.cs", SourceText.From(generator.CreateBuilderCode(), Encoding.UTF8));

                if (context.CancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private static ReadOnlySpan<BuilderAttributePair>
            GetBuilderSymbolAndItsAttribute(GeneratorExecutionContext context)
        {
            var result = new List<BuilderAttributePair>();

            var compilation = context.Compilation;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var classesSyntaxes = syntaxTree.GetRoot(context.CancellationToken).DescendantNodesAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c =>
                        c.AttributeLists.SelectMany(a => a.Attributes)
                        .Any(a => a.Name.ToString() == nameof(MakeBuilderAttribute))).ToArray();

                if (!classesSyntaxes.Any()) continue;

                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                foreach (var classSyntax in classesSyntaxes)
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax, context.CancellationToken);
                    if (classSymbol is not { })
                        continue;

                    var attribute = classSymbol.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Name == nameof(MakeBuilderAttribute));
                    if (attribute is null)
                        continue;

                    var makeBuilderAttribute = CreateMakeBuilderAttributeInternal(attribute);

                    if (makeBuilderAttribute.TypeForBuilder.IsAbstract)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(AbstractDiagnostic, classSymbol.Locations.First(), classSymbol.Name));
                        continue;
                    }

                    result.Add(new BuilderAttributePair(classSymbol, makeBuilderAttribute));
                }
            }

            return result.ToArray();
        }

        private static MakeBuilderAttributeInternal CreateMakeBuilderAttributeInternal(AttributeData attribute)
        {
            return new MakeBuilderAttributeInternal(
                (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!,
                (string?)attribute.ConstructorArguments[1].Value,
                (bool?)attribute.ConstructorArguments[2].Value,
                attribute.ConstructorArguments[3].Value is null
                    ? null
                    : (NullableStrategy)attribute.ConstructorArguments[3].Value!,
                (bool?)attribute.ConstructorArguments[4].Value);
        }

        private readonly struct BuilderAttributePair
        {
            public BuilderAttributePair(INamedTypeSymbol builder, MakeBuilderAttributeInternal attribute)
            {
                Builder = builder;
                Attribute = attribute;
            }

            public INamedTypeSymbol Builder { get; }
            public MakeBuilderAttributeInternal Attribute { get; }
        }

        private static readonly DiagnosticDescriptor AbstractDiagnostic = new("BDN001", "Cannot generate a builder for an abstract class", "Cannot generate a builder for the {0} abstract class", "Buildenator", DiagnosticSeverity.Error, true);
    }
}