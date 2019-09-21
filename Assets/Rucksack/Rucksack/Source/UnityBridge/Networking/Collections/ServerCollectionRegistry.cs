namespace Devdog.Rucksack.Collections
{
    public static class ServerCollectionRegistry
    {
        private static CollectionRegistry.CollectionRegisteryHelper<System.Guid, ICollection> _idCols = new CollectionRegistry.CollectionRegisteryHelper<System.Guid, ICollection>();
//        private static CollectionRegistry.CollectionRegisteryHelper<string, ICollection> _nameCols = new CollectionRegistry.CollectionRegisteryHelper<string, ICollection>();

        public static CollectionRegistry.CollectionRegisteryHelper<System.Guid, ICollection> byID
        {
            get { return _idCols; }
        }

//        public static CollectionRegistry.CollectionRegisteryHelper<string, ICollection> byName
//        {
//            get { return _nameCols; }
//        }
    }
}