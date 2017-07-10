using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using MVC.Infrastructure;
using MVC.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Web;

namespace MVC.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private AppUser currentUser = new AppUser();

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Wall");
            }

            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel details, bool captchaValid, string returnUrl)
        {

            if (details.Email == null || details.Password == null || details.Name == null)
            {
                ModelState.AddModelError("", "Заполните все поля");
            }
            else
            {
                AppUser user = await UserManager.FindByEmailAsync(details.Email);
                AppUser userByName = await UserManager.FindAsync(details.Name, details.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Такая почта не зарегистрирована");
                }
                else if (userByName == null)
                {
                    ModelState.AddModelError("", "Неправильно указан логин или пароль");
                }
                else if (!captchaValid)
                {
                    ModelState.AddModelError("", "Неверная капча");
                }
                else
                {
                    ClaimsIdentity ident = UserManager.CreateIdentity(user,
                        DefaultAuthenticationTypes.ApplicationCookie);

                    AuthManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = false
                    }, ident);

                    if (returnUrl != "") return Redirect(returnUrl);

                    TempData["email"] = user.Email;

                    return RedirectToAction("Wall");
                }
            }
            return View(details);
        }

        //выход
        [Authorize]
        public ActionResult Click()
        {
            AuthManager.SignOut();
            return View("Login");
        }


        [Authorize]
        public ActionResult Wall()
        {
            if (TempData.ContainsKey("email"))
            {
                currentUser.Email = TempData["email"] + "";
                currentUser = UserManager.FindByEmail(currentUser.Email);
            }
            else
            {
                string id = User.Identity.GetUserId();
                currentUser = UserManager.FindById(id);
            }
            return View(currentUser);
        }

        [AllowAnonymous]
        public ActionResult Registrate()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Registrate(LoginViewModel details, bool captchaValid)
        {
            AppUser user = new AppUser { UserName = details.Name, Email = details.Email };
            if (user != null)
            {
                user.Email = details.Email;
                IdentityResult validEmail = await UserManager.UserValidator.ValidateAsync(user);
                IdentityResult validPass = await UserManager.PasswordValidator.ValidateAsync(details.Password);
                
                if (!validEmail.Succeeded)
                {
                    foreach (string error in validEmail.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (validPass.Succeeded)
                {
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(details.Password);
                }
                else
                {
                    foreach (string error in validPass.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                if(!captchaValid)
                {
                      ModelState.AddModelError("", "Неверная капча");
                }

                if (validEmail.Succeeded && validPass.Succeeded && captchaValid)
                {
                    IdentityResult result = UserManager.Update(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Wall");
                    }
                    else
                    {
                        foreach (string error in result.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                }
            }
            return View(details);
        }

        private IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }
    }
}