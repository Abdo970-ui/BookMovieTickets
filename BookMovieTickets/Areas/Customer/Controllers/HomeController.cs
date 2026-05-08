using BookMovieTickets.Data;
using BookMovieTickets.Models;
using BookMovieTickets.Repositories;
using BookMovieTickets.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookMovieTickets.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        //ApplicationDbContext _context = new ApplicationDbContext();
        IRepository<Category> _categoryRepository; //= new Repository<Category>();
        IRepository<Actor> _actorRepository; //= new Repository<Actor>();
        IRepository<Cinema> _cinemaRepository; //= new Repository<Cinema>();
        IRepository<Movie> _movieRepository;// = new Repository<Movie>();
        IRepository<MovieImage> _movieImageRepository;// = new Repository<Movie>();
        IRepository<Seat> _setRepository;
        IRepository<ShowTime> _showTimeRepository;
        IRepository<Booking> _bookingRepository;
        public HomeController(IRepository<Category> categoryRepository, IRepository<Actor> actorRepository, IRepository<Cinema> cinemaRepository, IRepository<Movie> movieRepository, IRepository<MovieImage> movieImageRepository, IRepository<Seat> setRepository, IRepository<ShowTime> showTimeRepository, IRepository<Booking> bookingRepository)
        {
            _categoryRepository = categoryRepository;
            _actorRepository = actorRepository;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _movieImageRepository = movieImageRepository;
            _setRepository = setRepository;
            _showTimeRepository = showTimeRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<IActionResult> Index(FilterMovieVM vm)
        {
            //var query = _context.Movies
            //    .Include(m => m.Category)
            //    .Where(m => m.Status == true)
            //    .AsQueryable();

            var query = await _movieRepository.GetAsync(
                 filter: m => m.Status == true,
                  includes: new Expression<Func<Movie, object>>[]
                  {
                       m => m.Category
                  });


            if (!string.IsNullOrEmpty(vm.MovieName))
            {
                query = query.Where(m => m.Name.Contains(vm.MovieName));
            }


            if (vm.CinemaId.HasValue)
            {
                query = query.Where(m => m.CinemaId == vm.CinemaId);
            }

            if (vm.Date.HasValue)
            {
                var date = vm.Date.Value.Date;

                query = query.Where(m =>
                    m.DateTime >= date &&
                    m.DateTime < date.AddDays(1));
            }

            if (vm.CategoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == vm.CategoryId);
            }

            int pageSize = 2;

            int count = query.Count();

            vm.TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            if (vm.Page <= 0)
            {
                vm.Page = 1;
            }

            if (vm.Page > vm.TotalPages && vm.TotalPages > 0)
            {
                vm.Page = vm.TotalPages;
            }

            vm.PageSize = pageSize;

            vm.Movies = query
                .Skip((vm.Page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            //ViewBag.Cinemas = _context.Cinemas.ToList();
            //ViewBag.Categories = _context.Categories.ToList();

            ViewBag.Cinemas = await _cinemaRepository.GetAsync();
            ViewBag.Categories = await _categoryRepository.GetAsync();

            return View(vm);
        }
        public async Task<IActionResult> Details(int id)
        {
            var movies = await _movieRepository.GetAsync(
                m => m.Id == id,
                new Expression<Func<Movie, object>>[]
                {
            m => m.Category,
            m => m.Cinema
                }
            );

            var movie = movies.FirstOrDefault();

            if (movie == null)
                return NotFound();

            // 👇 هنا المهم: نجيب الـ ShowTimes
            var showTimes = await _showTimeRepository.GetAsync(s => s.MovieId == id);

            ViewBag.ShowTimes = showTimes;
            return View(movie);
        }

        public async Task<IActionResult> ShowTimes(int movieId)
        {
            var showTimes = await _showTimeRepository.GetAsync(s => s.MovieId == movieId);

            ViewBag.MovieId = movieId;

            return View(showTimes);
        }

        [HttpGet]
        public async Task<IActionResult> Book(int showTimeId)
        {
                var showTime = (await _showTimeRepository.GetAsync(
                s => s.Id == showTimeId,
                includes: new Expression<Func<ShowTime, object>>[]
                {
                s => s.Movie
                }
                )).FirstOrDefault();

            if (showTime == null)
                return NotFound();

            return View(showTime);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int showTimeId, int tickets)
        {
            var showTime = (await _showTimeRepository.GetAsync(
                s => s.Id == showTimeId,
                includes: new Expression<Func<ShowTime, object>>[]
                {
            s => s.Movie
                }
            )).FirstOrDefault();

            if (showTime == null)
                return NotFound();

            // ❌ لو عدد التذاكر غلط
            if (tickets <= 0)
            {
                TempData["Error"] = "Invalid tickets number!";
                return RedirectToAction(nameof(Book), new { showTimeId });
            }

            // ❌ لو مفيش كراسي خالص
            if (showTime.AvailableSeats <= 0)
            {
                TempData["Error"] = "No seats available for this show!";
                return RedirectToAction(nameof(Index));
            }

            // ❌ لو الكراسي أقل من المطلوب
            if (showTime.AvailableSeats < tickets)
            {
                TempData["Error"] = $"Only {showTime.AvailableSeats} seats available!";
                return RedirectToAction(nameof(Book), new { showTimeId });
            }

            // ✔ خصم المقاعد
            showTime.AvailableSeats -= tickets;
            _showTimeRepository.Update(showTime);
            await _showTimeRepository.CommitAsync();

            // ✔ إنشاء الحجز
            var booking = new Booking
            {
                ShowTimeId = showTimeId,
                Tickets = tickets,
                TotalPrice = tickets * 100
            };

            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.CommitAsync();

            TempData["Success"] = "Booking Confirmed 🎟";

            return RedirectToAction(nameof(Index), new { showTimeId });
        }
    }
}
