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
using System.Linq;
using System.Collections.Generic;

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

        AppUser currentUser = new AppUser();
        AppSocialNetworkBDContext context = new AppSocialNetworkBDContext();

        [Authorize]
        public ActionResult Wall()
        {
            string id = User.Identity.GetUserId();
            currentUser = context.Users.Where(x => x.Id == id).First();
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
            currentUser = context.Users.Where(x => x.Id == id).First();
            return View(currentUser);
        }

        [Authorize]
        public ActionResult Subscribers(int page=1)
        {
            int pageSize = 10;
            string id = User.Identity.GetUserId();
            currentUser = context.Users.Where(x => x.Id == id).First();
            
            
            var subscribersBD = context.Subscribes.Where(x => x.OwnerId == currentUser.Id).Select(s=>s.AppUsers.Id).ToList();
            ViewBag.subscribers = subscribersBD.ToPagedList(page, pageSize);

            return View(currentUser);
        }

        [Authorize]
        public ActionResult Settings()
        {
            string id = User.Identity.GetUserId();
            currentUser = context.Users.Where(x => x.Id == id).First();
            return View(currentUser);
        }

        [HttpPost]
        public ActionResult ChangeUserPartial()
        {
            string id = User.Identity.GetUserId();
            currentUser = context.Users.Where(x => x.Id == id).First();
            return PartialView(currentUser);
        }

        [HttpPost]
        public ActionResult BlackListPartial()
        {
            string id = User.Identity.GetUserId();
            List<string> subscribes = context.Subscribes.Where(x => x.OwnerId == id).Select(s => s.AppUsers.Id).ToList();
            return PartialView(subscribes);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Update(AppUser user)
        {
            string id = User.Identity.GetUserId();
            currentUser = context.Users.Where(x => x.Id == id).First();
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
            return RedirectToAction("Settings");
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UnBlock(AppUser user)
        {
            string id = User.Identity.GetUserId();
            currentUser = context.Users.Where(x => x.Id == id).First();
            var unBlockUser = context.Blocks
                                       .Where(x => x.AppUsers.Id == user.Id && x.OwnerId == id)
                                       .ToList();
            context.Blocks.Remove(unBlockUser[0]); 
            await context.SaveChangesAsync();
            return RedirectToAction("Settings");
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UnSubscribe(string BlockId, int pageNum = 1)
        {
            string id = User.Identity.GetUserId();
            currentUser = context.Users.Where(x => x.Id == id).First();
            var unSubscribeUser = context.Subscribes
                                          .Where(x => x.AppUsers.Id == BlockId && x.OwnerId ==id)
                                          .ToList();
            context.Subscribes.Remove(unSubscribeUser[0]);
            await context.SaveChangesAsync();
            return RedirectToAction("Subscribers", new { page= pageNum});
        }

        [Authorize]
        public ActionResult Random()
        {

            AppUser[] mass = new AppUser[100];
            System.Random rand = new System.Random();
            for(int i=0;i<100;i++)
            {
                mass[i] = new AppUser()
                {
                    Admin = false,
                    Email = rand.Next(10000, 110000) + "@mail.ru",
                    UserName = rand.Next(10000, 110000) + "",
                    PasswordHash = UserManager.PasswordHasher.HashPassword("Qwerty1234@")
                };
                Block block = new Block() { AppUsers = mass[i], OwnerId = User.Identity.GetUserId() };
                Subscribe subscribe = new Subscribe() { AppUsers = mass[i], OwnerId = User.Identity.GetUserId() };
                context.Blocks.Add(block);
                context.Subscribes.Add(subscribe);
            }
            
            context.SaveChanges();
            return RedirectToAction("Wall");
        }
    }
}