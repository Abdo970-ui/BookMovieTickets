using BookMovieTickets.Data;
using BookMovieTickets.Models;
using BookMovieTickets.Repositories;
using BookMovieTickets.Utilities;
using BookMovieTickets.Utilities.DbSeeder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BookMovieTickets
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();



            builder.Services.AddControllersWithViews();
            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Connection String "
                 + "'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;

            }).AddEntityFrameworkStores<ApplicationDbContext>()
           .AddDefaultTokenProviders();


            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IRepository<ApplicationUserOtp> , Repository<ApplicationUserOtp> >();
            // Booking

            builder.Services.AddScoped<IRepository<Seat> , Repository<Seat> >();
            builder.Services.AddScoped<IRepository<ShowTime> , Repository<ShowTime> >();
            builder.Services.AddScoped<IRepository<Booking> , Repository<Booking> >();
            builder.Services.AddScoped<IRepository<Promotion> , Repository<Promotion> >();
            builder.Services.AddScoped<IRepository<PromotionUsage> , Repository<PromotionUsage> >();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();


            builder.Services.ConfigureApplicationCookie(options =>
            {
                //Changing the defult routes for Identity
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                options.SlidingExpiration = true;
            });



            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];



            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var inializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                await inializer.InitializeAsnc();
            }




            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();


            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
                //pattern: "{area=Identity}/{controller=Account}/{action=Register}/{id?}")
                //pattern: "{area=Admin}/{controller=Movies}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();


       
        }

    }
}
