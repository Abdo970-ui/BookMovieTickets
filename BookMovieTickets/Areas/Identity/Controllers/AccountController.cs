using BookMovieTickets.Models;
using BookMovieTickets.Repositories;
using BookMovieTickets.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Cryptography;

namespace BookMovieTickets.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly SignInManager<ApplicationUser> _signInManager;
        public readonly IEmailSender _emailSender;
        public readonly IRepository<ApplicationUserOtp> _applicationUserOtpRepository;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IRepository<ApplicationUserOtp> applicationUserOtpRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOtpRepository = applicationUserOtpRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }
            ApplicationUser user = new ApplicationUser()
            {
                Name = registerVM.Name,
                Address = registerVM.Address,
                Email = registerVM.Email,
                UserName = registerVM.UserName,

            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var erorr in result.Errors)
                {
                    ModelState.AddModelError("", erorr.Description);
                }
                return View(registerVM);
            }
            TempData["Success"] = "Account created successfully!";

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { area = "Identity", userId = user.Id, token },
                Request.Scheme
            );

            var body = $@"
<div style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>

    <div style='max-width: 500px; margin: auto; background: #ffffff; border-radius: 10px; padding: 30px; text-align: center;'>

        <h2 style='color: #333;'>🎬 Welcome to BookMovieTickets</h2>

        <p style='color: #555; font-size: 15px;'>
            Thanks for creating an account!  
            Please confirm your email to get started.
        </p>

        <a href='{link}' 
           style='display: inline-block; margin-top: 20px; padding: 12px 25px; background-color: #4dabf7; color: white; text-decoration: none; border-radius: 6px; font-weight: bold;'>
            Confirm Email
        </a>

        <p style='margin-top: 20px; font-size: 14px; color: #777;'>
            Or copy and paste this link in your browser:
        </p>

        <a href='{link}' style='word-break: break-all; color: #4dabf7; font-size: 13px;'>
            {link}
        </a>

        <p style='margin-top: 25px; font-size: 13px; color: #999;'>
            If you didn’t create this account, you can safely ignore this email.
        </p>

    </div>

</div>";

            await _emailSender.SendEmailAsync(
                registerVM.Email,
                "Confirm your email",
                body
            );
            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if(!ModelState.IsValid)
            {
                return View(loginVM);
            }
            var user =await _userManager.FindByEmailAsync(loginVM.UserNameOrEmail)
                ??await _userManager.FindByNameAsync(loginVM.UserNameOrEmail);
             if(user == null)
            {
                ModelState.AddModelError("", "Invalid UserName Or Password !");
                return View(loginVM);
            }
           var result =  await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "TO Many Attempes Please Try Again Later");
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("", "Please Confirm Your Email First");

                }
                else
                {

                    ModelState.AddModelError("", "Invalid UserName Or Password .");
                }
                return View(loginVM);
            }
            TempData["Success"] = "Logged in successfully";

            return RedirectToAction("Index" , "Home" , new {area = "Customer"});
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId , string token)
        {
           
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                TempData["Error"] = "Invalid User";
                return RedirectToAction(nameof(Login));
            }
          var result =  await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                 TempData["Error"] = "Can't Confirm Email";

                 return RedirectToAction(nameof(Login));
            }

            TempData["Success"] = "Account created successfully!";
 
            return RedirectToAction(nameof(Login));

        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM )
        {
            if (!ModelState.IsValid)
            {
                return View(resendEmailConfirmationVM);
            }
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOrEmail)
               ?? await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOrEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid UserName Or Password !");
                return View(resendEmailConfirmationVM);
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { area = "Identity", userId = user.Id, token },
                Request.Scheme
            );

            var body = $@"
