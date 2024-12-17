using Microsoft.AspNetCore.Mvc;
using CVBuilder.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CVBuilder.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Login page
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: Login action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the user exists in the database
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    // Validate the password
                    var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

                    if (signInResult.Succeeded)
                    {
                        // Redirect to a dashboard or home page on successful login
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Add error message if invalid password
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                else
                {
                    // Add error message if user not found
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            // If validation fails, return the view with the model to show validation errors
            return View(model);
        }

        // GET: Register page
        public IActionResult Register()
        {
            return View(new RegisterViewModel()); // Return an empty RegisterViewModel for the form
        }

        // POST: Register action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? userName = model.Username;
                var existingUser = await _userManager.FindByNameAsync(userName);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Username already exists.");
                    return View(model);
                }

                var user = new IdentityUser
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }
    }
}
