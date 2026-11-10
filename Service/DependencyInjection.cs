using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Service.Helpers;
using Service.Helpers.Validators.Accounts;
using Service.Services;
using Service.Services.Interfaces;

namespace Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services)
        {
            services.AddScoped<JwtTokenGenerator>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IRefundService, RefundService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IUserAdminService, UserAdminService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IAdminNotificationService, AdminNotificationService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IDiscountLevelService, DiscountLevelService>();
            services.AddScoped<IWishlistService, WishlistService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IBalanceService, BalanceService>();
            services.AddScoped<IGiftCertificateService, GiftCertificateService>();
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICommunityAuthorService, CommunityAuthorService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IPostCommentService, PostCommentService>();
            services.AddScoped<ICommunityProfileService, CommunityProfileService>();
            services.AddScoped<ICommunityAdminService, CommunityAdminService>();
            services.AddScoped<IAssistantService, AssistantService>();
            services.AddScoped<IAdminAssistantService, AdminAssistantService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<ISiteTileService, SiteTileService>();
            services.AddHttpClient();
            services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
            return services;
        }
    }
}
