using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Net;

namespace FreeLance.Controllers
{
	[Authorize(Roles = "Coordinatior, Employer, Freelancer")]
    public class DocumentPackageController : Controller
    {
		private ApplicationDbContext db = new ApplicationDbContext();

		public class DetailsVR {
			public DocumentPackageModels DocumentPackage { get; set; }
		}

		public ActionResult Details(int? id) {
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
			if (id == null) {
				if (user.DocumentPackageId == null)
					return View(new DetailsVR { DocumentPackage = null });
				return RedirectToAction("Details", "DocumentPackage", user.DocumentPackageId);
			}
			DocumentPackageModels documentPackage = db.DocumentPackageModels.Find(id);
			if (documentPackage == null) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			if (!User.IsInRole("Coordinator")) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			return View(new DetailsVR { DocumentPackage = documentPackage });
		}
    }
}