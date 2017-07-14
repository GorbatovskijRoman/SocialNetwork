﻿using System.Web;
using System.Web.Mvc;
using MVC.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using MVC.Models;
using System.Threading.Tasks;
using MVC.Filters;

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
        
        
        public async Task<ActionResult> Edit(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult> Edit(string id, string email, string password, string sex)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Email = email;
                IdentityResult validEmail  = await UserManager.UserValidator.ValidateAsync(user);
                IdentityResult validPass = await UserManager.PasswordValidator.ValidateAsync(password);

                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }
                
                if (validPass.Succeeded)
                {
                    user.PasswordHash =  UserManager.PasswordHasher.HashPassword(password);
                }
                else
                {
                    AddErrorsFromResult(validPass);
                }
                
                if (validEmail.Succeeded && validPass.Succeeded)
                {
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Пользователь не найден");
            }
            return View(user);
        }
        
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);

            if (user != null)
            {
                IdentityResult result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            else
            {
                return View("Error", new string[] { "Пользователь не найден" });
            }
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