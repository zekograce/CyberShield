using CyberShield.API.Data;
using CyberShield.API.Models;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpClient();

// 3. Services
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();

// 4. JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CyberShield API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token here"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var db = services.GetRequiredService<ApplicationDbContext>();

    // Roles
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Admin user
    var adminEmail = "superadmin@technocrypt.com";
    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new ApplicationUser
        {
            UserName = "superadmin",
            Email = adminEmail,
            FullName = "Super Admin",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        var result = await userManager.CreateAsync(admin, "AdminSecure123!@#");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            await userManager.AddToRoleAsync(admin, "User");
        }
    }

    // Packages seed
    if (!db.Packages.Any())
    {
        var packages = new List<Package>
        {
            new Package
            {
                Name = "Basic",
                Description = "Essential cybersecurity tools for individuals.",
                CurrentPrice = 149,
                OriginalPrice = 149,
                Currency = "EGP",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Features = new List<PackageFeature>
                {
                    new() { FeatureKey = "MAX_FILES_PER_MONTH", Name = "File Scans Per Month", Value = "100", DisplayOrder = 1 },
                    new() { FeatureKey = "LINK_SCANNING", Name = "Link Scanning", Value = "Unlimited", DisplayOrder = 2 },
                    new() { FeatureKey = "PASSWORD_GENERATOR", Name = "Password Generator", Value = "true", DisplayOrder = 3 },
                    new() { FeatureKey = "SECURITY_TIPS", Name = "Security Tips", Value = "Daily", DisplayOrder = 4 },
                    new() { FeatureKey = "SUPPORT", Name = "Support", Value = "Email", DisplayOrder = 5 },
                    new() { FeatureKey = "REPORTS", Name = "Reports", Value = "Basic", DisplayOrder = 6 }
                }
            },
            new Package
            {
                Name = "Premium",
                Description = "Advanced protection for power users.",
                CurrentPrice = 299,
                OriginalPrice = 399,
                Currency = "EGP",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Features = new List<PackageFeature>
                {
                    new() { FeatureKey = "MAX_FILES_PER_MONTH", Name = "File Scans Per Month", Value = "Unlimited", DisplayOrder = 1 },
                    new() { FeatureKey = "LINK_SCANNING", Name = "Link Scanning", Value = "Unlimited", DisplayOrder = 2 },
                    new() { FeatureKey = "EMAIL_VERIFICATION", Name = "Email Verification", Value = "Advanced", DisplayOrder = 3 },
                    new() { FeatureKey = "PHISHING_PROTECTION", Name = "Phishing Protection", Value = "Advanced", DisplayOrder = 4 },
                    new() { FeatureKey = "SECURITY_NEWS", Name = "Cybersecurity News", Value = "true", DisplayOrder = 5 },
                    new() { FeatureKey = "SUPPORT", Name = "Support", Value = "24/7", DisplayOrder = 6 },
                    new() { FeatureKey = "REPORTS", Name = "Reports", Value = "Detailed", DisplayOrder = 7 },
                    new() { FeatureKey = "DATA_BACKUP", Name = "Data Backup", Value = "true", DisplayOrder = 8 },
                    new() { FeatureKey = "MAX_DEVICES", Name = "Device Protection", Value = "3 Devices", DisplayOrder = 9 }
                }
            },
            new Package
            {
                Name = "Enterprise",
                Description = "Complete cybersecurity suite for teams and businesses.",
                CurrentPrice = 1999,
                OriginalPrice = 2499,
                Currency = "EGP",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Features = new List<PackageFeature>
                {
                    new() { FeatureKey = "MAX_FILES_PER_MONTH", Name = "File Scans Per Month", Value = "Unlimited", DisplayOrder = 1 },
                    new() { FeatureKey = "LINK_SCANNING", Name = "Link Scanning", Value = "Unlimited", DisplayOrder = 2 },
                    new() { FeatureKey = "EMAIL_VERIFICATION", Name = "Email Verification", Value = "Advanced", DisplayOrder = 3 },
                    new() { FeatureKey = "PHISHING_PROTECTION", Name = "Phishing Protection", Value = "Advanced", DisplayOrder = 4 },
                    new() { FeatureKey = "SECURITY_NEWS", Name = "Cybersecurity News", Value = "true", DisplayOrder = 5 },
                    new() { FeatureKey = "SUPPORT", Name = "Support", Value = "24/7", DisplayOrder = 6 },
                    new() { FeatureKey = "REPORTS", Name = "Reports", Value = "Advanced Analytics", DisplayOrder = 7 },
                    new() { FeatureKey = "DATA_BACKUP", Name = "Data Backup", Value = "true", DisplayOrder = 8 },
                    new() { FeatureKey = "MAX_DEVICES", Name = "Device Protection", Value = "Unlimited", DisplayOrder = 9 },
                    new() { FeatureKey = "TEAM_SCANNING", Name = "Team Scanning", Value = "Unlimited", DisplayOrder = 10 },
                    new() { FeatureKey = "CUSTOM_DASHBOARD", Name = "Custom Dashboard", Value = "true", DisplayOrder = 11 },
                    new() { FeatureKey = "ACCOUNT_MANAGER", Name = "Account Manager", Value = "Dedicated", DisplayOrder = 12 },
                    new() { FeatureKey = "TEAM_TRAINING", Name = "Team Training", Value = "true", DisplayOrder = 13 },
                    new() { FeatureKey = "INTEGRATIONS", Name = "System Integrations", Value = "true", DisplayOrder = 14 },
                    new() { FeatureKey = "SLA", Name = "SLA", Value = "Custom", DisplayOrder = 15 },
                    new() { FeatureKey = "SECURITY_CONSULTATIONS", Name = "Security Consultations", Value = "true", DisplayOrder = 16 }
                }
            }
        };

        db.Packages.AddRange(packages);
        await db.SaveChangesAsync();
    }
}

app.Run();
