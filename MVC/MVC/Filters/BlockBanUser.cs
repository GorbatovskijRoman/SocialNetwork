using System.Web.Routing;
using System.Web.Mvc;
using MVC.Models;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using MVC.Infrastructure;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Microsoft.Owin.Security;

namespace MVC.Filters
{
    public class BlockUsersAttribute: AuthorizeAttribute
    {

        public override void  OnAuthorization(AuthorizationContext filterContext)
        {
            IAuthenticationManager auth = HttpContext.Current.GetOwinContext().Authentication;
            AppUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<AppUserManager>();
            AppUser user = userManager.FindByName(filterContext.HttpContext.User.Identity.Name);

            if (user!=null)
            {
                if (user.ReLogin)
                {
                    auth.SignOut();
                    ClaimsIdentity ident = userManager.CreateIdentity(user,
                            DefaultAuthenticationTypes.ApplicationCookie);

                    auth.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = false
                    }, ident);
                    user.ReLogin = false;
                    userManager.Update(user);
                }
            }
            else
            {
                auth.SignOut();
            }

            if (filterContext.HttpContext.User.IsInRole("Bans"))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                                      { "controller", "BlockUsers" }, { "action", "AccessDenied" } });
            }
        }
    }
}