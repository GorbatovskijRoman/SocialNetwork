using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MVC.Infrastructure;
using MVC.Models;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using PagedList;

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

            if (currentUser == null)
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
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public ActionResult Messages()
        {
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
            currentUser.Admin = UserManager.IsInRole(currentUser.Id, "Administrators");
            return View(currentUser);
        }

        [Authorize]
        public ActionResult Subscribers(int page=1)
        {
            int pageSize = 10;
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
            currentUser.Admin = UserManager.IsInRole(currentUser.Id, "Administrators");
           // ViewBag.count = currentUser.Subscribers.Count;
            ViewBag.CurrentUser = currentUser;
            // return View(currentUser.Subscribers.ToPagedList(page,pageSize));
            return View(currentUser);
        }

        [Authorize]
        public ActionResult Settings()
        {
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
            currentUser.Admin = UserManager.IsInRole(currentUser.Id, "Administrators");
           // currentUser.BlockUser.Add("0eb47175-107f-4052-a25d-74807d684948");
           // currentUser.Subscribers.Add("0eb47175-107f-4052-a25d-74807d684948");
           // UserManager.Update(currentUser);
            currentUser = UserManager.FindById(id);
            var c = currentUser;
            c.Equals(null);
            return View(currentUser);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Update(AppUser user)
        {
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
            currentUser.Admin = UserManager.IsInRole(currentUser.Id, "Administrators");
            if (ModelState.IsValid)
            {
                if (user.OldPass != null && user.NewPass != null)
                {
                    user.Email = currentUser.Email;
                    IdentityResult validUser = await UserManager.UserValidator.ValidateAsync(user);
                    IdentityResult validPass = await UserManager.PasswordValidator.ValidateAsync(user.OldPass);

                    if (validUser.Succeeded)
                    {
                        if (validPass.Succeeded)
                        {
                            validPass = await UserManager.PasswordValidator.ValidateAsync(user.NewPass);
                            if (validPass.Succeeded)
                            {
                                if (PasswordVerificationResult.Success == UserManager.PasswordHasher.VerifyHashedPassword(currentUser.PasswordHash, user.OldPass))
                                {
                                    currentUser.PasswordHash = UserManager.PasswordHasher.HashPassword(user.NewPass);
                                    currentUser.UserName = user.UserName;
                                    IdentityResult result = await UserManager.UpdateAsync(currentUser);
                                    if (result.Succeeded)
                                    {
                                        currentUser = UserManager.FindById(currentUser.Id);
                                        return RedirectToAction("Settings", currentUser);
                                    }
                                    else
                                    {
                                        foreach (string error in result.Errors) ModelState.AddModelError("", error);
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("update", "Old password is uncorrect");
                                }
                            }
                            else
                            {
                                foreach (string error in validPass.Errors) ModelState.AddModelError("new", error);
                            }
                        }
                        else
                        {
                            foreach (string error in validPass.Errors) ModelState.AddModelError("old", error);
                        }
                    }
                    else
                    {
                        foreach (string error in validUser.Errors) ModelState.AddModelError("name", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("all", "Fill in all the fields");
                }
            }
            return View("Settings", currentUser);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UnBlock(AppUser user)
        {
            AppUser DeleteUser = await UserManager.FindByIdAsync(user.Id);
            IdentityResult result = await UserManager.DeleteAsync(DeleteUser);
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
            return View("Settings",currentUser);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UnSubscribe(string UnSubscribeId)
        {
            string id = User.Identity.GetUserId();
            currentUser = UserManager.FindById(id);
           // currentUser.Subscribers.Remove(UnSubscribeId);
            await UserManager.UpdateAsync(currentUser);
            return RedirectToAction("Subscribers", "Wall");
        }
    }
}