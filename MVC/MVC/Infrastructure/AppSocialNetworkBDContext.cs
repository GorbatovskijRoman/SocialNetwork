using MVC.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace MVC.Infrastructure
{
    public class AppSocialNetworkBDContext : IdentityDbContext<AppUser>
    {
        public AppSocialNetworkBDContext() : base("name=SocialNetworkBD") { }

        static AppSocialNetworkBDContext()
        {
            Database.SetInitializer<AppSocialNetworkBDContext>(new SocialNetworkBDInit());
        }

        public static AppSocialNetworkBDContext Create()
        {
            return new AppSocialNetworkBDContext();
        }
    }

    public class SocialNetworkBDInit : DropCreateDatabaseIfModelChanges<AppSocialNetworkBDContext>
    {
        protected override void Seed(AppSocialNetworkBDContext context)
        {
            PerformInitialSetup(context);
            base.Seed(context);
        }
        public void PerformInitialSetup(AppSocialNetworkBDContext context)
        {
            AppUserManager userMgr = new AppUserManager(new UserStore<AppUser>(context));
            AppRoleManager roleMgr = new AppRoleManager(new RoleStore<AppRole>(context));

            string roleName = "Administrators";
            string userName = "Admin";
            string password = "Qwerty1234@";
            string email = "gorbatovskij.roman@mail.ru";

            if (!roleMgr.RoleExists(roleName))
            {
                roleMgr.Create(new AppRole(roleName));
            }

            AppUser user = userMgr.FindByName(userName);
            if (user == null)
            {
                userMgr.Create(new AppUser { UserName = userName, Email = email },
                    password);
                user = userMgr.FindByName(userName);
            }

            if (!userMgr.IsInRole(user.Id, roleName))
            {
                userMgr.AddToRole(user.Id, roleName);
            }
        }
    }
}