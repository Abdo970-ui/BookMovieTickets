using BookMovieTickets.Data;
using BookMovieTickets.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookMovieTickets.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();

        public IActionResult Index(FilterMovieVM vm)
        {
            var query = _context.Movies
                .Include(m => m.Category)
                .Where(m => m.Status == true)
                .AsQueryable();


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

            ViewBag.Cinemas = _context.Cinemas.ToList();
            ViewBag.Categories = _context.Categories.ToList();

            return View(vm);
        }
    }
}