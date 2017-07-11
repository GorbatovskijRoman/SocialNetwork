﻿using System.Web;
using System.Web.Mvc;
using MVC.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using MVC.Models;
using System.Threading.Tasks;

namespace MVC.Controllers
{
    
    public class AdminController : Controller
    {
        [Authorize(Roles ="Administrators")]
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

        [Authorize(Roles = "Administrators")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Administrators")]
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

        [Authorize(Roles = "Administrators")]
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

        [Authorize(Roles = "Administrators")]
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

        [Authorize(Roles = "Administrators")]
        [HttpPost]
        public async Task<ActionResult> Create(AppUser model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser { UserName = model.UserName, Email = model.Email };
                IdentityResult result =
                    await UserManager.CreateAsync(user, model.PasswordHash);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
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