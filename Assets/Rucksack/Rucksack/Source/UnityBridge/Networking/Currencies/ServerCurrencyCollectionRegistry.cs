namespace Devdog.Rucksack.Currencies
{
    public static class ServerCurrencyCollectionRegistry
    {
        private static CurrencyCollectionRegistry.CollectionRegisteryHelper<System.Guid, ICurrencyCollection> _idCols = new CurrencyCollectionRegistry.CollectionRegisteryHelper<System.Guid, ICurrencyCollection>();
//        private static CurrencyCollectionRegistry.CollectionRegisteryHelper<string, ICurrencyCollection> _nameCols = new CurrencyCollectionRegistry.CollectionRegisteryHelper<string, ICurrencyCollection>();

        public static CurrencyCollectionRegistry.CollectionRegisteryHelper<System.Guid, ICurrencyCollection> byID
        {
            get { return _idCols; }
        }

//        public static CurrencyCollectionRegistry.CollectionRegisteryHelper<string, ICurrencyCollection> byName
//        {
//            get { return _nameCols; }
//        }
    }
}