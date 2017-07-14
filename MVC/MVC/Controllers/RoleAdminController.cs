using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.ComponentModel.DataAnnotations;
using MVC.Infrastructure;
using MVC.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using MVC.Filters;

namespace MVC.Controllers
{
    [Authorize(Roles = "Administrators")]
    [BlockUsers]
    public class RoleAdminController : Controller
    {
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

     
        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }
                

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        
        public async Task<ActionResult> Edit(string id)
        {
            AppRole role = await RoleManager.FindByIdAsync(id);
            string[] memberIDs = role.Users.Select(x => x.UserId).ToArray();

            IEnumerable<AppUser> members
                = UserManager.Users.Where(x => memberIDs.Any(y => y == x.Id));

            IEnumerable<AppUser> nonMembers = UserManager.Users.Except(members);

            return View(new RoleEditModel
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }
        
        [HttpPost]
        public async Task<ActionResult> Edit(RoleModificationModel model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.IdsToAdd ?? new string[] { })
                {
                    foreach(var role in await UserManager.GetRolesAsync(userId))
                    {
                        if(await UserManager.IsInRoleAsync(userId, role))
                        {
                           await UserManager.RemoveFromRoleAsync(userId, role);
                        }
                    }
                    result = await UserManager.AddToRoleAsync(userId, model.RoleName);
                }
                foreach (string userId in model.IdsToDelete ?? new string[] { })
                {
                    result = await UserManager.RemoveFromRoleAsync(userId, model.RoleName);
                    if (model.RoleName != "Users") await UserManager.AddToRoleAsync(userId, "Users");
                    else await UserManager.AddToRoleAsync(userId, "Bans");
                }
            }
            return RedirectToAction("Index");
        }
    }
}