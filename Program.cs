using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOX_Backend.Models;
using NOX_Backend.Services;
using Azure.Identity;
using Azure.Storage.Blobs;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file only in development
if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Azure Blob Storage
var azureStorageAccountName = Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_NAME")
    ?? throw new InvalidOperationException("AZURE_STORAGE_ACCOUNT_NAME environment variable is not set.");

var blobServiceUri = new Uri($"https://{azureStorageAccountName}.blob.core.windows.net");
var tokenCredential = new DefaultAzureCredential();

builder.Services.AddSingleton(new BlobServiceClient(blobServiceUri, tokenCredential));
builder.Services.AddScoped(provider =>
{
    var blobServiceClient = provider.GetRequiredService<BlobServiceClient>();
    var logger = provider.GetRequiredService<ILogger<AzureBlobStorageService>>();
    const string containerName = "onboarding-materials";
    return new AzureBlobStorageService(blobServiceClient, containerName, logger);
});

// Configure HttpClient and AI Document Service
builder.Services.AddHttpClient<AiDocumentService>()
    .ConfigureHttpClient((serviceProvider, httpClient) =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(60);
    });

// Configure Identity with cookie-based authentication
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    // Configure password hashing to use PBKDF2 with a compatible iteration count
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

// Configure password hasher to use PBKDF2 only (compatible mode)
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
});

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/authentication/login";
    options.LogoutPath = "/api/authentication/logout";
    options.AccessDeniedPath = "/api/authentication/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<RoleSeederService>();
builder.Services.AddScoped<DepartmentSeederService>();

// Configure CORS for development (React/Vite frontend)
if (builder.Environment.IsDevelopment())
{
    var corsConfig = builder.Configuration.GetSection("Cors");
    var allowedOrigins = corsConfig.GetSection("AllowedOrigins").Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCorsPolicy", policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "NOX Main Backend";
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed roles, departments, and default SuperAdmin user
using (var scope = app.Services.CreateScope())
{
    try
    {
        var departmentSeeder = scope.ServiceProvider.GetRequiredService<DepartmentSeederService>();
        await departmentSeeder.SeedAsync();

        var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeederService>();
        await roleSeeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

// Apply CORS policy for development
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCorsPolicy");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
