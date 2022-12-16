namespace Buildenator.Abstraction
{
    public enum FixtureInterfacesStrategy
    {
        /// Needs to change the name for the strategy to only about interfaces
        /// It means that nothing will be faked, not only interfaces
        Null = -1,
        None = 0,
        All = 1,
        OnlyGenericCollections = 2
    }
}
