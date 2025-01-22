using QueryPhone.Core.Clients;
using QueryPhone.Web.Services;

namespace QueryPhone.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null); ;

            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IQueryPhoneService, QueryPhoneService>();
            builder.Services.AddSingleton<IQueryPhoneClient, PhoneBookClient>();
            builder.Services.AddSingleton<IQueryPhoneClient, TellowsClient>();
            builder.Services.AddSingleton<IQueryPhoneClient, WhocallClient>();
            builder.Services.AddSingleton<IQueryPhoneClient, WhosNumberClient>();
            builder.Services.AddSingleton<IQueryPhoneClient, BaselyClient>();

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
