using BookMovieTickets.Data;
using BookMovieTickets.Models;
using BookMovieTickets.Repositories;
using BookMovieTickets.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Linq.Expressions;
using System.Security.Cryptography;

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
        IRepository<Promotion> _promotionRepository;
        IRepository<PromotionUsage> _promotionUsageRepository;
        
        
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly IEmailSender _emailSender;

        public HomeController(IRepository<Category> categoryRepository, IRepository<Actor> actorRepository, IRepository<Cinema> cinemaRepository, IRepository<Movie> movieRepository, IRepository<MovieImage> movieImageRepository, IRepository<Seat> setRepository, IRepository<ShowTime> showTimeRepository, IRepository<Booking> bookingRepository, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IRepository<Promotion> promotionRepository, IRepository<PromotionUsage> promotionUsageRepository)
        {
            _categoryRepository = categoryRepository;
            _actorRepository = actorRepository;
            _cinemaRepository = cinemaRepository;
            _movieRepository = movieRepository;
            _movieImageRepository = movieImageRepository;
            _setRepository = setRepository;
            _showTimeRepository = showTimeRepository;
            _bookingRepository = bookingRepository;
            _userManager = userManager;
            _emailSender = emailSender;
            _promotionRepository = promotionRepository;
            _promotionUsageRepository = promotionUsageRepository;
        }

        public async Task<IActionResult> Index(FilterMovieVM vm)
        {
            //var query = _context.Movies
            //    .Include(m => m.Category)
            //    .Where(m => m.Status == true)
            //    .AsQueryable();

            var query = await _movieRepository.GetAsync(
                 filter: m => m.Status == true || m.Status == false,
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
            /////////
            int pageSize = 5;

            int count = query.Count();

            vm.TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            vm.Page = vm.Page <= 0 ? 1 : vm.Page;

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
            // جلب العروض السارية والمربوطة بأفلام ولم تنتهِ صلاحيتها بعد
            ViewBag.ActivePromotions = await _promotionRepository.GetAsync(
                filter: p => p.IsValid == true && DateTime.UtcNow < p.ValidTo,
                includes: new Expression<Func<Promotion, object>>[] { p => p.Movie } // أو Movie حسب اسم العلاقة عندك
            );
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
        //[Authorize]

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
        [Authorize]
        public async Task<IActionResult> ConfirmBooking(int showTimeId, int tickets)
        {
            var showTime = (await _showTimeRepository.GetAsync(
                s => s.Id == showTimeId,
                includes: new Expression<Func<ShowTime, object>>[]
                {
            s => s.Movie,
            s => s.Movie.Cinema
                }
            )).FirstOrDefault();

            if (showTime == null)
                return NotFound();

            // 👇 هات اليوزر الحالي
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account",
                    new { area = "Identity" });
            }

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
                TotalPrice = tickets * showTime.Movie.Price,

                // 👇 ربط الحجز باليوزر
                UserId = user.Id
            };

            await _bookingRepository.AddAsync(booking);
            // هنا ببعت رسالة لليوزر بتفاصيل الحجز 

            await _bookingRepository.CommitAsync();

            string body = $@"
<div style='font-family:Arial,sans-serif;
            max-width:650px;
            margin:auto;
            background:#111827;
            border-radius:18px;
            overflow:hidden;
            border:1px solid #374151;
            color:white;'>

    <!-- Header -->
    <div style='background:#dc2626;
                padding:25px;
                text-align:center;'>

        <h1 style='margin:0;
                   font-size:32px;'>
            🎬 Booking Confirmed
        </h1>

        <p style='margin-top:10px;
                  font-size:16px;
                  color:#ffe4e6;'>

            Your tickets have been booked successfully

        </p>

    </div>

    <!-- Content -->
    <div style='padding:30px;'>

        <p style='font-size:18px;
                  color:#d1d5db;
                  line-height:1.8;'>

            Hello <strong>{user.UserName}</strong>,
            <br/><br/>

            Your booking for the movie

            <strong style='color:#facc15;
                           font-size:20px;'>
                {showTime.Movie.Name}
            </strong>

            has been successfully confirmed 🍿

        </p>

        <!-- Booking Details -->
        <div style='background:#1f2937;
                    padding:25px;
                    border-radius:14px;
                    margin-top:25px;'>

            <h2 style='margin-top:0;
                       color:#ffffff;
                       margin-bottom:20px;'>

                Booking Details

            </h2>

            <p style='margin:10px 0;'>
                🎥 <strong>Movie:</strong>
                {showTime.Movie.Name}
            </p>

            <p style='margin:10px 0;'>
                🏢 <strong>Cinema:</strong>
                {showTime.Movie.Cinema.Name}
            </p>

            <p style='margin:10px 0;'>
                🎟 <strong>Tickets:</strong>
                {tickets}
            </p>

            <p style='margin:10px 0;'>
                💰 <strong>Total Price:</strong>
                {booking.TotalPrice} EGP
            </p>

            <p style='margin:10px 0;'>
                ⏰ <strong>Show Time:</strong>
                {showTime.StartTime:dddd, dd MMM yyyy - hh:mm tt}
            </p>

        </div>

        <!-- Footer Message -->
        <div style='margin-top:30px;
                    text-align:center;'>

            <p style='font-size:18px;
                      color:#f9fafb;'>

                Enjoy your movie night 🍿🎬

            </p>

            <p style='color:#9ca3af;
                      margin-top:15px;'>

                Thank you for choosing
                <strong>Book Movie Tickets</strong>

            </p>

        </div>

    </div>

    <!-- Footer -->
    <div style='background:#0f172a;
                padding:15px;
                text-align:center;
                color:#9ca3af;
                font-size:14px;'>

        Book Movie Tickets © 2026

    </div>

