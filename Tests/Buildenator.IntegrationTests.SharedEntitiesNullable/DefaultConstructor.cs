﻿namespace Buildenator.IntegrationTests.SharedEntitiesNullable
{
    public class DefaultConstructor
    {
        public DefaultConstructor(string stringEntry)
        {
            StringEntry = stringEntry;
        }

        public string StringEntry { get; }
        public int Entry { get; set; }
    }
}
