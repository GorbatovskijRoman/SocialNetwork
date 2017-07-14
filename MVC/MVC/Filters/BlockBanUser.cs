using System.Web.Routing;
using System.Web.Mvc;

namespace MVC.Filters
{
    public class BlockUsersAttribute: AuthorizeAttribute
    {
        public override void  OnAuthorization(AuthorizationContext filterContext)
        {
            if(filterContext.HttpContext.User.IsInRole("Bans"))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                                      { "controller", "BlockUsers" }, { "action", "AccessDenied" } });
            }
        }
    }
}