<div style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 30px;'>

    <div style='max-width: 500px; margin: auto; background: #ffffff; border-radius: 10px; padding: 30px; text-align: center;'>

        <h2 style='color: #333;'>🎬 Welcome to BookMovieTickets</h2>

        <p style='color: #555; font-size: 15px;'>
            Thanks for creating an account!  
            Please confirm your email to get started.
        </p>

        <a href='{link}' 
           style='display: inline-block; margin-top: 20px; padding: 12px 25px; background-color: #4dabf7; color: white; text-decoration: none; border-radius: 6px; font-weight: bold;'>
            Confirm Email
        </a>

        <p style='margin-top: 20px; font-size: 14px; color: #777;'>
            Or copy and paste this link in your browser:
        </p>

        <a href='{link}' style='word-break: break-all; color: #4dabf7; font-size: 13px;'>
            {link}
        </a>

        <p style='margin-top: 25px; font-size: 13px; color: #999;'>
            If you didn’t create this account, you can safely ignore this email.
        </p>

    </div>

</div>";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirm your email",
                body
            );
            TempData["Success"] = "Resend Email Confirmation successfully!";

            return RedirectToAction(nameof(Login));
           
        }
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(forgetPasswordVM);
            }
            var user = await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOrEmail)
              ?? await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError("", "Invalid UserName Or Password");
                return View(forgetPasswordVM);
            }
            var applicationUserOtps = await _applicationUserOtpRepository.GetAsync(e => e.ApplicationUserId == user.Id);
            var count = applicationUserOtps.Count(e => (DateTime.UtcNow - e.CreatedAt).TotalHours <= 24);
            if (count >= 5)
            {
                ModelState.AddModelError("", "To many attmpes please try again later");
                return View(forgetPasswordVM);
            }
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var applicationUserOtp = new ApplicationUserOtp(user.Id, otp);
            await _applicationUserOtpRepository.AddAsync(applicationUserOtp);
            await _applicationUserOtpRepository.CommitAsync();

            var subject = "Reset Your Password - MovieTickets";

            var body = $@"
<div style='font-family:Arial'>
    <h2>Password Reset Request</h2>
    <p>Your OTP code is:</p>

    <h1 style='color:#dc3545; letter-spacing:5px'>{otp}</h1>

    <p>This code will expire in <b>5 minutes</b>.</p>

    <hr/>
    <small>If you didn't request this, ignore this email.</small>
</div>";

            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return RedirectToAction(nameof(ValidateOtp), new { userId = user.Id });
        }
        [HttpGet]
        public IActionResult ValidateOtp(string userId)
        {
            return View(new ValidateOtpVM() { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOtp(ValidateOtpVM validateOtpVM)
        {
            if (!ModelState.IsValid)
            {
                return View(validateOtpVM);
            }
            var user = await _userManager.FindByIdAsync(validateOtpVM.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "invalid user ");
                return View(validateOtpVM);

            }
            var otps = await _applicationUserOtpRepository.GetAsync(e =>
               e.ApplicationUserId == user.Id &&
               e.IsValid == true &&
               e.ValidTo >= DateTime.UtcNow
                );
            var otp = otps.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
            if (otp is null || otp.Code != validateOtpVM.OTP)
            {
                ModelState.AddModelError("", "invalid / Expired OTP ");
                return View(validateOtpVM);
            }
            otp.IsValid = false;
            await _applicationUserOtpRepository.CommitAsync();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            TempData["token"] = token;
            return RedirectToAction(nameof(NewPassword), new { userId = user.Id });
        }

        [HttpGet]
        public IActionResult NewPassword(string userId)
        {
            var token = TempData["token"] as string;
            if (token is null)
            {
                return RedirectToAction(nameof(Login));
            }
            return View(new NewPasswordVM() { UserId = userId, Token = token });
        }
        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (newPasswordVM.Token is null)
            {
                return RedirectToAction(nameof(Login));
            }
            var user = await _userManager.FindByIdAsync(newPasswordVM.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "invalid user ");
                return View(newPasswordVM);

            }
            var result = await _userManager.ResetPasswordAsync(user, newPasswordVM.Token, newPasswordVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(newPasswordVM);
            }
            TempData["Success"] = "Your password has been updated successfully. You can now login.";

            return RedirectToAction(nameof(Login));
        }

    }
}
