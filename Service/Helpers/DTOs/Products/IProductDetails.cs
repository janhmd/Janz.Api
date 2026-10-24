namespace Service.Helpers.DTOs.Products
{
    // Yaratma və dəyişmə formalarının ortaq sahələri: qaydaları bir dəfə yazmaq üçün (ProductDetailsValidator)
    public interface IProductDetails
    {
        decimal Price { get; }
        decimal? OldPrice { get; }
        string? Badge { get; }
        string? Roast { get; }
        string? Region { get; }
        string? Processing { get; }
        string? Caffeine { get; }
        string? Brewing { get; }
        string? Storage { get; }
    }
}
