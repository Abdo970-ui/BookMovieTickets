using BookMovieTickets.Data;
using BookMovieTickets.Models;
using BookMovieTickets.Repositories;
using BookMovieTickets.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BookMovieTickets
{
    public class Program
    {

        public static void Main(string[] args)
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


            var app = builder.Build();

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
