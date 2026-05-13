using BookMovieTickets.Models;
using BookMovieTickets.ViewModel;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookMovieTickets.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
          var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Mapping بطرقتين 
            //var applicationUserVM = new ApplicationUserVM();
            //applicationUserVM.Address = user.Address;
            //applicationUserVM.PhoneNumber = user.PhoneNumber;
            //applicationUserVM.Email = user.Email;
            //applicationUserVM.Name = user.Name; 

            // الطريقة التانية
            
            var applicationUserVM = user.Adapt<ApplicationUserVM>();
            ViewBag.IsProfilePage = true;
            return View(applicationUserVM);
        }
        public async Task<IActionResult> UpdateProfile(ApplicationUserVM applicationUserVM)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return NotFound();

                // بيانات عادية
                user.Name = applicationUserVM.Name;
                user.Address = applicationUserVM.Address;
                user.PhoneNumber = applicationUserVM.PhoneNumber;

                // =========================
                // 📸 رفع الصورة
                // =========================
                if (applicationUserVM.ProfileImage != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(applicationUserVM.ProfileImage.FileName);

                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    // تأكد إن الفولدر موجود
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await applicationUserVM.ProfileImage.CopyToAsync(stream);
                    }

                    // نحفظ اسم الصورة في الداتابيز
                    user.ImageUrl = fileName;
                }

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    string errors = string.Join(" , ", result.Errors.Select(e => e.Description));
                    TempData["Error"] = errors;

                    return View(nameof(Index), applicationUserVM);
                }

                TempData["Success"] = "User Updated successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        public async Task<IActionResult> UpdatePassword(ApplicationUserVM applicationUserVM)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    applicationUserVM.CurrentPassword,
                    applicationUserVM.NewPassword
                );

                if (!result.Succeeded)
                {
                    string errors = string.Join(" , ", result.Errors.Select(e => e.Description));

                    TempData["Error"] = errors;

                    return View(nameof(Index), applicationUserVM);
                }
                else
                {
                    TempData["Success"] = "Password Changed successfully";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
