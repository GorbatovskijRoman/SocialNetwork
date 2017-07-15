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
                await UserManager.UpdateAsync(user);
                await UserManager.RemoveFromRolesAsync(user.Id, UserManager.GetRoles(user.Id).ToArray());
                UserManager.AddToRole(user.Id, "Bans");
                AppRole role = await RoleManager.FindByNameAsync("Bans");
                await RoleManager.UpdateAsync(role);
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ResetPass(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.ResetPass = true;
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> MakeAdmin(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Admin = true;
                await UserManager.UpdateAsync(user);
                await UserManager.RemoveFromRolesAsync(user.Id, UserManager.GetRoles(user.Id).ToArray());
                UserManager.AddToRole(user.Id, "Administrators");
                AppRole role = await RoleManager.FindByNameAsync("Administrators");
                await RoleManager.UpdateAsync(role);
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