</div>
";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Movie Booking Confirmation",
                body
            );

            TempData["Success"] = "Booking Confirmed 🎟";

            return RedirectToAction(nameof(MyBookings));
        }


        /////////////////////////////////////////////////


        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var bookings = await _bookingRepository.GetAsync(
                b => b.UserId == user.Id,
                includes: new Expression<Func<Booking, object>>[]
                {
            b => b.ShowTime,
            b => b.ShowTime.Movie
                });

            return View(bookings);
        }

        [Authorize]
        public async Task<IActionResult> IncrementTickets(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var booking = await _bookingRepository.GetOneAsync(
                b => b.Id == bookingId && b.UserId == user.Id,
                includes: new Expression<Func<Booking, object>>[]
                {
            b => b.ShowTime,
            b => b.ShowTime.Movie
                });

            if (booking == null)
                return NotFound();

            // ❌ لو مفيش كراسي متاحة
            if (booking.ShowTime.AvailableSeats <= 0)
            {
                TempData["Error"] = "No more available seats!";
                return RedirectToAction(nameof(MyBookings));
            }

            // ✔ زيادة التذاكر
            booking.Tickets++;

            // ✔ تحديث السعر
            booking.TotalPrice += booking.ShowTime.Movie.Price;

            // ✔ خصم كرسي
            booking.ShowTime.AvailableSeats--;

            await _bookingRepository.CommitAsync();

            TempData["Success"] = "Ticket Added Successfully";

            return RedirectToAction(nameof(MyBookings));
        }

        [Authorize]
        public async Task<IActionResult> DecrementTickets(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var booking = await _bookingRepository.GetOneAsync(
                b => b.Id == bookingId && b.UserId == user.Id,
                includes: new Expression<Func<Booking, object>>[]
                {
            b => b.ShowTime,
            b => b.ShowTime.Movie
                });

            if (booking == null)
                return NotFound();

            // لو تذكرة واحدة → امسح الحجز بالكامل
            if (booking.Tickets == 1)
            {
                booking.ShowTime.AvailableSeats += 1;

                _bookingRepository.Delete(booking);

                await _bookingRepository.CommitAsync();

                TempData["Success"] = "Booking removed successfully";

                return RedirectToAction(nameof(MyBookings));
            }

            // غير كدا قلل طبيعي
            booking.Tickets--;

            booking.TotalPrice -= booking.ShowTime.Movie.Price;

            booking.ShowTime.AvailableSeats++;

            await _bookingRepository.CommitAsync();

            TempData["Success"] = "Ticket Removed Successfully";

            return RedirectToAction(nameof(MyBookings));
        }

        [Authorize]
        public async Task<IActionResult> DeleteBooking(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var booking = await _bookingRepository.GetOneAsync(
                b => b.Id == bookingId && b.UserId == user.Id,
                includes: new Expression<Func<Booking, object>>[]
                {
            b => b.ShowTime,
            b => b.ShowTime.Movie
                });

            if (booking == null)
                return NotFound();

            // ✔ رجوع الكراسي
            booking.ShowTime.AvailableSeats += booking.Tickets;

            _bookingRepository.Delete(booking);

            await _bookingRepository.CommitAsync();

            await _emailSender.SendEmailAsync(
           user.Email,
           "Booking Cancellation",
           $@"
    <div style='font-family:Arial,sans-serif;
                max-width:600px;
                margin:auto;
                padding:30px;
                border-radius:15px;
                background:#111827;
                color:#ffffff;
                border:1px solid #374151;'>

        <h1 style='color:#ef4444;
                   text-align:center;
                   margin-bottom:20px;'>
            Booking Cancelled
        </h1>

        <p style='font-size:16px;
                  line-height:1.8;
                  color:#d1d5db;'>

            Hello <strong>{user.Name}</strong>,
            <br/><br/>

            Your booking for the movie 
            <strong style='color:#facc15;'>
                {booking.ShowTime.Movie.Name}
            </strong>

            scheduled at

            <strong style='color:#38bdf8;'>
                {booking.ShowTime.StartTime:dddd, dd MMM yyyy - hh:mm tt}
            </strong>

            has been

            <strong style='color:#ef4444;'>
                successfully cancelled
            </strong>.

            <br/><br/>

            We hope to see you again soon 🎬

        </p>

        <div style='background:#1f2937;
                    padding:20px;
                    border-radius:10px;
                    margin-top:25px;'>

            <h3 style='margin-top:0;
                       color:#ffffff;'>
                Booking Details
            </h3>

            <p>
                <strong>Movie:</strong>
                {booking.ShowTime.Movie.Name}
            </p>

            <p>
                <strong>Date & Time:</strong>
                {booking.ShowTime.StartTime:dddd, dd MMM yyyy - hh:mm tt}
            </p>

            <p>
                <strong>Total Price:</strong>
                {booking.TotalPrice} EGP
            </p>

        </div>

        <hr style='margin-top:30px;
                   border-color:#374151;' />

        <p style='text-align:center;
                  color:#9ca3af;
                  font-size:14px;'>

            Book Movie Tickets © 2026

        </p>

    </div>"
       );

            TempData["Success"] = "Booking Deleted Successfully";

            return RedirectToAction(nameof(MyBookings));
        }


        [HttpPost]
        public async Task<IActionResult> CheckPromoCode(string code, int currentShowTimeId, decimal currentPrice)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً لتطبيق الخصم!" });

            if (string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "يرجى إدخال كود الخصم!" });
            }

            var promotion = await _promotionRepository.GetOneAsync(p =>
                p.IsValid == true &&
                p.Code == code.Trim() &&
                DateTime.UtcNow < p.ValidTo &&
                p.MaxUsage > 0
            );

            if (promotion == null)
            {
                return Json(new { success = false, message = "كود الخصم غير صحيح أو انتهت صلاحيته!" });
            }

            var userBooking = await _bookingRepository.GetOneAsync(b =>
                b.UserId == user.Id &&
                b.ShowTimeId == currentShowTimeId
            );

            if (userBooking == null)
            {
                return Json(new { success = false, message = "عذراً، لم نجد حجزاً قائماً لهذا العرض في حسابك لتطبيق الخصم عليه!" });
            }

            var showTimeWithMovie = await _showTimeRepository.GetOneAsync(s => s.Id == currentShowTimeId);
            if (showTimeWithMovie != null && promotion.MovieId != showTimeWithMovie.MovieId)
            {
                return Json(new { success = false, message = "هذا الكود غير مخصص للفيلم المعروض في هذه الحفلة!" });
            }

            var alreadyUsed = await _promotionUsageRepository.GetOneAsync(pu =>
                pu.UserId == user.Id &&
                pu.PromotionId == promotion.Id
            );

            if (alreadyUsed != null)
            {
                return Json(new { success = false, message = "لقد قمت باستخدام هذا الكود من قبل!" });
            }

            decimal discountAmount = currentPrice * (promotion.Discount / 100);
            if (discountAmount > currentPrice)
                discountAmount = currentPrice;

            decimal finalPrice = currentPrice - discountAmount;

            userBooking.TotalPrice = finalPrice;

            
            promotion.MaxUsage--;

            var usage = new PromotionUsage
            {
                UserId = user.Id,
                PromotionId = promotion.Id
            };
            await _promotionUsageRepository.AddAsync(usage);

            //  حفظ كل العجن ده في الداتا بيز 
            await _bookingRepository.CommitAsync();
            await _promotionRepository.CommitAsync();
            await _promotionUsageRepository.CommitAsync();

            
            return Json(new
            {
                success = true,
                message = "تم تطبيق كود الخصم وحفظ السعر الجديد بنجاح! 🎉",
                discount = discountAmount,
                newPrice = finalPrice

            });
        }
        // الدفع 
        public async Task<IActionResult> Pay(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var booking = await _bookingRepository.GetOneAsync(
                filter: b => b.Id == bookingId,
                includes: new Expression<Func<Booking, object>>[]
                {
            b => b.ShowTime,
            b => b.ShowTime.Movie
                }
            );

            if (booking is null || booking.ShowTime is null || booking.ShowTime.Movie is null)
            {
                return NotFound("بيانات الحجز أو الفيلم غير كاملة.");
            }

            decimal actualPricePerTicket = booking.TotalPrice / booking.Tickets;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Home/Success?bookingId={bookingId}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Home/Cancel",
            };

            var sessionLineItemOptions = new SessionLineItemOptions()
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "egp",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = booking.ShowTime.Movie.Name,
                        Description = $"Booking for {booking.Tickets} tickets - Total paid: {booking.TotalPrice} EGP",
                    },
                    UnitAmount = Convert.ToInt64(actualPricePerTicket * 100),
                },
                Quantity = booking.Tickets
            };

            options.LineItems.Add(sessionLineItemOptions);

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Redirect(session.Url);
        }



    }
}
