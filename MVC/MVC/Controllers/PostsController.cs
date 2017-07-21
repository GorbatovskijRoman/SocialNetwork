using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC.Filters;
using MVC.Models;
using Microsoft.Owin.Security;
using MVC.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace MVC.Controllers
{
    [Authorize]
    [BlockUsers]
    public class PostsController : Controller
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

        [HttpPost]
        public ActionResult AddPost(string Content, string to)
        {
            if (Content != "")
            {
                AppUser Sender = context.Users.Find(to);
                context.Posts.Add(
                          new WallPost()
                          {
                              Content = Content,
                              LikeCount = 0,
                              Time = DateTime.Now,
                              Wall = Sender,
                              Owner = currentUser
                          });
            }
            context.SaveChanges();
            return RedirectToAction("Wall", "Wall", new { OwnerId = to });
        }

        public void Delete(int id)
        {
            WallPost wp = context.Posts.Find(id);
            if(wp!=null)
            {
                if(wp.Owner==currentUser || wp.Wall == currentUser)
                {
                    context.Posts.Remove(wp);
                }
            }
        }
    }
}