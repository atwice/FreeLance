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
			public List<ProblemInProgressViewModel> ProblemsInProgress { get; set; }
			public List<ProblemOpenViewModel> ProblemsOpen { get; set; }
		}

		public class ProblemOpenViewModel
		{
			public String Name { get; set; }
			public String ShortDescription { get; set; }
			public decimal Cost { get; set; }
			public int SubscribersCount { get; set; }
			public int Id { get; set; }
			public int NewMsgCount { get; set; }
		}

		public class ProblemInProgressViewModel
		{
			public String Name { get; set; }
			public int Id { get; set; }
			public List<ContractInProgressViewModel> Contracts { get; set; }
		}

		public class ContractInProgressViewModel
		{
			public String FIO { get; set; }
			public String freelancerId { get; set; }
			public int id { get; set; }
			public ContractStatus status { get; set; }
			public int newMsgCount { get; set; }
			public decimal Cost { get; set; }
			public String EndingDate { get; set; }
			public String CreationDate { get; set;  }
			public String StatusIcon { get; set; }
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


		public String getStatusIcon(ContractStatus status)
		{
			String answer = "";
			switch(status)
			{
				case ContractStatus.Opened:
					answer = "flaticon-question41";
					break;

				case ContractStatus.InProgress:
					answer = "flaticon-two185";
					break;

				case ContractStatus.Done:
					answer = "flaticon-justice4";
					break;

				case ContractStatus.ClosedNotPaid:
					answer = "flaticon-payment7";
					break;
			}
			return answer;
		}

		public List<ContractInProgressViewModel> getProblemContract(int problemId)
		{
			List<ContractInProgressViewModel> contractsData = new List<ContractInProgressViewModel>();
			ICollection<ContractModels> contrats = db.ProblemModels
				.Where(
					p => p.ProblemId == problemId
				)
				.Select(
					p => p.Contracts
				)
				.ToList()[0];
			foreach(var contract in contrats)
			{
				if(contract.Status == ContractStatus.ClosedNotPaid ||
					contract.Status == ContractStatus.Done ||
					contract.Status == ContractStatus.InProgress ||
					contract.Status == ContractStatus.Opened)
				{
					contractsData.Add(
					new ContractInProgressViewModel
						{
							FIO = contract.Freelancer.FIO,
							freelancerId = contract.Freelancer.Id,
							id = contract.ContractId,
							status = contract.Status,
							newMsgCount = 0, // TODO
							EndingDate = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"), // TODO
							Cost = contract.Cost,
							CreationDate = contract.CreationDate.ToString("dd/MM/yyyy"),
							StatusIcon = getStatusIcon(contract.Status)
						}
					);
				}
			}
			return contractsData;
		}

		// GET: Employer
		public ActionResult Home()
		{
			string userId = User.Identity.GetUserId();
			var model = new HomeViewModel();

			model.ProblemsInProgress = db.ProblemModels
				.Where(
					p => p.Employer.Id == userId && 
						(p.Status == ProblemStatus.Opened || p.Status == ProblemStatus.InProgress)
						&& p.Contracts.Count != 0
				)
				.Select(
					p => new ProblemInProgressViewModel
					{
						Name = p.Name,
						Id = p.ProblemId
					}
				)
				.ToList();

			foreach(var problem in model.ProblemsInProgress)
			{
				problem.Contracts = getProblemContract(problem.Id);
			}

			model.ProblemsOpen = db.ProblemModels
				.Where(
					p => p.Employer.Id == userId
						&& p.Status == ProblemStatus.Opened
				)
				.Select(
					p => new ProblemOpenViewModel
					{
						Name = p.Name,
						Id = p.ProblemId,
						ShortDescription = p.SmallDescription,
						Cost = p.Cost,
						SubscribersCount = p.Subscriptions.Count,
						NewMsgCount = 0
					}
				)
				.ToList();

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

		public ActionResult Settings()
		{
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
			return View(user.EmailNotificationPolicy);
		}

		[HttpPost]
		public ActionResult Settings(ApplicationUser.EmailNotificationPolicyModel policy)
		{
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
			user.EmailNotificationPolicy = policy;
			db.SaveChanges();
			return View(user.EmailNotificationPolicy);
		}
	}
}