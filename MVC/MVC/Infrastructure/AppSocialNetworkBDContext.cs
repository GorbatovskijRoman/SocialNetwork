using MVC.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace MVC.Infrastructure
{
    public class AppSocialNetworkBDContext : IdentityDbContext<AppUser>
    {
        public AppSocialNetworkBDContext() : base("name=SocialNetworkBD") { }
        public DbSet<Subscribe> Subscribes { get; set; }
        public DbSet<Block> Blocks { get; set; }
        public DbSet<MessageModel> Message { get; set; }
        public DbSet<ConnectionInfo> ConInfo { get; set; }
        public DbSet<WallPost> Posts { get; set; }
        public DbSet<WallPostComment> Comments { get; set; }

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
            Subscribe subscribe = new Subscribe();

            string roleNameForUsers = "Users";
            string roleNameForAdmin = "Administrators";
            string roleNameForBan = "Bans";
            string userName = "Admin";
            string password = "Qwerty1234@";
            string email = "gorbatovskij.roman@mail.ru";

            if (!roleMgr.RoleExists(roleNameForAdmin))
            {
                roleMgr.Create(new AppRole(roleNameForAdmin));
            }
            if (!roleMgr.RoleExists(roleNameForUsers))
            {
                roleMgr.Create(new AppRole(roleNameForUsers));
            }
            if (!roleMgr.RoleExists(roleNameForBan))
            {
                roleMgr.Create(new AppRole(roleNameForBan));
            }

            AppUser user = userMgr.FindByName(userName);
            if (user == null)
            {
                userMgr.Create(new AppUser { UserName = userName, Email = email, Admin = true, ReLogin = false, ResetPass = false },
                    password);
                user = userMgr.FindByName(userName);
            }

            if (!userMgr.IsInRole(user.Id, roleNameForAdmin))
            {
                userMgr.AddToRole(user.Id, roleNameForAdmin);
            }
        }
    }
}