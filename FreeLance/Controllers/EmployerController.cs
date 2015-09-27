using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using FreeLance.Models;
using Microsoft.AspNet.Identity;

namespace FreeLance.Controllers
{
	[Authorize(Roles = "Admin, Employer")]
	public class EmployerController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		public class HomeViewModel
		{
			public List<ContractModels> ActualContracts { get; set; }
			public List<ProblemModels> OpenProblems { get; set; }
		}

		public class ProblemView
		{
			public List<SubscriptionModels> Subscriptions { get; set; }
			public ProblemModels Problem { get; set; }
		}

		public class FreelancerViewModel
		{
			public String Name { get; set; }
		}

        public class ArchivedProblemViewModel
        {
            public int ProblemId { get; set; }
            public String Name { get; set; }
            public List<ArchivedContractViewModel> Contracts { get; set; }
        }

        public class ArchivedContractViewModel
		{
			public int ContractId { get; set; }
			//public String Name { get; set; }
			public String FreelancerName { get; set; }
			public String Details { get; set; }
		}

        public class ArchiveViewModel
        {
            public List<ArchivedProblemViewModel> Problems { get; set; } 
        }

        public ActionResult Index()
		{
			return RedirectToAction("Home");
		}

		// GET: Employer
		public ActionResult Home()
		{
			string userId = User.Identity.GetUserId();
			var model = new HomeViewModel();
			model.ActualContracts = db.ContractModels.Where(
				c => c.Problem.Employer.Id == userId
					&& (
                    c.Status == ContractStatus.Opened
                    || c.Status == ContractStatus.InProgress
                    || c.Status == ContractStatus.Done
                    )).ToList();
			model.OpenProblems = db.ProblemModels.Where(
				p => p.Employer.Id == userId
					&& p.Status == ProblemStatus.Opened
				).ToList();
			return View( model );
		}

		public ActionResult Archive()
		{
			string userId = User.Identity.GetUserId();
            var model = db.ProblemModels
                .Where(
					p => p.Employer.Id == userId
						&& p.Status == ProblemStatus.Closed)
				.Select(
					p => new ArchivedProblemViewModel
					{
						ProblemId = p.ProblemId,
						Name = p.Name,
                        Contracts = p.Contracts
                        .Where(
                            c => c.Status != ContractStatus.Opened)
                        .Select(
                            c => new ArchivedContractViewModel
                            {
                                ContractId = c.ContractId,
                                FreelancerName = c.Freelancer.UserName,
                                Details = c.Details
                            })
                        .ToList()
					})
				.ToList();
            return View(model);
		}

		// try /Employer/Problem/5 
		public ActionResult Problem(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ProblemModels problemModels = db.ProblemModels.Find(id);

			// check if employer is this problem's owner?
			if (problemModels == null)
			{
				return HttpNotFound();
			}

			var viewModel = new ProblemView
			{
				Subscriptions = db.SubscriptionModels.Where(x => x.Problem.ProblemId == id).ToList(),
				Problem = problemModels
			};
			return View(viewModel);
		}

		public ActionResult Freelancers()
		{
			var model = AccountController.GetApplicationUsersInRole(db, "Freelancer").Select(
				u => new FreelancerViewModel { Name = u.UserName }).ToList();
			return View(model);
		}

		

		public ActionResult NewProblem()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult NewProblem([Bind(Include = "ProblemId,Name,Description,Status")] ProblemModels problem)
		{
			if (ModelState.IsValid)
			{
				problem.Employer = db.Users.Find(User.Identity.GetUserId());
				db.ProblemModels.Add(problem);
				db.SaveChanges();
				return RedirectToAction("Problem", new { id = problem.ProblemId });
			}

			return View(problem);
		}

	}
}