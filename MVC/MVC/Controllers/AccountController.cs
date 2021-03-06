﻿using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using MVC.Infrastructure;
using MVC.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System;
using System.Linq;

namespace MVC.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }
        
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated )
            {
                return RedirectToAction("Wall","Wall");
            }

            ViewBag.returnUrl = returnUrl;
            return View();
        }

        AppUser[] mass = new AppUser[100];
        AppSocialNetworkBDContext context = new AppSocialNetworkBDContext();
        
        public ActionResult Vall()
        {
            for(int i=0;i<100;i++)
            {
                UserManager.Create(new AppUser() { Admin = false, Email = "user" + i+"@mail.ru", UserName = "user" + i, ReLogin = false, ResetPass = false },"Qwerty1234@");
                AppUser us = UserManager.FindByName("user" + i);
                UserManager.AddToRole(us.Id, "Users");
            }
            return View("Login");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel details, bool captchaValid, string returnUrl)
        {

            if (details.Email == null || details.Password == null || details.Name == null)
            {
                ModelState.AddModelError("", "Fill in all fields");
            }
            else
            {
                AppUser user = await UserManager.FindByEmailAsync(details.Email);
                AppUser userByName = await UserManager.FindAsync(details.Name, details.Password);

                if (user == null)
                {
                    ModelState.AddModelError("", "Uncorrect email");
                }
                else if (userByName == null)
                {
                    ModelState.AddModelError("", "Uncorrect login or password");
                }
                else if (!captchaValid)
                {
                    ModelState.AddModelError("", "Uncorrect capthcha");
                }
                else
                {
                    ClaimsIdentity ident = UserManager.CreateIdentity(user,
                        DefaultAuthenticationTypes.ApplicationCookie);

                    AuthManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = false
                    }, ident);
                    if (returnUrl != null && returnUrl!="") return Redirect(returnUrl);
                    else return RedirectToAction("Wall", "Wall");
                }
            }
            return View(details);
        }
        
        public ActionResult Registrate()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Wall", "Wall");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrate(LoginViewModel details, bool captchaValid)
        {
            AppUser user = new AppUser { UserName = details.Name, Email = details.Email };
            if (user != null)
            {
                if (details.Password != null)
                {
                    user.Email = details.Email;
                    user.ReLogin = false;
                    user.ResetPass = false;
                    IdentityResult validEmail = await UserManager.UserValidator.ValidateAsync(user);
                    IdentityResult validPass = await UserManager.PasswordValidator.ValidateAsync(details.Password);

                    if (!validEmail.Succeeded)
                    {
                        foreach (string error in validEmail.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }

                    if (validPass.Succeeded)
                    {
                        user.PasswordHash = UserManager.PasswordHasher.HashPassword(details.Password);
                    }
                    else
                    {
                        foreach (string error in validPass.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    if (!captchaValid)
                    {
                        ModelState.AddModelError("", "Uncorrect captcha");
                    }

                    if (validEmail.Succeeded && validPass.Succeeded && captchaValid)
                    {
                        IdentityResult result = UserManager.Create(user, details.Password);
                        if (result.Succeeded)
                        {
                            ClaimsIdentity ident = UserManager.CreateIdentity(user,
                           DefaultAuthenticationTypes.ApplicationCookie);

                            UserManager.AddToRole(user.Id, "Users");

                            AuthManager.SignIn(new AuthenticationProperties
                            {
                                IsPersistent = false
                            }, ident);

                            return RedirectToAction("Wall", "Wall");
                        }
                        else
                        {
                            foreach (string error in result.Errors)
                            {
                                ModelState.AddModelError("", error);
                            }
                        }
                    }
                }
            }
            return View(details);
        }
    }
}