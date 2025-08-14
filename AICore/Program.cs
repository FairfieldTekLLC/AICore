using AICore.Classes;
using AICore.SemanticKernel;
using AICore.SemanticKernel.Database;
using Microsoft.AspNetCore.Http.Features;

namespace AICore;

public class Program
{
    public static void Main(string[] args)
    {
        Config.Instance.Load();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.AddDatabaseServices();
        builder.AddSemanticKernel();

        builder.Services.AddMvc().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        builder.Services
            .AddControllersWithViews(options =>
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 512 * 1024 * 1024; // 512 MB
        });

        builder.Services.AddSession();
        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(15);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.Configure<IISServerOptions>(options => { options.MaxRequestBodySize = int.MaxValue; });
        builder.Services.Configure<FormOptions>(options =>
        {
            options.ValueLengthLimit = int.MaxValue;
            options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
            options.MultipartHeadersLengthLimit = int.MaxValue;
        });


        var app = builder.Build();
        app.ConfigureDatabaseServices();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();
        app.UseSession();
        app.MapStaticAssets();
        app.MapControllerRoute(
                "default",
                "{controller=Login}/{action=LoginOrCreate}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}