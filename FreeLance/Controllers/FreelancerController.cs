using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace FreeLance.Controllers
{
	[Authorize(Roles = "Admin, Freelancer")]
	public class FreelancerController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		public class HomeView
		{
			public List<ContractModels> Contracts { get; set; }
			public List<ProblemModels> Problems { get; set; }
		}

		public ActionResult Index()
		{
			return RedirectToAction("Home");
		}

		// GET: Freelancer
		public ActionResult Home()
		{
			string userId = User.Identity.GetUserId();
			var viewModel = new HomeView
			{
				Contracts = db.ContractModels.Where(
					contract => contract.Status != ContractStatus.Done
						&& contract.Freelancer != null
						&& contract.Freelancer.Id == userId
					).ToList(),
				Problems = db.SubscriptionModels.Where(subscription => subscription.Freelancer.Id == userId)
												.Select(subscription => subscription.Problem).ToList()
			};
			return View(viewModel);
		}

		public ActionResult Archive()
		{
			return View(db.ContractModels.ToList());
		}

		public ActionResult Contract(int id)
		{
			return View(db.ContractModels.Find(id));
		}

		public class ProblemView
		{
			public ProblemModels ProblemModels { get; set; }
			public bool IsSubscibed { get; set; }
		}

		public ActionResult Problem(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			ProblemModels problemModels = db.ProblemModels.Find(id);
			if (problemModels == null)
			{
				return HttpNotFound();
			}
			string userId = User.Identity.GetUserId();
			var contracts = db.ContractModels.Where(t => t.Freelancer.Id.Equals(userId)).ToList();
			var subscriptions = db.SubscriptionModels.Where(t => t.Freelancer.Id.Equals(userId)).ToList();
			ViewBag.contractsSize = contracts.LongCount();
			ViewBag.subscriptionsSize = subscriptions.LongCount();
			return View();
		}

		public ViewResult OpenProblems()
		{
			ProblemModels[] openProblems = db.ProblemModels.Where(x => x.Status == 0).ToArray();
			return View(openProblems);
		}

		public ActionResult Profile()
		{
			string userId = User.Identity.GetUserId();
			var contracts = db.ContractModels.Where(t => t.Freelancer.Id.Equals(userId)).ToList();
			var subscriptions = db.SubscriptionModels.Where(t => t.Freelancer.Id.Equals(userId)).ToList();
			ViewBag.contractsSize = contracts.LongCount();
			ViewBag.subscriptionsSize = subscriptions.LongCount();
			return View();
		}
	}
}