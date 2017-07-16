using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MVC.Infrastructure;
using MVC.Models;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Data.Entity;
using System.IO;
using MVC.Filters;

namespace MVC.Controllers
{
    [Authorize]
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

        AppSocialNetworkBDContext context = new AppSocialNetworkBDContext();
            
        AppUser currentUser
        {
            get
            {
                string id = User.Identity.GetUserId();
                return context.Users.Where(x => x.Id == id).First();
            }
        }

        [BlockUsers]
        public ActionResult Wall()
        {

            string id = User.Identity.GetUserId();

            if (currentUser.ResetPass)
            {
                return RedirectToAction("Settings", "Wall");
            }
            else return View(currentUser);
        }
        
        //SignOut
        public ActionResult Click()
        {
            AuthManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        [BlockUsers]
        public ActionResult Messages()
        {
            return View(currentUser);
        }
        [BlockUsers]
        public ActionResult Subscribers(int page=1)
        {
            int pageSize = 10;
            var subscribersBD = context.Subscribes.Where(x => x.OwnerId == currentUser.Id).Select(s=>s.AppUsers.Id).ToList();
            ViewBag.subscribers = subscribersBD.ToPagedList(page, pageSize);
            return View(currentUser);
        }

        [BlockUsers]
        public ActionResult Settings()
        {
            return View(currentUser);
        }

        [BlockUsers]
        [HttpPost]
        public ActionResult ChangeUserPartial()
        {
            return PartialView(currentUser);
        }

        [BlockUsers]
        [HttpPost]
        public ActionResult ChangePassPartial()
        {
            ViewBag.ID = currentUser.Id;
            PasswordChangeModel pasCh = new PasswordChangeModel();
            return PartialView(pasCh);
        }

        [BlockUsers]
        [HttpPost]
        public async Task<ActionResult> BlackListPartial()
        {
            string id = User.Identity.GetUserId();
            List<string> blocks = await context.Blocks.Where(x => x.OwnerId == id).Select(s => s.AppUsers.Id).ToListAsync();
            return PartialView(blocks);
        }

        [BlockUsers]
        [HttpPost]
        public ActionResult AvatarPartial()
        {
            if (currentUser.Avatar == null)
            {
                ViewBag.imageArray = null;
                return PartialView();
            }
            else
            {

                ViewBag.imageArray = currentUser.Avatar;
                return PartialView();
            }
        }

        [BlockUsers]
        [HttpPost]
        public async Task<ActionResult> UpdatePass(PasswordChangeModel passChange)
        {
            if (ModelState.IsValid)
            {
                IdentityResult validOld = await UserManager.PasswordValidator.ValidateAsync(passChange.OldPass);
                IdentityResult validNew = await UserManager.PasswordValidator.ValidateAsync(passChange.NewPass);

                if (validNew.Succeeded && validOld.Succeeded)
                {
                    if (UserManager.PasswordHasher.VerifyHashedPassword(currentUser.PasswordHash, 
                                                                        passChange.OldPass)
                                                                        ==PasswordVerificationResult.Success)
                    {
                        currentUser.PasswordHash = UserManager.PasswordHasher.HashPassword(passChange.NewPass);
                        await context.SaveChangesAsync();
                        ModelState.AddModelError("", "Password changed");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Old password did not match with current");
                    }
                }
                else
                {
                    foreach (string error in validOld.Errors.Union(validNew.Errors))
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            else ModelState.AddModelError("","Password is not valid");
            ViewBag.ID = currentUser.Id;
            return PartialView("ChangePassPartial", new PasswordChangeModel());
        }

        [BlockUsers]
        [HttpPost]
        public async Task<ActionResult> UpdateName(AppUser user)
        {
            if (ModelState.IsValid)
            {
                if (user.UserName != "")
                {
                    user.Email = currentUser.Email;
                    IdentityResult validUser = await UserManager.UserValidator.ValidateAsync(user);

                    if (validUser.Succeeded)
                    {
                        currentUser.UserName = user.UserName;
                        await context.SaveChangesAsync();
                        ModelState.AddModelError("", "Name changed");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Name is not valid");
                    }
                }
                else ModelState.AddModelError("","Name is empty");
            }
            else
            {
                ModelState.AddModelError("", "Name is uncorrect");
            }
            return PartialView("ChangeUserPartial", currentUser);
        }

        [BlockUsers]
        [HttpPost]
        public async Task<ActionResult> UnBlock(AppUser user)
        {
            var unBlockUser = context.Blocks
                                       .Where(x => x.AppUsers.Id == user.Id && x.OwnerId == currentUser.Id)
                                       .ToList();
            context.Blocks.Remove(unBlockUser[0]); 
            await context.SaveChangesAsync();
            return PartialView("BlackListPartial", currentUser);
        }

        [BlockUsers]
        [HttpPost]
        public async Task<ActionResult> UnSubscribe(string SubscriberId, int pageNum = 1)
        {
            var unSubscribeUser = await context.Subscribes
                                          .Where(x => x.AppUsers.Id == SubscriberId && x.OwnerId == currentUser.Id)
                                          .ToListAsync();
            context.Subscribes.Remove(unSubscribeUser[0]);
            await context.SaveChangesAsync();
            return RedirectToAction("Subscribers", new { page= pageNum});
        }

        [BlockUsers]
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

        [BlockUsers]
        public async Task<ActionResult> UpdateAvatar(HttpPostedFileBase file)
        {
            if (file!=null)
            {
                Image sourceimage =  Image.FromStream(file.InputStream);
                string id = User.Identity.GetUserId();
               
                using (var ms = new MemoryStream())
                {
                    sourceimage.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                    context.Users.Where(x => x.Id == id).First().Avatar = ms.ToArray();
                    await context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Wall");
        }
    }
}