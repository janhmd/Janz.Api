var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
