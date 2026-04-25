using BookMovieTickets.Data;
using BookMovieTickets.Models;
using BookMovieTickets.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookMovieTickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MoviesController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        public IActionResult Index()
        {
            var movies = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Cinema)
                .Include(m => m.Actors)
                .Include(m => m.SubImages)
                .ToList();

            return View(movies);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new MovieVM
            {
                Categories = _context.Categories.ToList(),
                Cinemas = _context.Cinemas.ToList(),
                Actors = _context.Actors.ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Create(MovieVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = _context.Categories.ToList();
                vm.Cinemas = _context.Cinemas.ToList();
                vm.Actors = _context.Actors.ToList();
                return View(vm);

            }


            // حفظ الصورة
            string fileName = Guid.NewGuid() + Path.GetExtension(vm.MainImg.FileName);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                vm.MainImg.CopyTo(stream);
            }

            var movie = new Movie
            {
                Name = vm.Name,
                Description = vm.Description,
                MainImg = fileName,
                Price = vm.Price,
                Status = vm.Status,
                DateTime = vm.DateTime,
                CategoryId = vm.CategoryId,
                CinemaId = vm.CinemaId
            };

            movie.Actors = _context.Actors
                .Where(a => vm.ActorIds.Contains(a.Id))
                .ToList();

            movie.SubImages = new List<MovieImage>();

            foreach (var img in vm.SubImages)
            {
                string imgName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                string imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imgName);

                using (var stream = new FileStream(imgPath, FileMode.Create))
                {
                    img.CopyTo(stream);
                }

                movie.SubImages.Add(new MovieImage
                {
                    ImageUrl = imgName
                });
            }

            _context.Movies.Add(movie);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Update(int id)
        {
            var test = id;
            var movie = _context.Movies
                .Include(m => m.Actors)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return RedirectToAction("NotFoundPage", "Home");

            var vm = new MovieVM
            {
                Id = movie.Id,
                Name = movie.Name,
                Description = movie.Description,
                Price = movie.Price,
                Status = movie.Status,
                DateTime = movie.DateTime,
                CategoryId = movie.CategoryId,
                CinemaId = movie.CinemaId,

                Categories = _context.Categories.ToList(),
                Cinemas = _context.Cinemas.ToList(),
                Actors = _context.Actors.ToList(),

                ActorIds = movie.Actors.Select(a => a.Id).ToList()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(MovieVM model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = _context.Categories.ToList();
                model.Cinemas = _context.Cinemas.ToList();
                model.Actors = _context.Actors.ToList();
                return View(model);
            }

            var movie = _context.Movies
                .Include(m => m.Actors)
                .Include(m => m.SubImages)
                .FirstOrDefault(m => m.Id == model.Id);

            if (movie == null)
                return RedirectToAction("NotFoundPage", "Home");

            movie.Name = model.Name;
            movie.Description = model.Description;
            movie.Price = model.Price;
            movie.Status = model.Status;
            movie.DateTime = model.DateTime;
            movie.CategoryId = model.CategoryId;
            movie.CinemaId = model.CinemaId;

            if (model.MainImg != null)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(model.MainImg.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    model.MainImg.CopyTo(stream);
                }

                movie.MainImg = fileName;
            }
            if (model.SubImages != null && model.SubImages.Count > 0)
            {
                movie.SubImages.Clear();

                foreach (var img in model.SubImages)
                {
                    string imgName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    string imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imgName);

                    using (var stream = new FileStream(imgPath, FileMode.Create))
                    {
                        img.CopyTo(stream);
                    }

                    movie.SubImages.Add(new MovieImage
                    {
                        ImageUrl = imgName
                    });
                }
            }
            movie.Actors.Clear();

            if (model.ActorIds != null && model.ActorIds.Any())
            {
                var actors = _context.Actors
                    .Where(a => model.ActorIds.Contains(a.Id))
                    .ToList();

                foreach (var actor in actors)
                {
                    movie.Actors.Add(actor);
                }
            }
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var movie = _context.Movies
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(movie);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Movie movie)
        {
            var data = _context.Movies.FirstOrDefault(m => m.Id == movie.Id);

            if (data == null)
                return RedirectToAction("NotFoundPage", "Home");
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", data.MainImg);

            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _context.Movies.Remove(data);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
