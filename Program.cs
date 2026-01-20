using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<JobRankingSystem.Data.AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity Configuration
builder.Services.AddIdentity<JobRankingSystem.Models.ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<JobRankingSystem.Data.AppDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_key_123456789_at_least_32_chars";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "JobRankingSystem",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "JobRankingSystemUser",
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddMemoryCache(); // Register IMemoryCached<JobRankingSystem.Services.AVLTreeService>();

// DSA Services Registration
builder.Services.AddScoped<JobRankingSystem.Services.MaxHeapService>();
builder.Services.AddScoped<JobRankingSystem.Services.AVLTreeService>();
builder.Services.AddScoped<JobRankingSystem.Services.HashIndexService>();
builder.Services.AddScoped<JobRankingSystem.Services.TrieService>();
builder.Services.AddScoped<JobRankingSystem.Services.GraphService>();
builder.Services.AddScoped<JobRankingSystem.Services.DPFitScoreService>();
builder.Services.AddScoped<JobRankingSystem.Services.KMPService>();
builder.Services.AddScoped<JobRankingSystem.Services.SortingService>();
builder.Services.AddScoped<JobRankingSystem.Services.GreedySelectionService>();
builder.Services.AddScoped<JobRankingSystem.Repositories.ICandidateRepository, JobRankingSystem.Repositories.CandidateRepository>();

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // context.Database.EnsureCreated() is useful for in-memory or quick prototypes without migrations
        var context = services.GetRequiredService<JobRankingSystem.Data.AppDbContext>();
        // context.Database.EnsureCreated(); // Skipping auto-mig for now as connection string might be invalid.
        // Actually, to make it work out of the box for the user, I should wrap this. 
        // But the walkthrough says "It will automatically attempt to create". 
        // I'll leave it but catch the exception gracefully so app runs even if DB is down.
        // In the catch block I already log error. So it shouldn't crash the APP, it just fails to seed.
        
        // context.Database.EnsureCreated(); 
        await context.Database.MigrateAsync(); 
        await JobRankingSystem.SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB (Check connection string).");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

