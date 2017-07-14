using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC.Controllers
{
    public class BlockUsersController : Controller
    {
        // GET: BlockUsers
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}