using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FreeLance.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {

		private ApplicationDbContext db = new ApplicationDbContext();

		// GET: Coordinator
		public ActionResult Index()
        {
            return RedirectToAction("Home");
        }

        public ActionResult Home()
        {
            return View(AccountController.GetApplicationUsersInRole(db, "employer")
						.Where(x => !x.IsApprovedByCoordinator).Count());
        }

		public class EmployerApprovationVR {
			public ApplicationUser Employer { get; set; }
			public class ApprovationForm {
				public string ButtonText { get; set; }
				public string IsApproved { get; set; }
				public string Redirect { get; set; }
				public string EmployerId { get; set; }
			}
			public ApprovationForm Form { get; set; }
        }

		public ActionResult EmployerApprovationList() {
			return View(
				Enumerable.Select(AccountController.GetApplicationUsersInRole(db, "employer"),
					employer => new EmployerApprovationVR {
						Employer = employer,
						Form = new EmployerApprovationVR.ApprovationForm {
							ButtonText = !employer.IsApprovedByCoordinator ? "Подтвердить" : "Отменить подтверждение",
							IsApproved = (!employer.IsApprovedByCoordinator).ToString(),
							Redirect = "/Coordinator/EmployerApprovationList",
							EmployerId = employer.Id
						}
					}
				).OrderBy(data => data.Employer.IsApprovedByCoordinator)
			);
		}

		[HttpPost]
		public ActionResult ChangeEmployerApprovalStatus(string employerId, bool isApproved, string redirect) {
			ApplicationUser employer = db.Users.Find(employerId);
			var employerRole = db.Roles.Where(role => role.Name == "employer").ToArray()[0];
			
			if (employer == null || employer.Roles.Where(role => role.RoleId == employerRole.Id).Count() == 0) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			employer.IsApprovedByCoordinator = isApproved;
			db.SaveChanges();
			return Redirect(redirect == null ? "/Coordinator/Home" : redirect);
		}

	}
}