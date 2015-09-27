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
	[Authorize(Roles = "Admin, Freelancer, Incognito, Coordinator")]
	public class FreelancerController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		public class HomeView
		{
			public List<ContractModels> Contracts { get; set; }
			public List<ProblemModels> Problems { get; set; }
		}

        public class ArchivedContractViewModel
        {
            public int ContractId { get; set; }
            public String Name { get; set; }
            public String EmployerName { get; set; }
            public String Details { get; set; }
        }

        public class ArchiveViewModel
        {
            public List<ArchivedContractViewModel> SuccessfulContracts { get; set; } // with Closed status
            public List<ArchivedContractViewModel> FailedContracts { get; set; } // with CancelledByEmpoyer, CancelledByFreelancer & Failed status
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
					contract => (
                        contract.Status == ContractStatus.Opened
                        || contract.Status == ContractStatus.InProgress)
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
            string userId = User.Identity.GetUserId();
            var model = new ArchiveViewModel();
            model.SuccessfulContracts = db.ContractModels
                .Where(
                    c => c.Freelancer.Id == userId
                        && c.Status == ContractStatus.Closed)
                .Select(
                    c => new ArchivedContractViewModel
                    {
                        ContractId = c.ContractId,
                        EmployerName = c.Problem.Employer.UserName,
                        Name = c.Problem.Name,
                        Details = c.Details
                    })
                .ToList();
            model.FailedContracts = db.ContractModels
                .Where(
                    c => c.Freelancer.Id == userId
                        && (
                        c.Status == ContractStatus.Failed
                        || c.Status == ContractStatus.СancelledByEmployer
                        || c.Status == ContractStatus.СancelledByFreelancer))
                .Select(
                    c => new ArchivedContractViewModel
                    {
                        ContractId = c.ContractId,
                        EmployerName = c.Problem.Employer.UserName,
                        Name = c.Problem.Name,
                        Details = c.Details
                    })
                .ToList();
            return View(model);
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