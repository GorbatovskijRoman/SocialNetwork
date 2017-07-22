using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC.Infrastructure;
using MVC.Models;
using Microsoft.AspNet.Identity;
using MVC.Filters;

namespace MVC.Controllers
{
    [Authorize]
    [BlockUsers]
    public class GuestController : Controller
    {
        AppSocialNetworkBDContext context = new AppSocialNetworkBDContext();
        AppUser currentUser
        {
            get
            {
                string id = User.Identity.GetUserId();
                return context.Users.Where(x => x.Id == id).First();
            }
        }

        
        public ActionResult UnSubscribe(string OwnerId)
        {
            var CurUserSubs = context.Subscribes.Find(OwnerId);
            CurUserSubs.UserSubscribers.Remove(currentUser);
            context.SaveChanges();

            return RedirectToAction("Wall", "Wall",new { OwnerId = OwnerId });
        }

        public ActionResult UnBlock(string BlockId)
        {
            AppUser blockUser = context.Users.Find(BlockId);

            var CurUserBlocks = context.Blocks.Find(currentUser.Id);
            CurUserBlocks.UserBlocks.Remove(blockUser);
            context.SaveChanges();

            return RedirectToAction("Wall", "Wall", new { OwnerId = BlockId });
        }


        public ActionResult Subscribe(string OwnerId)
        {
            if (context.Subscribes.Find(OwnerId) != null)
            {
                context.Subscribes.Find(OwnerId).UserSubscribers.Add(currentUser);
            }
            else
            {
                Subscribe sub = new Subscribe()
                {
                    AppUserId = OwnerId,
                    UserSubscribers = new List<AppUser>() { currentUser}
                };
                context.Subscribes.Add(sub);
                
            }
            context.SaveChanges();
            return RedirectToAction("Wall", "Wall", new { OwnerId = OwnerId });
        }

        public ActionResult Block(string BlockId)
        {
            AppUser blockUser = context.Users.Find(BlockId);
            if (context.Blocks.Find(currentUser.Id) != null)
            {
                context.Blocks.Find(currentUser.Id).UserBlocks.Add(blockUser);
            }
            else
            {
                Block bl = new Block()
                {
                    AppUserId = currentUser.Id,
                    UserBlocks = new List<AppUser>() { blockUser }
                };
                context.Blocks.Add(bl);
            }
            context.SaveChanges();
            return RedirectToAction("Wall", "Wall", new { OwnerId = BlockId });
        }

    }
}