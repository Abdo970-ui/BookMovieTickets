using BookMovieTickets.Data;
using BookMovieTickets.Models;
using BookMovieTickets.Repositories;
using BookMovieTickets.Utilities.DbSeeder;
using BookMovieTickets.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookMovieTickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =$"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}, {CD.EMPLOYEE_ROLE}")]
    public class MoviesController : Controller
    {
        //ApplicationDbContext _context = new ApplicationDbContext();
        IRepository<Category> _categoryRepository; //= new Repository<Category>();
        IRepository<Actor> _actorRepository; //= new Repository<Actor>();
        IRepository<Cinema> _cinemaRepository; //= new Repository<Cinema>();
        IRepository<Movie> _movieRepository;// = new Repository<Movie>();
        IRepository<MovieImage> _movieImageRepository;// = new Repository<Movie>();

        public MoviesController(IRepository<Category> categoryRepository, IRepository<Actor> actorRepository, IRepository<Cinema> cinemaRepository, IRepository<Movie> movieRepository, IRepository<MovieImage> movieImageRepository)
        {
            _categoryRepository = categoryRepository;
            _actorRepository = actorRepository;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _movieImageRepository = movieImageRepository;
        }

        public async Task<IActionResult> Index()
        {
            //var movies = _context.Movies
            //.Include(m => m.Category)
            //.Include(m => m.Cinema)
            //.Include(m => m.Actors)
            //.Include(m => m.SubImages)
            //.ToList();
            //return View(movies);
            var movies = await _movieRepository.GetAsync(
                includes: new Expression<Func<Movie, object>>[]
                {
                    m=>m.Category,
                    m=>m.Cinema,
                    m=>m.Actors,
                    m=>m.SubImages
                });
            return View(movies);
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new MovieVM
            {
                //Categories = _context.Categories.ToList(),
                //Cinemas = _context.Cinemas.ToList(),
                //Actors = _context.Actors.ToList()

                Categories = (await _categoryRepository.GetAsync()).ToList(),
                Cinemas = (await _cinemaRepository.GetAsync()).ToList(),
                Actors = (await _actorRepository.GetAsync()).ToList(),
            };

            return View(vm);
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]


        [HttpPost]
        public async Task<IActionResult> Create(MovieVM vm)
        {
            if (!ModelState.IsValid)
            {
                //vm.Categories = _context.Categories.ToList();
                //vm.Cinemas = _context.Cinemas.ToList();
                //vm.Actors = _context.Actors.ToList();
                vm.Categories = (await  _categoryRepository.GetAsync()).ToList();
                vm.Cinemas = (await _cinemaRepository.GetAsync()).ToList();
                vm.Actors = (await _actorRepository.GetAsync()).ToList();
                return View(vm);

            }


            // حفظ الصورة
            string fileName = Guid.NewGuid() + Path.GetExtension(vm.MainImg.FileName);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
               await vm.MainImg.CopyToAsync(stream);
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

            //movie.Actors = _context.Actors
            //    .Where(a => vm.ActorIds.Contains(a.Id))
            //    .ToList();
            movie.Actors = (await _actorRepository.GetAsync(a => vm.ActorIds.Contains(a.Id))).ToList(); 

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

            //_context.Movies.Add(movie);
            //_context.SaveChanges();
          await  _movieRepository.AddAsync(movie);
            await _movieRepository.CommitAsync();

            return RedirectToAction("Index");
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]


        public async Task<IActionResult> Update(int id)
        {
            var movie = await _movieRepository.GetOneAsync(
                m => m.Id == id,
                includes: new Expression<Func<Movie, object>>[]
                {
            m => m.Actors,
            m => m.SubImages,
            m => m.Category,
            m => m.Cinema
                });

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

                ActorIds = movie.Actors.Select(a => a.Id).ToList(),

                Categories = (await _categoryRepository.GetAsync()).ToList(),
                Cinemas = (await _cinemaRepository.GetAsync()).ToList(),
                Actors = (await _actorRepository.GetAsync()).ToList()
            };

            return View(vm);
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(MovieVM model)
        {
            if (!ModelState.IsValid)
            {
                //model.Categories = _context.Categories.ToList();
                //model.Cinemas = _context.Cinemas.ToList();
                //model.Actors = _context.Actors.ToList();
                model.Categories = (await _categoryRepository.GetAsync()).ToList();
                model.Cinemas = (await _cinemaRepository.GetAsync()).ToList();
                model.Actors = (await _actorRepository.GetAsync()).ToList();
                return View(model);
            }

            //var movie = _context.Movies
            //    .Include(m => m.Actors)
            //    .Include(m => m.SubImages)
            //    .FirstOrDefault(m => m.Id == model.Id);
            var movie =await _movieRepository.GetOneAsync(
                m => m.Id == model.Id,
                includes: new Expression<Func<Movie, object>>[]
                {
                  m => m.Actors,
                  m => m.SubImages
                });


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
                //var actors = _context.Actors
                //    .Where(a => model.ActorIds.Contains(a.Id))
                //    .ToList();
                var actors = (await _actorRepository.GetAsync(a => model.ActorIds.Contains(a.Id))).ToList();

                foreach (var actor in actors)
                {
                    movie.Actors.Add(actor);
                }
            }
            //_context.SaveChanges();

            _movieRepository.Update(movie);
            await _movieRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            //var movie = _context.Movies
            //    .FirstOrDefault(m => m.Id == id);
            var movie =await _movieRepository.GetOneAsync(m => m.Id == id);

            if (movie == null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(movie);
        }
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Movie movie)
        {
            //var data = _context.Movies.FirstOrDefault(m => m.Id == movie.Id);
            var data =await _movieRepository.GetOneAsync(m => m.Id == movie.Id);

            if (data == null)
                return RedirectToAction("NotFoundPage", "Home");
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", data.MainImg);

            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            //_context.Movies.Remove(data);
            //_context.SaveChanges();
            _movieRepository.Delete(data);
           await _movieRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
