﻿using System;
// ReSharper disable UnusedParameter.Local

namespace Buildenator.Abstraction;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false)]
public abstract class FixtureConfigurationAttribute : Attribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fixtureTypeName"></param>
    /// <param name="strategy"></param>
    /// <param name="additionalNamespaces">List all the additional namespaces that are important for the fixture; separate them by comma ','. 
    /// An example: "Namespace1,Namespace2.Subspace"</param>
    /// <param name="constructorParameters"></param>
    /// <param name="additionalConfiguration">You can make additional configuration of your fixture instance. 
    /// {0} is for the fixture object's name.
    /// {1} is for the fixture object's type.</param>
    /// <param name="createSingleFormat">It is used to configure how each property will be generated by your fixture.
    /// {0} is for the type of a property.
    /// {1} is for the name of a property.
    /// {2} is for the fixture object's name.</param>
    public FixtureConfigurationAttribute(
        string fixtureTypeName,
        string createSingleFormat,
        string? constructorParameters = null,
        string? additionalConfiguration = null,
        FixtureInterfacesStrategy strategy = FixtureInterfacesStrategy.OnlyGenericCollections,
        string? additionalNamespaces = null)
    {
            Strategy = strategy;
            AdditionalNamespaces = additionalNamespaces?.Split(',') ?? Array.Empty<string>();
        }

    public FixtureInterfacesStrategy Strategy { get; }
    public string[] AdditionalNamespaces { get; }
}