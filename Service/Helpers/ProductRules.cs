namespace Service.Helpers
{
    // Admin paneldə məhsul üçün ümumi limitlər
    public static class ProductRules
    {
        public const int MaxGallery = 6;          // qalereyada ən çox şəkil
        public const string ImageFolder = "products";
        public const int MaxVariants = 8;         // bir məhsulda ən çox variant
        public const int MaxVariantImages = 6;    // bir variantda ən çox şəkil
    }
}
