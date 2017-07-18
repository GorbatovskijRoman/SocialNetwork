using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MVC.Filters;
using MVC.Infrastructure;
using MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Controllers
{
    [Authorize]
    [BlockUsers]
    public class ChatController : Controller
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
        
        public ActionResult Message()
        {
            string id = User.Identity.GetUserId();
            var rec = context.Message.Where(x => x.SenderId.Id ==id).Select(x => x.RecieverId);
            var sen = context.Message.Where(x => x.RecieverId.Id == id).Select(x => x.SenderId);
            var all = rec.Union(sen).ToList();                       
                
            ViewBag.Users = all;
            return View(currentUser);
        }
        
        [HttpPost]
        public ActionResult MessagePartial(string recieverId)
        {
            string id = User.Identity.GetUserId();
            var part1 = context.Message.Where(x => x.RecieverId.Id == recieverId && x.SenderId.Id == id).Select(x=>x.SenderId.UserName).ToArray();
            var part2 = context.Message.Where(x => x.RecieverId.Id == id && x.SenderId.Id == recieverId).Select(x => x.SenderId.UserName).ToArray();

            var mes1 = context.Message.Where(x => x.RecieverId.Id == recieverId && x.SenderId.Id == id).ToArray();
            var mes2 = context.Message.Where(x => x.RecieverId.Id == id && x.SenderId.Id == recieverId).ToArray();

            var allName = part1.Concat(part2).ToArray();
            var allMess = mes1.Concat(mes2).ToArray();

            if (allName != null)
            {
                List<ChatContent> chat = new List<ChatContent>();
                for(int i=0;i<allName.Count();i++)
                {
                    chat.Add
                        (
                            new ChatContent()
                            {
                                Name = allName[i],
                                Text = allMess[i].MessageText,
                                Time = allMess[i].SendTime
                            }
                        );
                }

                ViewBag.ChatContent = chat;
                return PartialView();
            }
            else return PartialView();
        }
    }
}