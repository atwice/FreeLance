using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using FreeLance.Models;
using Microsoft.AspNet.Identity;
using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.DynamicData;
using System.Web.Mvc;
using Antlr.Runtime.Misc;
using FreeLance.Code;
using Microsoft.AspNet.Identity;
using Novacode;

namespace FreeLance.Controllers
{
	[Authorize(Roles = "Admin, Employer, Coordinator, Freelancer")]
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
			public DateTime EndingDate { get; set; }
			public DateTime CreationDate { get; set; }
			public int SubscribersCount { get; set; }
			public int Id { get; set; }
			public int NewMsgCount { get; set; }
			public String StatusIcon { get; set; }
			public int AmountOfWorkers { get; set; }
        }

		public class ProblemInProgressViewModel
		{
			public String Name { get; set; }
			public int Id { get; set; }
			public DateTime EndingDate { get; set; }
			public DateTime CreationDate { get; set; }
            public decimal Cost { get; set; }
			public String StatusIcon { get; set; }
			public int NewMsgCount { get; set; }
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
			public DateTime EndingDate { get; set; }
			public String CreationDate { get; set; }
            public DateTime DeadlineDate { get; set; }
            public String StatusIcon { get; set; }
		}

		public class ProblemView
		{
			public List<SubscriptionModels> Subscriptions { get; set; }
			public ProblemModels Problem { get; set; }
		}

		public class ProfileView
		{
			public String EmployerEmail { get; set; }
            public String EmployerPhoto { get; set; }
			public int OpenProblemsCount { get; set; }
			public int OpenContractsCount { get; set; }
			public int ClosedProblemsCount { get; set; }
			public int ClosedContractsCount { get; set; }
			public ApplicationUser.EmailNotificationPolicyModel emailNotifications { get; set; }
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
					int chatId = ChatController.FindContractChatId(contract.ContractId);
                    int newMsgCount = ChatController.CalcUserInfo(User.Identity.GetUserId(), chatId).UnreadMessagesCount;
                    contractsData.Add(
					new ContractInProgressViewModel
						{
							FIO = contract.Freelancer.FIO,
							freelancerId = contract.Freelancer.Id,
							id = contract.ContractId,
							status = contract.Status,
							newMsgCount = newMsgCount,
							EndingDate = contract.EndingDate,
                            DeadlineDate = contract.DeadlineDate,
							Cost = contract.Cost,
							CreationDate = contract.CreationDate.ToString("dd/MM/yyyy"),
							StatusIcon = getStatusIcon(contract.Status)
						}
					);
				}
			}
			return contractsData;
		}

		private List<ProblemInProgressViewModel> getProblemsInProgress( String userId )
		{
			List<ProblemInProgressViewModel> problemsInProgress = db.ProblemModels
				.Where(
					p => p.Employer.Id == userId &&
						(p.Status == ProblemStatus.Opened || p.Status == ProblemStatus.InProgress)
						&& p.Contracts.Count != 0
				)
				.Select(
					p => new ProblemInProgressViewModel
					{
						Name = p.Name,
						EndingDate = p.DeadlineDate,
						CreationDate = p.CreationDate,
						NewMsgCount = 0,
                        Cost = p.Cost,
						Id = p.ProblemId
					}
				)
				.ToList();
			foreach (ProblemInProgressViewModel problem in problemsInProgress) {
				problem.NewMsgCount = ChatController.CalcUserInfo(User.Identity.GetUserId(),
							ChatController.FindProblemChatId(problem.Id)).UnreadMessagesCount;
            }
			return problemsInProgress;
        }

		private List<ProblemOpenViewModel> getOpenProblems( String userId )
		{
			List<ProblemOpenViewModel> openProblems = db.ProblemModels
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
						CreationDate = p.CreationDate,
						EndingDate = p.DeadlineDate,
						NewMsgCount = 0 
					}
				)
				.ToList();
			foreach (ProblemOpenViewModel problem in openProblems) {
				problem.NewMsgCount = ChatController.CalcUserInfo(User.Identity.GetUserId(),
							ChatController.FindProblemChatId(problem.Id)).UnreadMessagesCount;
			}
			return openProblems;
        }

		private List<ProblemOpenViewModel> getOpenProblemsForFreelancer(String userId)
		{
			List<ProblemOpenViewModel> openProblems = db.ProblemModels
				.Where(
					p => p.Employer.Id == userId
						&& (p.Status == ProblemStatus.Opened)
				)
				.Select(
					p => new ProblemOpenViewModel
					{
						Name = p.Name,
						Id = p.ProblemId,
						ShortDescription = p.SmallDescription,
						Cost = p.Cost,
						SubscribersCount = p.Subscriptions.Count,
						CreationDate = p.CreationDate,
						EndingDate = p.DeadlineDate,
						AmountOfWorkers = p.AmountOfWorkes
					}
				)
				.ToList();
			return openProblems;
		}

		// GET: Employer
		[Authorize(Roles = "Employer")]
		public ActionResult Home()
		{
			string userId = User.Identity.GetUserId();
			var model = new HomeViewModel();

			model.ProblemsInProgress = getProblemsInProgress( userId );

			foreach(var problem in model.ProblemsInProgress)
			{
				problem.Contracts = getProblemContract(problem.Id);
			}

			model.ProblemsOpen = getOpenProblems( userId );

			return View( model );
		}

		[Authorize(Roles = "Coordinator, Freelancer, Employer")]
		public ActionResult Details(string id, String sortOrder, String lastSort)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ApplicationUser employer = db.Users.Find(id);
			if (employer == null)
			{
				return HttpNotFound();
			}

			if (User.IsInRole("Freelancer") || User.IsInRole("Employer"))
			{
				return PartialView("_DetailsForFreelancerAndEmployer", getDetailsForFreelancer(employer, sortOrder, lastSort));
			}

			return View(getDetailsForCoordinator(employer, sortOrder, lastSort));
		}

		public class DetailsForCoordinatorView
		{
			public String Name { get; set; }
			public String Email { get; set; }
			public String Phone { get; set; }
			public String PhotoPath { get; set; }
			public String Id { get; set; }
			public bool isApproved { get; set; }

			public List<ProblemInProgressViewModel> ProblemsInProgress { get; set; }
			public List<ProblemOpenViewModel> ProblemsOpen { get; set; }
			public List<ArchivedContractViewModel> ArchievedContracts { get; set; }
		}

		public class DetailsForFreelancerView
		{
			public String Name { get; set; }
			public String Email { get; set; }
			public String Phone { get; set; }
			public String PhotoPath { get; set; }
			public String Id { get; set; }
			
			public List<ProblemOpenViewModel> ProblemsOpen { get; set; }
		}

		public DetailsForCoordinatorView getDetailsForCoordinator(ApplicationUser employer, String sortOrder, String lastSort)
		{
			string id = employer.Id;
			List<ContractModels> contracts = db.ContractModels
				.Where(c => c.Freelancer.Id == employer.Id).ToList();

			DetailsForCoordinatorView model = new DetailsForCoordinatorView
			{
				Email = employer.Email,
				Phone = "+7(916)0001122", // TODO
				Name = employer.FIO,
				isApproved = employer.IsApprovedByCoordinator == true ? true : false,
				PhotoPath = employer.PhotoPath, 
				Id = id
                
			};

			model.ProblemsInProgress = getProblemsInProgress( id );
			model.ArchievedContracts = getArchievedContracts( id );
			foreach (var problem in model.ProblemsInProgress)
			{
				problem.Contracts = getProblemContract(problem.Id);
			}

			model.ProblemsOpen = getOpenProblems( id );

			return model;
		}

		public DetailsForFreelancerView getDetailsForFreelancer(ApplicationUser employer, String sortOrder, String lastSort)
		{
			List<ContractModels> contracts = db.ContractModels
				.Where(c => c.Freelancer.Id == employer.Id).ToList();

			DetailsForFreelancerView model = new DetailsForFreelancerView
			{
				Email = employer.Email,
				Phone = "+7(916)0001122", // TODO
				Name = employer.FIO,
				PhotoPath = employer.PhotoPath, 
				Id = employer.Id
			};

			model.ProblemsOpen = getOpenProblemsForFreelancer(employer.Id);

			return model;
		}

		public static String getStatusMessage(ContractStatus status)
		{
			String result = "";
			switch(status)
			{
				case ContractStatus.Closed:
					result = "Выполнена полностью";
					break;
				case ContractStatus.ClosedNotPaid:
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
				DeadlineDate = c.DeadlineDate,
				StatusMessage = getStatusMessage(c.Status)
			};

			return contract;
		}

		private List<ArchivedContractViewModel> getArchievedContracts(String userId, bool hideFailed = false)
		{
			ICollection<ContractModels> contracts = db.ContractModels
				.Where(
					c => c.Problem.Employer.Id == userId
						&& (c.Status == ContractStatus.Closed
							|| c.Status == ContractStatus.Failed
							|| c.Status == ContractStatus.СancelledByEmployer
							|| c.Status == ContractStatus.СancelledByFreelancer))
				.ToList();

			List<ArchivedContractViewModel> model = new List<ArchivedContractViewModel>();
			foreach (var contract in contracts)
			{
				if (!hideFailed || (hideFailed && contract.Status == ContractStatus.Closed))
				{
					model.Add(getClosedContractData(contract));
				}
			}
			return model;
		}

		[Authorize(Roles = "Employer")]
		public ActionResult Archive(String sortOrder, bool hideFailed=false)
		{
			ViewBag.hideFailed = hideFailed;

			string userId = User.Identity.GetUserId();
			List<ArchivedContractViewModel> model = getArchievedContracts(userId, hideFailed);

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

		[Authorize(Roles = "Employer")]
		public ActionResult Freelancers(String searchString, String sortOrder)
		{
			List<string> Ids = AccountController.GetApplicationUsersInRole(db, "Freelancer").Select(
				u => u.Id ).ToList();
			List<FreelancerViewModel> model = new List<FreelancerViewModel>();
			foreach(var id in Ids)
			{
				model.Add(new FreelancerViewModel(id));
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

			return PartialView("~/Views/_FreelancersView.cshtml", model);
		}

		[Authorize(Roles = "Employer")]
		public ActionResult Profile()
		{
			String id = User.Identity.GetUserId();
            ApplicationUser employer = db.Users.Find(id);

			ProfileView model = new ProfileView
			{
				EmployerEmail = employer.Email,
                EmployerPhoto = employer.PhotoPath,
				OpenContractsCount = db.ContractModels.Where(
					c => c.Problem.Employer.Id == id && (c.Status == ContractStatus.Done 
					|| c.Status == ContractStatus.InProgress || c.Status == ContractStatus.ClosedNotPaid
					|| c.Status == ContractStatus.Opened)).Count(),
				ClosedContractsCount = db.ContractModels.Where(
					c => c.Problem.Employer.Id == id && (c.Status == ContractStatus.Closed
					|| c.Status == ContractStatus.Failed || c.Status == ContractStatus.СancelledByEmployer
					|| c.Status == ContractStatus.СancelledByFreelancer)).Count(),
				OpenProblemsCount = db.ProblemModels.Where(
					p => p.Employer.Id == id 
					&& (p.Status == ProblemStatus.InProgress || p.Status == ProblemStatus.InProgress)).Count(),
				ClosedProblemsCount = db.ProblemModels.Where(
					p => p.Employer.Id == id
					&& p.Status == ProblemStatus.Closed).Count(),
				emailNotifications = employer.EmailNotificationPolicy
			};
			return View(model);
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

        private string saveDocumentOnDisc(HttpPostedFileBase file, string dir, string location = "/App_Data/")
        {
            var ext = Path.GetExtension(file.FileName);
            var fileName = User.Identity.GetUserId() + "_" + DateTime.Now.Ticks.ToString() + ext;
            var path = Path.Combine(Server.MapPath("~" + location + dir + "/"), fileName);
            file.SaveAs(path);
            return location + dir + "/" + fileName;
        }


        [HttpPost]
        public ActionResult UploadPhoto()
        {
            ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    user.PhotoPath = saveDocumentOnDisc(file, "photo", "/Files/");
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Profile");
        }
    }
}