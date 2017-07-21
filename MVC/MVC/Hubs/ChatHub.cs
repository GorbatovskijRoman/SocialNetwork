using System.Threading.Tasks;
using MVC.Models;
using Microsoft.AspNet.SignalR;
using MVC.Infrastructure;
using System.Security.Principal;
using System.Linq;
using System;
using MVC.Filters;

namespace MVC
{
    [Authorize]
    [BlockUsers]
    public class ChatHub : Hub
    {
        AppSocialNetworkBDContext DbContext = new AppSocialNetworkBDContext();

        public void Send(string sender, string reciever, string message)
        {
            if (DbContext.Users.Where(x => x.UserName == reciever).Count() != 0)
            {
                AppUser user = DbContext.Users.Where(x => x.UserName == reciever).First();
                if (user.Id != sender)
                {
                    string recieverId = user.Id;

                    ConnectionInfo UserReciever = DbContext.ConInfo.Find(recieverId);
                    if (UserReciever != null)
                    {
                        if (UserReciever.StatusConnection)
                        {
                            Clients.Client(UserReciever.ConnectionId).addMessage(DbContext.Users.Find(sender).UserName, reciever, message);
                            Clients.Caller.addMessage(DbContext.Users.Find(sender).UserName, reciever, message);
                        }
                    }
                    MessageModel mes = new MessageModel()
                    {
                        RecieverId = user,
                        MessageText = message,
                        SenderId = DbContext.Users.Find(sender),
                        SendTime = DateTime.Now
                    };
                    DbContext.Message.Add(mes);
                    DbContext.SaveChanges();
                }
                else Clients.Caller.addMessage("", "", "This user is you.");
            }
            else Clients.Caller.addMessage("", "", "The user is no longer connected.");
        }

        public override Task OnConnected()
        {
            var users = DbContext.Users.Where(x => x.UserName == Context.User.Identity.Name);
            if (users.Count() != 0)
            {
                AppUser user = DbContext.Users.Where(x => x.UserName == Context.User.Identity.Name).First();

                if (DbContext.ConInfo.Where(x => x.AppUserId == user.Id).Count() == 0)
                {
                    ConnectionInfo ci = new ConnectionInfo()
                    {
                        AppUserId = user.Id,
                        ConnectionId = Context.ConnectionId,
                        StatusConnection = true
                    };
                    DbContext.ConInfo.Add(ci);
                }
                else
                {
                    ConnectionInfo ci = DbContext.ConInfo.Where(x => x.AppUserId == user.Id).First();
                    ci.ConnectionId = Context.ConnectionId;
                    ci.StatusConnection = true;
                }
                DbContext.SaveChanges();
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {

            DbContext.ConInfo.Where(x => x.ConnectionId == Context.ConnectionId).First().StatusConnection = false;
            DbContext.SaveChanges();
            return base.OnDisconnected(stopCalled);
        }
    }
}