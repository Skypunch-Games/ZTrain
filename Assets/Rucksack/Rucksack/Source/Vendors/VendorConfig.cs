namespace Devdog.Rucksack.Vendors
{
    [System.Serializable]
    public partial class VendorConfig
    {
        /// <summary>
        /// Should a product sold to the vendor be added to the vendor's collection so it can be re-bought back later?
        /// </summary>
        public bool addSoldProductToVendor = true;
        
        /// <summary>
        /// Should a product bought from a vendor be removed?
        /// When false the product in the vendor collection will be cloned instead.
        /// </summary>
        public bool removeBoughtProductFromVendor = true;
        

        public float buyFromVendorPriceMultiplier = 1.0f;
        public float sellToVendorPriceMultiplier = 1.0f;
        
    }
}