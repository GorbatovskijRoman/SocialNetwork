using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;

namespace MVC.Infrastructure
{
    public static class IdentityHelpers
    {
        public static MvcHtmlString GetUserName( string id)
        {
            AppUserManager mgr = HttpContext.Current
                .GetOwinContext().GetUserManager<AppUserManager>();

            return new MvcHtmlString(mgr.FindByIdAsync(id).Result.UserName);
        }
    }
}