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
using System.IO;
using MVC.Filters;
using System;
using System.ServiceModel.Syndication;

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
        public ActionResult Wall(string OwnerId = "")
        {
            string id = User.Identity.GetUserId();

            if (currentUser.ResetPass)
            {
                return RedirectToAction("Settings", "Wall");
            }
            else
            {
                if (OwnerId == "")
                {
                    ViewBag.Posts = context.Posts.Where(l => l.Wall.Id == currentUser.Id).OrderByDescending(x => x.Time).ToList();
                    if (context.Subscribes.Find(id) != null)
                        ViewBag.Subscribers = context.Subscribes.Find(id).UserSubscribers.Count();
                    else ViewBag.Subscribers = 0;
                    return View(currentUser);
                }
                else
                {
                    AppUser owner = context.Users.Find(OwnerId);

                    if (owner != null && owner.Id != currentUser.Id)
                    {
                        ViewBag.Owner = owner;
                        ViewBag.Block = false;
                        ViewBag.Ban = false;
                        ViewBag.Subscribers = 0;
                        ViewBag.Subscribe = true;
                        ViewBag.Posts = context.Posts.Where(z => z.Wall.Id == owner.Id).OrderByDescending(x => x.Time).ToList();

                        if (context.Blocks.Find(currentUser.Id) != null && context.Blocks.Find(currentUser.Id).UserBlocks.Contains(owner))
                        {
                            ViewBag.Block = true;
                        }
                        if (context.Blocks.Find(OwnerId) != null && context.Blocks.Find(OwnerId).UserBlocks.Contains(currentUser))
                        {
                            ViewBag.Ban = true;
                        }

                        if (context.Subscribes.Find(OwnerId) != null && context.Subscribes.Find(OwnerId).UserSubscribers.Contains(currentUser))
                        {
                            ViewBag.Subscribers = context.Subscribes.Find(OwnerId).UserSubscribers.Count();
                            ViewBag.Subscribe = false;
                        }

                        return View("GuestWall", currentUser);
                    }
                    else
                    {
                        if (context.Subscribes.Find(currentUser.Id) != null)
                        {
                            ViewBag.Subscribers = context.Subscribes.Find(currentUser.Id).UserSubscribers.Count();
                        }
                        else ViewBag.Subscribers = 0;
                        ViewBag.Posts = context.Posts.Where(z => z.Wall.Id == currentUser.Id).OrderByDescending(x => x.Time).ToList();
                        return View(currentUser);
                    }
                }
            }
        }

        SyndicationFeed feed = new SyndicationFeed("Social", "Network", new Uri("http://localhost:52865/Wall/Rss"),
                Guid.NewGuid().ToString(), DateTime.Now);
        List<SyndicationItem> items = new List<SyndicationItem>();
        

        public RssActionResult Rss()
        {
            var mySubscribers = context.Subscribes.Where(x => x.UserSubscribers.Where(z => z.Id == currentUser.Id).Count() != 0);
            if (mySubscribers.Count() != 0)
            {
                List<WallPost> wp = new List<WallPost>();
                DateTime dt = DateTime.Now.AddMinutes(-30);
                foreach (Subscribe user in mySubscribers)
                {
                    var posts = context.Posts.Where(x => x.Owner.Id == user.AppUserId && x.Time >= dt).ToList();
                    if (posts.Count() != 0)
                    {
                        wp.AddRange(posts);
                    }
                }
                if (wp.Count != 0)
                {
                    items = new List<SyndicationItem>();
                    foreach (WallPost post in wp)
                    {
                        SyndicationItem item = new SyndicationItem(post.Owner.UserName, post.Content,
                        new Uri("http://localhost:52865/Wall/Wall?OwnerId=" + post.Owner.Id));
                        items.Add(item);
                    }
                }
            }
            feed.Items = items;
            return new RssActionResult() { Feed = feed };
        }

        public ActionResult Users()
        {
            var Users = context.Users;
            if (Users != null && Users.Count() != 0)
            {
                ViewBag.Users = Users.ToList();
                return View(currentUser);
            }
            else
            {
                return RedirectToAction("Wall", "Wall");
            }
        }

        public ActionResult News(int hours = 1)
        {
            var mySubscribers = context.Subscribes.Where(x => x.UserSubscribers.Where(z => z.Id == currentUser.Id).Count() != 0);
            if (mySubscribers.Count() != 0)
            {
                List<WallPost> wp = new List<WallPost>();
                DateTime dt = DateTime.Now.AddHours(-hours);
                foreach (Subscribe user in mySubscribers)
                {
                    var posts = context.Posts.Where(x => x.Owner.Id == user.AppUserId && x.Time >= dt).ToList();
                    if (posts.Count() != 0)
                    {
                        wp.AddRange(posts);
                    }
                }
                if (wp.Count != 0)
                {
                    ViewBag.News = wp.OrderByDescending(x => x.Time).ToList();
                }

            }
            return View(currentUser);
        }

        //SignOut
        public ActionResult Click()
        {
            AuthManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        [BlockUsers]
        public ActionResult Subscribers(int page = 1)
        {
            int pageSize = 10;
            if (context.Subscribes.Where(x => x.AppUserId == currentUser.Id).Count() != 0)
            {
                var subscribersBD = context.Subscribes.Where(x => x.AppUserId == currentUser.Id).
                                                     First().UserSubscribers.
                                                     Select(s => s.Id).ToList();
                ViewBag.subscribers = subscribersBD.ToPagedList(page, pageSize);
            }
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
        public ActionResult BlackListPartial()
        {
            string id = User.Identity.GetUserId();
            List<string> blocks = new List<string>();
            if (context.Blocks.Where(x => x.AppUserId == currentUser.Id).Count() != 0)
            {
                blocks = context.Blocks.Where(x => x.AppUserId == currentUser.Id).
                                                   First().UserBlocks.
                                                   Select(s => s.Id).ToList();
            }
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
                                                                        == PasswordVerificationResult.Success)
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
            else ModelState.AddModelError("", "Password is not valid");
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
                else ModelState.AddModelError("", "Name is empty");
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
            var CurUserBlocks = context.Blocks.Where(x => x.AppUserId == currentUser.Id).First();
            var unBlockUser = context.Blocks
                                       .Where(x => x.AppUserId == currentUser.Id).
                                       First().UserBlocks.Where(x => x.Id == user.Id).First();
            CurUserBlocks.UserBlocks.Remove(unBlockUser);

            await context.SaveChangesAsync();
            return RedirectToAction("Settings", "Wall");
        }

        [BlockUsers]
        [HttpPost]
        public async Task<ActionResult> UnSubscribe(string SubscriberId, int pageNum = 1)
        {
            var CurUserSubs = context.Subscribes.Where(x => x.AppUserId == currentUser.Id).First();
            var unSubsUser = context.Subscribes
                                       .Where(x => x.AppUserId == currentUser.Id).
                                       First().UserSubscribers.Where(x => x.Id == SubscriberId).First();
            CurUserSubs.UserSubscribers.Remove(unSubsUser);
            await context.SaveChangesAsync();
            return RedirectToAction("Subscribers", new { page = pageNum });
        }
        
        [BlockUsers]
        public async Task<ActionResult> UpdateAvatar(HttpPostedFileBase file)
        {
            if (file != null)
            {
                Image sourceimage = Image.FromStream(file.InputStream);
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