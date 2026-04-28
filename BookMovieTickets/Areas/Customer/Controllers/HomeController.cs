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

        public HomeController(IRepository<Category> categoryRepository, IRepository<Actor> actorRepository, IRepository<Cinema> cinemaRepository, IRepository<Movie> movieRepository, IRepository<MovieImage> movieImageRepository)
        {
            _categoryRepository = categoryRepository;
            _actorRepository = actorRepository;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _movieImageRepository = movieImageRepository;
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

            ViewBag.Cinemas =await _cinemaRepository.GetAsync();
            ViewBag.Categories = await _categoryRepository.GetAsync();

            return View(vm);
        }
    }
}