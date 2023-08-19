using Microsoft.AspNetCore.Http.Extensions;

namespace Sample.WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        // Add services to the container.
        builder.Services.AddRazorPages();

        var app = builder.Build();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        app.Use(async (ctx, next) =>
        {
            logger.LogInformation("HttpRequest: {Url}", ctx.Request.GetDisplayUrl());
            await next();
        });
        app.UseHttpLogging();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        if (app.Environment.IsProduction())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();



        app.MapRazorPages();

        app.MapGet("/BadRequest", ctx => throw new BadHttpRequestException("Bad Request"));
        app.Run();
    }
}