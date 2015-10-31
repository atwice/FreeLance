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
			public string Email { get; set; }
			public int ClosedContractsCount { get; set; }
			public int OpenContractsCount { get; set; }
			public decimal Rate { get; set; }
		}


        public class ArchivedContractViewModel
		{
			public String ProblemName { get; set; }
			public int ContractId { get; set; }
			public String FreelancerName { get; set; }
			public String Details { get; set; }
			public String FreelancerId { get; set; }
			public decimal Cost { get; set; }
			public DateTime EndingDate { get; set; }
			public DateTime CreationDate { get; set; }
			public DateTime DeadlineDate { get; set; }
			public ContractStatus Status { get; set; }
			public String StatusMessage { get; set; }
			public decimal Rate { get; set; }
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


		public static String getStatusIcon(ContractStatus status)
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


		public String getStatusMessage(ContractStatus status)
		{
			String result = "";
			switch(status)
			{
				case ContractStatus.Closed:
					result = "Выполнена полностью";
					break;
				case ContractStatus.Failed:
					result = "Не выполнена: работодатель не принял работу";
					break;
				case ContractStatus.СancelledByEmployer:
					result = "Не выполнена: работодатель отменил заказ";
					break;
				case ContractStatus.СancelledByFreelancer:
					result = "Не выполнена: исполнитель отказался от задачи";
					break;
			}
			return result;
		}

		public ArchivedContractViewModel getClosedContractData(ContractModels c)
		{
			var contract = new ArchivedContractViewModel
			{

				ProblemName = c.Problem.Name,
				ContractId = c.ContractId,
				FreelancerName = c.Freelancer.FIO,
				Details = c.Details,
				FreelancerId = c.Freelancer.Id,
				Cost = c.Cost,
				Status = c.Status,
				Rate = c.Rate,
				CreationDate = c.CreationDate,
				EndingDate = c.EndingDate,
				DeadlineDate = DateTime.Now.AddDays(30), // TODO
				StatusMessage = getStatusMessage(c.Status)
			};

			return contract;
		}

		
		public ActionResult Archive(String sortOrder, bool hideFailed=false)
		{
			ViewBag.hideFailed = hideFailed;

			string userId = User.Identity.GetUserId();
			ICollection <ContractModels> contracts = db.ContractModels
                .Where(
					c => c.Problem.Employer.Id == userId
						&& (c.Status == ContractStatus.Closed
							|| c.Status == ContractStatus.Failed
							|| c.Status == ContractStatus.СancelledByEmployer
							|| c.Status == ContractStatus.СancelledByFreelancer))
				.ToList();

			List<ArchivedContractViewModel> model = new List<ArchivedContractViewModel>();
			foreach (var contract in contracts) { 
				if(!hideFailed || (hideFailed && contract.Status == ContractStatus.Closed))
				{
					model.Add(getClosedContractData(contract));
				}
			}

			ViewBag.sortCost = "cost";
			ViewBag.sortName = "name";
			ViewBag.sortEndingDate = "ending_date";
			ViewBag.sortCreationDate = "creation_date";

			switch (sortOrder)
			{
				case "cost_desc":
					model = model.OrderByDescending(с => с.Cost).ToList();
					break;
				case "cost":
					ViewBag.sortCost = "cost_desc";
					model = model.OrderBy(с => с.Cost).ToList();
					break;
				case "name_desc":
					model = model.OrderByDescending(с => с.ProblemName).ToList();
					break;
				case "name":
					ViewBag.sortName = "name_desc";
					model = model.OrderBy(с => с.ProblemName).ToList();
					break;
				case "ending_date_desc":
					model = model.OrderByDescending(с => с.EndingDate).ToList();
					break;
				case "ending_date":
					ViewBag.sortEndingDate = "ending_date_desc";
					model = model.OrderBy(с => с.EndingDate).ToList();
					break;
				case "creation_date_desc":
					model = model.OrderByDescending(с => с.CreationDate).ToList();
					break;
				case "creation_date":
					ViewBag.sortCreationDate = "creation_date_desc";
					model = model.OrderBy(с => с.CreationDate).ToList();
					break;

				default:
					model = model.OrderBy(c => c.ProblemName).ToList();
					break;
			}

			ViewBag.sortOrder = sortOrder;

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
				Email = freelancer.Email,
				ClosedContractsCount = 0,
				OpenContractsCount = 0,
				Id = id
			};
			decimal rate = 0;
			foreach (var contract in contracts)
			{
				if (contract.Status == ContractStatus.Closed 
					|| contract.Status == ContractStatus.Failed
					|| contract.Status == ContractStatus.СancelledByEmployer
					|| contract.Status == ContractStatus.СancelledByFreelancer)
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

		public ActionResult Freelancers(String searchString, String sortOrder)
		{
			List<string> Ids = AccountController.GetApplicationUsersInRole(db, "Freelancer").Select(
				u => u.Id ).ToList();
			List<FreelancerViewModel> model = new List<FreelancerViewModel>();
			foreach(var id in Ids)
			{
				model.Add(GetFreelancerInfo(id));
			}

			if (!String.IsNullOrEmpty(searchString))
			{
				model = model.Where(c => c.Name.Contains(searchString) || c.Email.Contains(searchString) ).ToList();
			}

			ViewBag.sortEmail = "email";
			ViewBag.sortName = "name";
			ViewBag.sortRate = "rate";
			ViewBag.sortDone = "done";
			ViewBag.sortInProgress = "in_progress";

			switch (sortOrder)
			{
				case "email_desc":
					model = model.OrderByDescending(с => с.Email).ToList();
					break;
				case "email":
					ViewBag.sortEmail = "sort_desc";
					model = model.OrderBy(с => с.Email).ToList();
					break;
				case "name_desc":
					model = model.OrderByDescending(с => с.Name).ToList();
					break;
				case "name":
					ViewBag.sortName = "name_desc";
					model = model.OrderBy(с => с.Name).ToList();
					break;
				case "rate_desc":
					model = model.OrderByDescending(с => с.Rate).ToList();
					break;
				case "rate":
					ViewBag.sortRate = "rate_desc";
					model = model.OrderBy(с => с.Rate).ToList();
					break;
				case "done_desc":
					model = model.OrderByDescending(с => с.ClosedContractsCount).ToList();
					break;
				case "done":
					ViewBag.sortDone = "done_desc";
					model = model.OrderBy(с => с.ClosedContractsCount).ToList();
					break;
				case "in_progress_desc":
					model = model.OrderByDescending(с => с.OpenContractsCount).ToList();
					break;
				case "in_progress":
					ViewBag.sortInProgress = "in_progress_desc";
					model = model.OrderBy(с => с.OpenContractsCount).ToList();
					break;

				default:
					model = model.OrderBy(c => c.Name).ToList();
					break;
			}

			ViewBag.sortOrder = sortOrder;

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