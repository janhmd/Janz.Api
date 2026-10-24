namespace Service.Helpers.DTOs.Cart
{
    public class AddCartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Variantı olan məhsulda mütləqdir; üyütmə yalnız paket üçün (Domain/Constants/GrindOptions)
        public int? VariantId { get; set; }
        public string? Grind { get; set; }
    }
}
