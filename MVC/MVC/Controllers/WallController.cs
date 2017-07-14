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
        public async Task<ActionResult> Wall()
        {
            string id = User.Identity.GetUserId();
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
            return View(currentUser);
        }

        [Authorize]
        public ActionResult Click()
        {
            AuthManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public async Task<ActionResult> Messages()
        {
            string id = User.Identity.GetUserId();
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
            return View(currentUser);
        }

        [Authorize]
        public async Task<ActionResult> Subscribers(int page=1)
        {
            int pageSize = 10;
            string id = User.Identity.GetUserId();
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
            var subscribersBD = context.Subscribes.Where(x => x.OwnerId == currentUser.Id).Select(s=>s.AppUsers.Id).ToList();
            ViewBag.subscribers = subscribersBD.ToPagedList(page, pageSize);
            return View(currentUser);
        }

        [Authorize]
        public async Task<ActionResult> Settings()
        {
            string id = User.Identity.GetUserId();
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
            return View(currentUser);
        }

        [HttpPost]
        public async Task<ActionResult> ChangeUserPartial()
        {
            string id = User.Identity.GetUserId();
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
            return PartialView(currentUser);
        }
        
        [HttpPost]
        public async Task<ActionResult> BlackListPartial()
        {
            string id = User.Identity.GetUserId();
            List<string> blocks = await context.Blocks.Where(x => x.OwnerId == id).Select(s => s.AppUsers.Id).ToListAsync();
            return PartialView(blocks);
        }

        [HttpPost]
        public async Task<ActionResult> AvatarPartial()
        {
            string id = User.Identity.GetUserId();
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Update(AppUser user)
        {
            string id = User.Identity.GetUserId();
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
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
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
            var unBlockUser = context.Blocks
                                       .Where(x => x.AppUsers.Id == user.Id && x.OwnerId == id)
                                       .ToList();
            context.Blocks.Remove(unBlockUser[0]); 
            await context.SaveChangesAsync();
            return RedirectToAction("Settings");
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> UnSubscribe(string SubscriberId, int pageNum = 1)
        {
            string id = User.Identity.GetUserId();
            currentUser = await context.Users.Where(x => x.Id == id).FirstAsync();
            var unSubscribeUser = await context.Subscribes
                                          .Where(x => x.AppUsers.Id == SubscriberId && x.OwnerId ==id)
                                          .ToListAsync();
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

        [Authorize]
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
            return RedirectToAction("Settings");
        }
    }
}