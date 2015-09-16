using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FreeLance.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			if (User.Identity.IsAuthenticated)
			{
				string userController = getUserControllerByRole();
				return RedirectToAction("Home", userController);
			}
			return View();
		}

		private string getUserControllerByRole()
		{
			if( User.IsInRole( "Employer" ) || User.IsInRole( "Admin" ) )
			{
				return "Employer";
			} else if( User.IsInRole( "Freelancer" ) )
			{
				return "Freelancer";
			} else
			{
				throw new InvalidOperationException("Unknown role for user: " + User.Identity.Name);
			}

		}
	}
}