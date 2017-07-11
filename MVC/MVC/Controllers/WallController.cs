using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MVC.Infrastructure;
using MVC.Models;
using System.Web;
using System.Web.Mvc;

namespace MVC.Controllers
{
    public class WallController : Controller
    {
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

        private AppUser currentUser = new AppUser();

        [Authorize]
        public ActionResult Wall()
        {
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);

            if(currentUser==null)
            {
                AuthManager.SignOut();
                return RedirectToAction("Login", "Account");
            }
            else
            {
                currentUser.Admin = UserManager.IsInRole(currentUser.Id, "Administrators");
            }
            return View(currentUser);
        }

        [Authorize]
        public ActionResult Click()
        {
            AuthManager.SignOut();
            return RedirectToAction("Login","Account");
        }

        [Authorize]
        public ActionResult Messages()
        {
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
            currentUser.Admin = UserManager.IsInRole(currentUser.Id, "ADministrators");
            return View(currentUser);
        }

        [Authorize]
        public ActionResult Subscribers()
        {
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
            currentUser.Admin = UserManager.IsInRole(currentUser.Id, "ADministrators");
            return View(currentUser);
        }

        [Authorize]
        public ActionResult Settings()
        {
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
            currentUser.Admin = UserManager.IsInRole(currentUser.Id, "ADministrators");
            return View(currentUser);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Settings(AppUser user, string pass)
        {

            if(PasswordVerificationResult.Success == UserManager.PasswordHasher.VerifyHashedPassword(currentUser.PasswordHash, pass))
            {
                currentUser = user;
                currentUser.PasswordHash = UserManager.PasswordHasher.HashPassword(pass);
                UserManager.Update(currentUser);
            }
            return View(currentUser);
        }
       
    }
}