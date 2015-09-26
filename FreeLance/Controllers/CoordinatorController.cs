using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FreeLance.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        // GET: Coordinator
        public ActionResult Index()
        {
            return RedirectToAction("Home");
        }

        public ActionResult Home()
        {
            return View();
        }

    }
}