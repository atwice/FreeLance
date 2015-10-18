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
	[Authorize(Roles = "Admin, Employer, Coordinator")]
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
			public string Id { get; set;  }
			public int ClosedContractsCount { get; set; }
			public int OpenContractsCount { get; set; }
			public decimal Rate { get; set; }
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
			public String FreelancerName { get; set; }
			public String Details { get; set; }
		}

		public class SmallContractInfoModel
		{
			public ContractStatus Status { get; set; }
			public decimal Rate { get; set; }
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
                                FreelancerName = c.Freelancer.FIO,
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

		public FreelancerViewModel GetFreelancerInfo(string id)
		{
			ApplicationUser freelancer = db.Users.Find(id);
			List<SmallContractInfoModel> contracts = db.ContractModels
				.Where(
					c => c.Freelancer.Id == id)
				.Select(
				c => new SmallContractInfoModel
				{
					Rate = c.Rate,
					Status = c.Status
				})
				.ToList();
			var model = new FreelancerViewModel
			{
				Rate = 0,
				Name = freelancer.FIO,
				ClosedContractsCount = 0,
				OpenContractsCount = 0,
				Id = id
			};
			decimal rate = 0;
			foreach (var contract in contracts)
			{
				if (contract.Status == ContractStatus.Closed)
				{
					rate += contract.Rate;
					model.ClosedContractsCount += 1;
				}
				else if (contract.Status == ContractStatus.InProgress ||
				  contract.Status == ContractStatus.Opened)
				{
					model.OpenContractsCount += 1;
				}
			}
			if(model.ClosedContractsCount != 0)
			{
				model.Rate = rate / model.ClosedContractsCount;
			}
			return model;
		}

		public ActionResult Freelancers()
		{
			List<string> Ids = AccountController.GetApplicationUsersInRole(db, "Freelancer").Select(
				u => u.Id ).ToList();
			List<FreelancerViewModel> model = new List<FreelancerViewModel>();
			foreach(var id in Ids)
			{
				model.Add(GetFreelancerInfo(id));
			}
			return View(model);
		}

		

		public ActionResult NewProblem()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult NewProblem([Bind(Include = "ProblemId,Name,Description,Status,Cost")] ProblemModels problem)
		{
			if (ModelState.IsValid)
			{
				problem.CreationDate = DateTime.Now;
				problem.Employer = db.Users.Find(User.Identity.GetUserId());
				db.ProblemModels.Add(problem);
				db.SaveChanges();
				return RedirectToAction("Problem", new { id = problem.ProblemId });
			}

			return View(problem);
		}

	}
}