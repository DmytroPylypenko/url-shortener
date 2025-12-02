using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Services.Auth;
using UrlShortener.Application.Services.UrlShortening;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Infrastructure.Persistence.Repositories;
using UrlShortener.Web.Configuration;

namespace UrlShortener.Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
       
        builder.Services.Configure<ApiSettings>(
            builder.Configuration.GetSection("ApiSettings"));
        
        builder.Services.AddHttpClient();
        
        builder.Services.AddScoped<IPasswordHasher, PbkdfPasswordHasher>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUrlRecordRepository, UrlRecordRepository>();
        builder.Services.AddScoped<IShortCodeGenerator, Base62ShortCodeGenerator>();
        builder.Services.AddScoped<IUrlShorteningService, UrlShorteningService>();
        builder.Services.AddScoped<IAboutContentRepository, AboutContentRepository>(); 
        
        // Add and configure JWT Authentication
        var jwtKey = builder.Configuration["JwtSettings:Key"];
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        builder.Services.AddAuthorization();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        
        app.UseStaticFiles();
        app.UseRouting();

        // Take JWT from cookie and attach it to every API call
        app.Use(async (context, next) =>
        {
            if (context.Request.Cookies.TryGetValue("auth_token", out var token))
            {
                context.Request.Headers.Authorization = $"Bearer {token}";
            }

            await next();
        });
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllerRoute(
            name: "redirect",
            pattern: "r/{shortCode}",
            defaults: new { controller = "Redirect", action = "RedirectToOriginal" });
        
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        
        app.Run();
    }
}
