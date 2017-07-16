using System.Web;
using System.Web.Mvc;
using MVC.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using MVC.Models;
using System.Threading.Tasks;
using MVC.Filters;
using System.Linq;

namespace MVC.Controllers
{
    [Authorize(Roles = "Administrators")]
    [BlockUsers]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View(UserManager.Users);
        }

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppRoleManager>();
            }
        }

        public async Task<ActionResult> BlockUser(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Admin = false;
                user.ReLogin = true;
                await UserManager.RemoveFromRolesAsync(user.Id, UserManager.GetRoles(user.Id).ToArray());
                UserManager.AddToRole(user.Id, "Bans");
                await UserManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ResetPass(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.ResetPass = true;
                await UserManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> MakeAdmin(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Admin = true;
                user.ReLogin = true;
                await UserManager.RemoveFromRolesAsync(user.Id, UserManager.GetRoles(user.Id).ToArray());
                UserManager.AddToRole(user.Id, "Administrators");
                await UserManager.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

    }
}