﻿using System;
// ReSharper disable UnusedParameter.Local

namespace Buildenator.Abstraction;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class BuildenatorConfigurationAttribute : Attribute
{
    /// <summary>
    /// An assembly attribute to configure globally the buildenator generation
    /// </summary>
    /// <param name="buildingMethodsPrefix">How the builder methods should be named.</param>
    /// <param name="defaultStaticCreator">The resulting builder will have a special static building method with default parameters.</param>
    /// <param name="nullableStrategy">Change nullable context behaviour.</param>
    /// <param name="generateMethodsForUnreachableProperties"></param>
    /// <param name="implicitCast">Should the builder have implicit cast to the target type.</param>
    public BuildenatorConfigurationAttribute(
        string buildingMethodsPrefix = "With",
        bool defaultStaticCreator = true,
        NullableStrategy nullableStrategy = NullableStrategy.Default,
        bool generateMethodsForUnreachableProperties = false,
        bool implicitCast = false)
    {
        }
}