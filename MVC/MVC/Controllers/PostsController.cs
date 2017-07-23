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
        
        public ActionResult AddPost(string Content, string to)
        {
            
            if (Content != "")
            {
                if (context.Blocks.Find(to) == null || !context.Blocks.Find(to).UserBlocks.Contains(currentUser))
                {
                    AppUser Sender = context.Users.Find(to);
                    WallPost wp = new WallPost()
                    {
                        Content = Content,
                        LikeCount = 0,
                        Time = DateTime.Now,
                        Wall = Sender,
                        Owner = currentUser
                    };
                    context.Posts.Add(wp);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Wall", "Wall", new { OwnerId = to });
        }

        public ActionResult Delete(int id, string OwnerId)
        {
            WallPost wp = context.Posts.Find(id);
            if(wp!=null)
            {
                if(wp.Owner==currentUser || wp.Wall == currentUser)
                {
                    var likes = context.Likes.Where(x => x.PostId.Id == id);
                    context.Likes.RemoveRange(likes);
                    context.Posts.Remove(wp);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Wall", "Wall", new { OwnerId = OwnerId });
        }

        [HttpPost]
        public ActionResult Like(int id)
        {
            int count = 0;
            WallPost wp = context.Posts.Find(id);
            if (wp != null)
            {
                var likes = context.Likes.Where(x => x.PostId.Id == id && x.UserLike.Id == currentUser.Id);
                if ( likes!=null && likes.Count()!=0)
                {
                   WallPostLike Like = likes.First();
                   context.Likes.Remove(Like);
                   wp.LikeCount--;
                }
                else
                {
                    WallPostLike NewLike = new WallPostLike()
                    {
                        PostId = wp,
                        UserLike = currentUser
                    };
                    context.Likes.Add(NewLike);
                    wp.LikeCount++;
                }
                context.SaveChanges();
                count = wp.LikeCount;
            }
            ViewBag.Count = count;
            return PartialView("LikeCountPartial");
        }

        public ActionResult Repost(int id)
        {
            WallPost wp = context.Posts.Find(id);
            wp.Content = "Repost from " + wp.Owner.UserName + ":" +wp.Content;

            return RedirectToAction("AddPost", new { Content = wp.Content, to = currentUser.Id });
        }
    }
}