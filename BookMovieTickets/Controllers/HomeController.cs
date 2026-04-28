using System.Diagnostics;
using BookMovieTickets.Data;
using BookMovieTickets.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookMovieTickets.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext _context;// = new ApplicationDbContext();

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
           
            return View();
        }
        public IActionResult Privacy()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
