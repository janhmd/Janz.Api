using System.Globalization;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Janz.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Repository.Data;
using Service;

var builder = WebApplication.CreateBuilder(args);

// Formdan gələn qiymət "29.90" kimi nöqtə ilə gəlir: kompüterin dili (az-AZ, vergül) onu səhv oxumasın
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Janz API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the token from register or login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Token hələ vaxtı bitməyibsə də, istifadəçi silinib və ya bloklanıbsa sorğu qəbul olunmur
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
                var user = userId is null ? null : await userManager.FindByIdAsync(userId);

                if (user is null || await userManager.IsLockedOutAsync(user))
                {
                    context.Fail("The account is blocked or was removed.");
                    return;
                }

                // Rollar bazadan təzədən götürülür: SuperAdmin rolu dəyişəndə yenidən daxil olmadan qüvvəyə minir
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    foreach (var claim in identity.FindAll(ClaimTypes.Role).ToList())
                    {
                        identity.RemoveClaim(claim);
                    }

                    foreach (var role in await userManager.GetRolesAsync(user))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
        };
    });

builder.Services.AddRepositoryLayer();
builder.Services.AddServiceLayer();

var mvcOrigins = builder.Configuration.GetSection("Frontend:MvcOrigins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(mvcOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);
    await SiteSeeder.SeedAsync(db);

    // İşçi hesabları (appsettings -> SeedAdmins): ilk SuperAdmin buradan yaranır
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var seedAdmins = app.Configuration.GetSection("SeedAdmins").Get<List<SeedAdminSettings>>() ?? new List<SeedAdminSettings>();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
    await AdminSeeder.SeedAsync(userManager, seedAdmins, seedLogger);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Admin paneldən yüklənən şəkillər (wwwroot/uploads/...) buradan verilir
app.UseStaticFiles();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
