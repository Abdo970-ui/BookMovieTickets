using BookMovieTickets.Models;
using BookMovieTickets.Repositories;
using BookMovieTickets.Utilities.DbSeeder;
using BookMovieTickets.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace BookMovieTickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]

    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Movie> _movieRepo;
        private readonly IRepository<Booking> _bookingRepo;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IRepository<Movie> movieRepo,
            IRepository<Booking> bookingRepo)
        {
            _userManager = userManager;
            _movieRepo = movieRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardVM();

            // Users
            vm.TotalUsers = _userManager.Users.Count();

            // Movies
            var movies = await _movieRepo.GetAsync();
            vm.TotalMovies = movies.Count();

            // Bookings (with includes inside repo)
            var bookings = await _bookingRepo.GetAsync(
                includes: new Expression<Func<Booking, object>>[]
                {
                    b => b.ShowTime,
                    b => b.ShowTime.Movie
                });

            vm.TotalBookings = bookings.Count();
            vm.TotalRevenue = bookings.Sum(b => b.TotalPrice);

            // Group by Movie
            var grouped = bookings
                .GroupBy(b => b.ShowTime.Movie.Name)
                .Select(g => new
                {
                    MovieName = g.Key,
                    Count = g.Count()
                })
                .ToList();

            vm.MovieNames = grouped
                .Select(x => x.MovieName)
                .ToList();

            vm.BookingCounts = grouped
                .Select(x => x.Count)
                .ToList();

            // Most booked movie
            var mostBooked = grouped
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            vm.MostBookedMovie = mostBooked?.MovieName ?? "No Data";

            return View(vm);
        }
    }
}