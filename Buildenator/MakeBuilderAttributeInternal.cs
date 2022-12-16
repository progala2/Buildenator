﻿using Buildenator.Abstraction;
using Microsoft.CodeAnalysis;

namespace Buildenator
{
    internal readonly struct MakeBuilderAttributeInternal
    {
        public MakeBuilderAttributeInternal(
            INamedTypeSymbol typeForBuilder, string? buildingMethodsPrefix, bool? staticCreator, NullableStrategy? nullableStrategy, bool? generateMethodsForUnreachableProperties)
        {
            TypeForBuilder = typeForBuilder;
            BuildingMethodsPrefix = buildingMethodsPrefix;
            DefaultStaticCreator = staticCreator;
            NullableStrategy = nullableStrategy;
            GenerateMethodsForUnreachableProperties = generateMethodsForUnreachableProperties;
        }

        public INamedTypeSymbol TypeForBuilder { get; }
        public string? BuildingMethodsPrefix { get; }
        public bool? DefaultStaticCreator { get; }
        public NullableStrategy? NullableStrategy { get; }
        public bool? GenerateMethodsForUnreachableProperties { get; }
    }
}