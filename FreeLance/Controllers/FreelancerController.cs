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
	[Authorize(Roles = "Admin, Freelancer, Incognito, Employer, Coordinator, WithoutDocuments")]
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
			public String ProblemName { get; set; }
			public int ContractId { get; set; }
			public String EmployerName { get; set; }
			public String Details { get; set; }
			public String EmployerId { get; set; }
			public decimal Cost { get; set; }
			public DateTime EndingDate { get; set; }
			public DateTime CreationDate { get; set; }
			public DateTime DeadlineDate { get; set; }
			public ContractStatus Status { get; set; }
			public String StatusMessage { get; set; }
			public decimal Rate { get; set; }
		}

		public class FreelancerContractViewModel
		{
			public int ContractId { get; set; }
			public String Name { get; set; }
			public String EmployerName { get; set; }
			public String EmployerId { get; set; }
			public String Comment { get; set; }
			public decimal Rate { get; set; }
			public DateTime DeadlineDate { get; set; }
			public DateTime EndingDate { get; set; }
			public DateTime StartDate { get; set; }
			public decimal Cost { get; set; }
		}

		public class FreelancerLawContractViewModel
		{
			public String LawFacePath { get; set; }
			public String LawFace { get; set; }
			public DateTime StartingDate { get; set; }
			public DateTime EndingDate { get; set; }
		}

		public class ArchiveViewModel
		{
			public List<ArchivedContractViewModel> SuccessfulContracts { get; set; } // with Closed status
			public List<ArchivedContractViewModel> FailedContracts { get; set; } // with CancelledByEmpoyer, CancelledByFreelancer & Failed status
		}

		public class ContractHomeViewModel
		{
			public string Name { get; set; }
			public string Author { get; set; }
			public DateTime Deadline { get; set; }
			public int CommentNumber { get; set; }
			public int ContractId { get; set; }
		}

		public class ProblemHomeViewModel
		{
			public int ProblemId { get; set; }
			public string Name { get; set; }
			public decimal Money { get; set; }
			public string ShortDescription { get; set; }
			public int CommentNumber { get; set; }
			public DateTime DeadlineDate { get; set; }
			public int Others { get; set; }
		}

		public class HomeViewModel
		{
			public List<ContractHomeViewModel> ContractsInWork;
			public List<ContractHomeViewModel> ContractsPending;
			public List<ContractHomeViewModel> ContractsWaitingMoney;
			public List<ProblemHomeViewModel> Problems;
		}

		public ActionResult Index()
		{
			return RedirectToAction("Home");
		}

		public class ContractInfoForEmployer
		{
			public String ProblemName { get; set; }
			public String Comment { get; set; }
			public String WorkMessage { get; set; }
			public int ProblemId { get; set; }
			public String DeadlineDate { get; set; }
			public String CreationDate { get; set; }
			public String EndingDate { get; set; }
			public decimal Cost { get; set; }
			public decimal Rate { get; set; }
			public String EmployerName { get; set; }
			public String EmployerId { get; set; }
			public int ContractId { get; set; }
			public String StatusIcon { get; set; }

		}

		public class SubscriptionInfoForEmployer
		{
			public String ProblemName { get; set; }
			public int ProblemId { get; set; }
			public String EmployerName { get; set; }
			public String EmployerId { get; set; }
			public decimal Cost { get; set; }
			public DateTime DeadlineDate { get; set; }
		}

		public ContractInfoForEmployer getContractInfoForEmployer(ContractModels contract)
		{
			ContractInfoForEmployer info = new ContractInfoForEmployer
			{
				ProblemName = contract.Problem.Name,
				Comment = contract.Comment,
				WorkMessage = EmployerController.getStatusMessage(contract.Status),
				ProblemId = contract.Problem.ProblemId,
				DeadlineDate = contract.DeadlineDate.ToShortDateString(), // TODO
				CreationDate = contract.CreationDate.ToShortDateString(),
				EndingDate = contract.EndingDate.ToShortDateString(),
				Cost = contract.Cost,
				Rate = contract.Rate,
				EmployerName = contract.Problem.Employer.FIO,
				EmployerId = contract.Problem.Employer.Id,
				ContractId = contract.ContractId,
				StatusIcon = EmployerController.getStatusIcon(contract.Status)
			};
			return info;
		}

		public SubscriptionInfoForEmployer getSubscriptionInfoForEmployer(SubscriptionModels subscription)
		{
			SubscriptionInfoForEmployer info = new SubscriptionInfoForEmployer
			{
				ProblemName = subscription.Problem.Name,
				ProblemId = subscription.Problem.ProblemId,
				DeadlineDate = subscription.Problem.DeadlineDate,
				Cost = subscription.Problem.Cost,
				EmployerName = subscription.Problem.Employer.FIO,
				EmployerId = subscription.Problem.Employer.Id
			};
			return info;
		}

		public class OpenProblemsInfo
		{
			public bool isApproved { get; set; }
			public String lastSort { get; set; }
			public string showSubscriptions { get; set; }
			public List<OpenProblemInfo> openProblems { get; set; }
		}

		public class OpenProblemInfo
		{
			public String ProblemName { get; set; }
			public String ProblemShortDescription { get; set; }
			public String EmployerName { get; set; }
			public String EmployerId { get; set; }
			public DateTime CreationDate { get; set; }
			public DateTime DeadlineDate { get; set; }
			public decimal Cost { get; set; }
			public int AmountOfWorkers { get; set; }
			public int ProblemId { get; set; }
			public bool IsSubscribed { get; set; }
		}

		public OpenProblemInfo getOpenProblemInfo(ProblemModels problem)
		{
			OpenProblemInfo info = new OpenProblemInfo
			{
				ProblemName = problem.Name,
				ProblemShortDescription = problem.SmallDescription,
				EmployerName = problem.Employer.FIO,
				EmployerId = problem.Employer.Id,
				CreationDate = problem.CreationDate,
				DeadlineDate = problem.DeadlineDate,
				Cost = problem.Cost,
				AmountOfWorkers = problem.AmountOfWorkes,
				ProblemId = problem.ProblemId,
				IsSubscribed = problem.Subscriptions.Any(s => s.Freelancer.Id == User.Identity.GetUserId())
			};
			return info;
		}

		public class DetailsForEmployerView
		{
			public String FreelancerName { get; set; }
			public String FreelancerEmail { get; set; }
			public String FreelancerPhone { get; set; }
			public decimal FreelancerRate { get; set; }
			public String PhotoPath { get; set; }
			public String FreelancerId { get; set; }
			public String info;
			public String lastSort;
			public List<ContractInfoForEmployer> ClosedContracts { get; set; }
			public List<ContractInfoForEmployer> InProgressContracts { get; set; }
			public List<SubscriptionInfoForEmployer> Subscriptions { get; set; }
		}

		public class DetailsForCoordinatorView
		{
			public GeneralInfo General { get; set; }
			public PassportInfo Passport { get; set; }
			public BankInfo Bank { get; set; }

			public decimal FreelancerRate { get; set; }
			public string PhotoPath { get; set; }
			public string FreelancerId { get; set; }
			public string info;
			public string lastSort;
			public bool isApproved { get; set; }

			public DetailsProblemsView ProblemsView;
			public DetailsProfileView ProfileView;
		}

		public class GPHInfo
		{
			public String Name { get; set; }
		}

		public class ProfileView
		{
			public string FreelancerEmail { get; set; }
			public string FreelancerPhoto { get; set; }
			public decimal Rate { get; set; }
			public decimal TotalMoney { get; set; }
			public List<GPHInfo> GpHList { get; set; }

			public PassportInfo Passport { get; set; }
			public BankInfo Bank { get; set; }
			public GeneralInfo GeneralInfo { get; set; }

			public int OpenContractsCount { get; set; }
			public int ClosedContractsCount { get; set; }
			public string PhotoFirstPage { get; set; }
			public string PhotoAdress { get; set; }
			public ApplicationUser.EmailNotificationPolicyModel emailNotifications { get; set; }
		}

		public static bool checkIfContractRated(ContractStatus status)
		{
			return status == ContractStatus.Closed
				|| status == ContractStatus.ClosedNotPaid
				|| status == ContractStatus.Failed
				|| status == ContractStatus.СancelledByEmployer;
		}

		public bool checkIfContractClosed(ContractStatus status)
		{
			return status == ContractStatus.Closed
				|| status == ContractStatus.ClosedNotPaid
				|| status == ContractStatus.Failed
				|| status == ContractStatus.СancelledByEmployer
				|| status == ContractStatus.СancelledByFreelancer;
		}

		public static decimal getFreelancerRate(List<ContractModels> contracts)
		{
			decimal rate = 0;
			int ratedContrats = 0;
			foreach (var contract in contracts)
			{
				if (checkIfContractRated(contract.Status))
				{
					ratedContrats += 1;
					rate += contract.Rate;
				}
			}
			if (ratedContrats != 0)
			{
				rate /= ratedContrats;
			}
			return rate;
		}

		private List<FreelancerProblemViewModel> sortProblemsForCoordinator(String sortOrder, List<FreelancerProblemViewModel> problems, String lastSort)
		{
			if (lastSort == sortOrder)
			{
				switch (sortOrder)
				{
					case "sortCost":
						sortOrder = "cost_desc";
						break;
					case "sortProblemName":
						sortOrder = "problemName_desc";
						break;
					case "sortEmployerName":
						sortOrder = "employerName_desc";
						break;
					case "sortEndDate":
						sortOrder = "endDate_desc";
						break;
				}
			}

			switch (sortOrder)
			{
				case "cost_desc":
					problems = problems.OrderByDescending(s => s.Cost).ToList();
					break;
				case "cost":
					problems = problems.OrderBy(s => s.Cost).ToList();
					break;

				case "problemName_desc":
					problems = problems.OrderByDescending(s => s.Name).ToList();
					break;
				case "problemName":
					problems = problems.OrderBy(s => s.Name).ToList();
					break;

				case "employerName_desc":
					problems = problems.OrderByDescending(s => s.EmployerName).ToList();
					break;
				case "employerName":
					problems = problems.OrderBy(s => s.EmployerName).ToList();
					break;

				case "endDate_desc":
					problems = problems.OrderByDescending(s => s.EndingDate).ToList();
					break;
				case "endDate":
					problems = problems.OrderBy(s => s.EndingDate).ToList();
					break;

				default:
					problems = problems.OrderBy(s => s.Name).ToList();
					break;
			}

			return problems;
		}

		public DetailsForEmployerView sortSubcriptionsForEmployer(String sortOrder, DetailsForEmployerView model, String lastSort)
		{
			if (lastSort == sortOrder)
			{
				switch (sortOrder)
				{
					case "cost":
						sortOrder = "cost_desc";
						break;
					case "problemName":
						sortOrder = "problemName_desc";
						break;
					case "employerName":
						sortOrder = "employerName_desc";
						break;
					case "deadlineDate":
						sortOrder = "deadlineDate_desc";
						break;
				}
			}

			switch (sortOrder)
			{
				case "cost_desc":
					model.Subscriptions = model.Subscriptions.OrderByDescending(s => s.Cost).ToList();
					break;
				case "cost":
					model.Subscriptions = model.Subscriptions.OrderBy(s => s.Cost).ToList();
					break;

				case "problemName_desc":
					model.Subscriptions = model.Subscriptions.OrderByDescending(s => s.ProblemName).ToList();
					break;
				case "problemName":
					model.Subscriptions = model.Subscriptions.OrderBy(s => s.ProblemName).ToList();
					break;

				case "employerName_desc":
					model.Subscriptions = model.Subscriptions.OrderByDescending(s => s.EmployerName).ToList();
					break;
				case "employerName":
					model.Subscriptions = model.Subscriptions.OrderBy(s => s.EmployerName).ToList();
					break;

				case "deadlineDate_desc":
					model.Subscriptions = model.Subscriptions.OrderByDescending(s => s.DeadlineDate).ToList();
					break;
				case "deadlineDate":
					ViewBag.sortCost = "deadlineDate_desc";
					model.Subscriptions = model.Subscriptions.OrderBy(s => s.DeadlineDate).ToList();
					break;

				default:
					model.Subscriptions = model.Subscriptions.OrderBy(s => s.ProblemName).ToList();
					break;
			}

			model.lastSort = sortOrder;

			return model;
		}

		public DetailsForEmployerView getDetailsForEmployer(ApplicationUser freelancer, String _info, String sortOrder, String lastSort)
		{
			List<ContractModels> contracts = db.ContractModels
				.Where(c => c.Freelancer.Id == freelancer.Id).ToList();

			List<SubscriptionModels> subscriptions = db.SubscriptionModels
				.Where(s => s.Freelancer.Id == freelancer.Id).ToList();

			if (_info == null)
			{
				_info = "profile";
			}

			DetailsForEmployerView model = new DetailsForEmployerView
			{
				info = _info,
				FreelancerEmail = freelancer.Email,
				FreelancerPhone = "+7(916)0001122", // TODO
				FreelancerName = freelancer.FIO,
				PhotoPath = Utils.GetPhotoUrl(freelancer.PhotoPath),
				FreelancerRate = getFreelancerRate(contracts),
				FreelancerId = freelancer.Id,
				ClosedContracts = new List<ContractInfoForEmployer>(),
				InProgressContracts = new List<ContractInfoForEmployer>(),
				Subscriptions = new List<SubscriptionInfoForEmployer>()
			};


			foreach (var contract in contracts)
			{
				if (checkIfContractClosed(contract.Status))
				{
					model.ClosedContracts.Add(getContractInfoForEmployer(contract));
				}
				else
				{
					model.InProgressContracts.Add(getContractInfoForEmployer(contract));
				}
			}

			foreach (var subscription in subscriptions)
			{
				model.Subscriptions.Add(getSubscriptionInfoForEmployer(subscription));
			}

			model = sortSubcriptionsForEmployer(sortOrder, model, lastSort);

			return model;
		}

		public DetailsForCoordinatorView getDetailsForCoordinator(ApplicationUser freelancer, String _info, String sortOrder, String lastSort, bool hideFailed)
		{
			if (_info == null)
			{
				_info = "profile";
			}

			string id = freelancer.Id;
			List<ContractModels> contracts = db.ContractModels
				.Where(c => c.Freelancer.Id == freelancer.Id).ToList();

			DocumentPackageModels documents = getDocumentsForUser(freelancer);

			DetailsForCoordinatorView model = new DetailsForCoordinatorView
			{
				info = _info,
				General = documents.General,
				Passport = documents.Passport,
				Bank = documents.Bank,
				PhotoPath = Utils.GetPhotoUrl(freelancer.PhotoPath),
				FreelancerRate = countRating(id),
				FreelancerId = id,
				isApproved = freelancer.IsApprovedByCoordinator == true ? true : false,

				ProfileView = new DetailsProfileView
				{
					freelancer = freelancer,

					LawContracts = db.LawContracts
						.Where(
							c => c.User.Id == id)
						.Select(
							c => new FreelancerLawContractViewModel
							{
								LawFace = c.LawFace.Name,
								LawFacePath = c.Path,
								StartingDate = c.EndDate,
								EndingDate = c.EndDate
							})
						.ToList()
				},

				ProblemsView = new DetailsProblemsView
				{
					freelancer = freelancer,
					UnpaidContracts = extractUnpaidContracts(id),
					OpenContracts = extractOpenContracts(id),
					archieve = getArchievedContracts(id, hideFailed),
					SubscribedProblems = extractSubscribedProblems(id)
				}
			};

			model.ProblemsView.SubscribedProblems = sortProblemsForCoordinator(sortOrder, model.ProblemsView.SubscribedProblems, lastSort);
			model.lastSort = sortOrder;
			return model;
		}

		private int getContractCommentsNumber(int contractId, string userId)
		{
			int number = 0;
			try {
				number = ChatController.FindContractChatId(contractId);
			} catch (Exception) {}
			return ChatController.CalcUserInfo(userId, number).UnreadMessagesCount;
		}

		private int getProblemCommentsNumber(int problemId, string userId) 
		{
			int number = 0;
			try {
				number = ChatController.FindProblemChatId(problemId);
			} catch (Exception) {}
			return ChatController.CalcUserInfo(userId, number).UnreadMessagesCount;
		}

		private List<ContractHomeViewModel> getContractsInState(string userId, ContractStatus status)
		{
			List<ContractModels> contracts = db.ContractModels.Where(
				contract => (contract.Status == status && contract.Freelancer.Id == userId)).ToList();
			List<ContractHomeViewModel> result = new List<ContractHomeViewModel>();
			foreach (var contract in contracts)
			{
				result.Add(new ContractHomeViewModel()
				{
					Author = contract.Problem.Employer.FIO,
					CommentNumber = getContractCommentsNumber(contract.ContractId, userId),
					ContractId = contract.ContractId,
					Deadline = contract.EndingDate,
					Name = contract.Problem.Name
				});
			}
			return result;
		}

		private List<ProblemHomeViewModel> getProblemInState(string userId, ProblemStatus status)
		{
			List<ProblemModels> problems = db.ProblemModels.Where(
				problem => (problem.Status == status)).ToList();
			List<ProblemHomeViewModel> result = new List<ProblemHomeViewModel>();
			foreach (var problem in problems)
			{
				if (problem.Subscriptions.Where(subscription => subscription.Freelancer.Id == userId).ToList().Capacity == 0)
				{
					continue;
				}

				result.Add(new ProblemHomeViewModel()
				{
					CommentNumber = getProblemCommentsNumber(problem.ProblemId, userId),
					Money = problem.Cost,
					Name = problem.Name,
					ProblemId = problem.ProblemId,
					DeadlineDate = problem.DeadlineDate,
					ShortDescription = problem.SmallDescription,
					Others = problem.Subscriptions.Count
				});
			}
			return result;
		}

		// GET: Freelancer
		public ActionResult Home()
		{
			if (User.IsInRole("Incognito"))
			{
				return RedirectToAction("Profile");
			}
			string userId = User.Identity.GetUserId();

			HomeViewModel model = new HomeViewModel();
			model.ContractsInWork = getContractsInState(userId, ContractStatus.InProgress);
			model.ContractsPending = getContractsInState(userId, ContractStatus.Opened);
			model.ContractsWaitingMoney = getContractsInState(userId, ContractStatus.ClosedNotPaid);
			model.Problems = getProblemInState(userId, ProblemStatus.Opened);
			if (model.ContractsInWork.Count + model.ContractsPending.Count + model.ContractsWaitingMoney.Count +
				model.Problems.Count == 0)
			{
				return RedirectToAction("OpenProblems");
			}
			return View(model);
		}

		/// <param name="info">Профиль -> profile, Задачи -> problems, Отзывы -> comments</param>
		public ActionResult Details(string id, String sortOrder, String info, String lastSort, bool hideFailed = false)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ViewBag.hideFailed = hideFailed;
			ApplicationUser freelancerModel = db.Users.Find(id);
			if (freelancerModel == null)
			{
				return HttpNotFound();
			}

			if (User.IsInRole("Employer"))
			{
				return PartialView("_DetailsForEmployer", getDetailsForEmployer(freelancerModel, info, sortOrder, lastSort));
			}

			if (User.IsInRole("Coordinator"))
			{
				return PartialView("_DetailsForCoordinator", getDetailsForCoordinator(freelancerModel, info, sortOrder, lastSort, hideFailed));
			}
			
			return View();
		}

		/* 
			Count the rating for freelancer with id
		*/
		private decimal countRating(string id)
		{
			decimal rating = 0;
			List<ContractModels> contracts = db.ContractModels
				.Where(
					c => c.Freelancer.Id == id
					&& (c.Status == ContractStatus.Closed
					|| c.Status == ContractStatus.ClosedNotPaid
					|| c.Status == ContractStatus.Failed
					|| c.Status == ContractStatus.СancelledByEmployer))
				.ToList();
			foreach (var contract in contracts)
			{
				rating += contract.Rate;
			}
			if (contracts.Count != 0)
			{
				rating = rating / contracts.Count;
			}
			return rating;
		}

		public class FreelancerProblemViewModel
		{
			public int ProblemId { get; set; }
			public String Name { get; set; }
			public String EmployerName { get; set; }
			public String EmployerId { get; set; }
			public decimal Cost { get; set; }
			public DateTime EndingDate { get; set; }
		}

		public class DetailsProfileView
		{
			public ApplicationUser freelancer { get; set; }
			public List<FreelancerLawContractViewModel> LawContracts { get; set; }
		}

		public class DetailsProblemsView
		{
			public ApplicationUser freelancer { get; set; }
			public List<ArchivedContractViewModel> archieve { get; set; }
			public List<FreelancerContractViewModel> OpenContracts { get; set; }
			public List<FreelancerContractViewModel> UnpaidContracts { get; set; }
			public List<FreelancerProblemViewModel> SubscribedProblems { get; set; }
		}

		// todo: comments

		/*
			Auxiliary methods return view models for page
			Координатор. Фрилансер. Вкладка Задачи
		*/

		private List<FreelancerContractViewModel> extractUnpaidContracts(string freelancerId)
		{
			return db.ContractModels
				.Where(
					c => c.Freelancer.Id == freelancerId
						&& c.Status == ContractStatus.ClosedNotPaid)
				.Select(
					c => new FreelancerContractViewModel
					{
						ContractId = c.ContractId,
						EmployerName = c.Problem.Employer.FIO,
						EmployerId = c.Problem.Employer.Id,
						Name = c.Problem.Name,
						Comment = c.Comment,
						Rate = c.Rate,
						EndingDate = c.EndingDate,
						DeadlineDate = c.DeadlineDate,
						Cost = c.Cost
					})
				.ToList();
		}

		private List<FreelancerContractViewModel> extractOpenContracts(string freelancerId)
		{
			return db.ContractModels
				.Where(
					c => c.Freelancer.Id == freelancerId
						&& c.Status != ContractStatus.Closed)
				.Select(
					c => new FreelancerContractViewModel
					{
						ContractId = c.ContractId,
						EmployerName = c.Problem.Employer.FIO,
						EmployerId = c.Problem.Employer.Id,
						Name = c.Problem.Name,
						Comment = c.Comment,
						Rate = c.Rate,
						EndingDate = c.EndingDate,
						StartDate = c.CreationDate,
						DeadlineDate = c.DeadlineDate,
						Cost = c.Cost
					})
				.ToList();
		}

		private List<FreelancerProblemViewModel> extractSubscribedProblems(string freelancerId)
		{
			return db.SubscriptionModels
				.Where(
					c => c.Freelancer.Id == freelancerId
						&& c.Problem.Status != ProblemStatus.Closed)
				.Select(
					c => new FreelancerProblemViewModel
					{
						ProblemId = c.Problem.ProblemId,
						EmployerName = c.Problem.Employer.FIO,
						EmployerId = c.Problem.Employer.Id,
						Name = c.Problem.Name,
						EndingDate = c.Problem.DeadlineDate,
						Cost = c.Problem.Cost
					})
				.ToList();
		}

		public static String getStatusMessage(ContractStatus status)
		{
			String result = "";
			switch (status)
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
				EmployerName = c.Problem.Employer.FIO,
				Details = c.Details,
				EmployerId = c.Problem.Employer.Id,
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

		private List<ArchivedContractViewModel> getArchievedContracts(String userId, bool hideFailed)
		{
			ICollection<ContractModels> contracts = db.ContractModels
				.Where(
					c => c.Freelancer.Id == userId
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

		public ActionResult Archive(String sortOrder, bool hideFailed = false)
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

		[Authorize(Roles = "Admin, Freelancer, Coordinator, WithoutDocuments")]
		public ActionResult Contract(int id)
		{
			return View(db.ContractModels.Find(id));
		}

		public class ProblemView
		{
			public ProblemModels ProblemModels { get; set; }
			public bool IsSubscibed { get; set; }
		}

		[Authorize(Roles = "Admin, Freelancer, Coordinator, WithoutDocuments")]
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

		public OpenProblemsInfo sortProblems(OpenProblemsInfo model, String sortOrder, String lastSort)
		{

			if (lastSort == sortOrder)
			{
				switch (sortOrder)
				{
					case "cost":
						sortOrder = "cost_desc";
						break;
					case "creationDate":
						sortOrder = "creationDate_desc";
						break;
					case "deadlineDate":
						sortOrder = "deadlineDate_desc";
						break;
				}
			}

			switch (sortOrder)
			{
				case "cost_desc":
					model.openProblems = model.openProblems.OrderByDescending(s => s.Cost).ToList();
					break;
				case "cost":
					model.openProblems = model.openProblems.OrderBy(s => s.Cost).ToList();
					break;

				case "creationDate_desc":
					model.openProblems = model.openProblems.OrderByDescending(s => s.CreationDate).ToList();
					break;
				case "creationDate":
					ViewBag.sortCost = "creationDate_desc";
					model.openProblems = model.openProblems.OrderBy(s => s.CreationDate).ToList();
					break;

				case "deadlineDate_desc":
					model.openProblems = model.openProblems.OrderByDescending(s => s.DeadlineDate).ToList();
					break;
				case "deadlineDate":
					ViewBag.sortCost = "deadlineDate_desc";
					model.openProblems = model.openProblems.OrderBy(s => s.DeadlineDate).ToList();
					break;

				default:
					model.openProblems = model.openProblems.OrderBy(s => s.CreationDate).ToList();
					break;
			}

			model.lastSort = sortOrder;

			return model;
		}

		public OpenProblemsInfo getOpenProblemsInfo(String sortOrder, string lastSort, string showSubscriptionsParam)
		{
			List<ProblemModels> problems = db.ProblemModels
				.Where(p => (p.Status == ProblemStatus.InProgress || p.Status == ProblemStatus.Opened) 
									&& p.Employer.IsApprovedByCoordinator == true)
				.ToList();

			OpenProblemsInfo model = new OpenProblemsInfo
			{
				openProblems = new List<OpenProblemInfo>(),
				showSubscriptions = showSubscriptionsParam,
				isApproved = getDocuments().IsApproved == null ? false : getDocuments().IsApproved.Value
			};

			foreach (var p in problems)
			{
				OpenProblemInfo info = getOpenProblemInfo(p);
				if (!info.IsSubscribed || showSubscriptionsParam != null)
				{
					model.openProblems.Add(info);
				}
			}

			model = sortProblems(model, sortOrder, lastSort);

			return model;
		}

		[Authorize(Roles = "Admin, Freelancer, Coordinator, WithoutDocuments, Employer")]
		public ActionResult OpenProblems(String sortOrder, string searchString, string lastSort, string showSubscriptionsParam)
		{
			if (User.IsInRole("Employer") || User.IsInRole("Coordinator"))
			{
				return PartialView("_OpenProblemsForEmployer", getOpenProblemsInfo(sortOrder, lastSort, "show"));
			}

			if (User.IsInRole("Freelancer"))
			{
				return View(getOpenProblemsInfo(sortOrder, lastSort, showSubscriptionsParam));
			}

			return Redirect("index");
		}

		[Authorize(Roles = "Freelancer")]
		public ActionResult Profile()
		{
			String id = User.Identity.GetUserId();
			ApplicationUser freelancer = db.Users.Find(id);

			DocumentPackageModels documents = getDocuments();
			ProfileView model = new ProfileView
			{
				FreelancerEmail = freelancer.Email,
				FreelancerPhoto = Utils.GetPhotoUrl(freelancer.PhotoPath),
				OpenContractsCount = db.ContractModels.Where(
					c => c.Freelancer.Id == id && (c.Status == ContractStatus.Done
					|| c.Status == ContractStatus.InProgress || c.Status == ContractStatus.ClosedNotPaid
					|| c.Status == ContractStatus.Opened)).Count(),
				ClosedContractsCount = db.ContractModels.Where(
					c => c.Freelancer.Id == id && (c.Status == ContractStatus.Closed
					|| c.Status == ContractStatus.Failed || c.Status == ContractStatus.СancelledByEmployer
					|| c.Status == ContractStatus.СancelledByFreelancer)).Count(),
				emailNotifications = freelancer.EmailNotificationPolicy,
				TotalMoney = getTotalMoney(),
				GpHList = new List<GPHInfo>(), // TODO
				Passport = documents.Passport,
				GeneralInfo = documents.General,
				Bank = documents.Bank,
				PhotoAdress = "", // TODO
				PhotoFirstPage = "", // TODO
				Rate = countRating(id)
			};
			return View(model);
		}

		private decimal getTotalMoney()
		{
			decimal result = 0;
			try
			{
				result =
					db.ContractModels.Where(c => c.Freelancer.Id == User.Identity.GetUserId() && (c.Status == ContractStatus.Closed))
						.Select(c => c.Cost).Sum();
			}
			catch (Exception e)
			{
			}
			return 0;
		}

		public ActionResult Documents()
		{
			string userId = User.Identity.GetUserId();
			ApplicationUser freelancer = db.Users.Find(userId);
			DocumentPackageViewModel model = new DocumentPackageViewModel();
			DocumentPackageModels documents = getDocuments();
			model.General = documents.General;
			model.Passport = documents.Passport;
			model.Bank = documents.Bank;
			model.Photos = documents.Photos;
			return View(model);
		}

		public class DocumentPackageViewModel
		{
			public PassportInfo Passport { get; set; }
			public BankInfo Bank { get; set; }
			public GeneralInfo General { get; set; }
			public Photos Photos { get; set; }
		}

		[HttpPost]
		public ActionResult CreateDocumentTextFields(DocumentPackageViewModel documentFromView)
		{
			if (documentFromView != null)
			{
				DocumentPackageModels documents = getDocuments();
				documents.General = documentFromView.General;
				documents.Passport = documentFromView.Passport;
				documents.Bank = documentFromView.Bank;
				db.SaveChanges();
			}
			return RedirectToAction("Documents");
		}

		[HttpPost]
		public ActionResult UploadPassportFace()
		{
			if (Request.Files.Count > 0)
			{
				var file = Request.Files[0];
				if (file != null && file.ContentLength > 0)
				{
					getDocuments().Photos.PassportFace = saveDocumentOnDisc(file, "passports");
					db.SaveChanges();
				}

			}
			return RedirectToAction("Documents");
		}

		[HttpPost]
		public ActionResult UploadPassportRegistration()
		{
			if (Request.Files.Count > 0)
			{
				var file = Request.Files[0];
				if (file != null && file.ContentLength > 0)
				{
					getDocuments().Photos.PassportRegistration = saveDocumentOnDisc(file, "registrations");
					db.SaveChanges();
				}
			}
			return RedirectToAction("Documents");
		}

		private FileContentResult viewFile(string pathToContract)
		{
			byte[] filedata = System.IO.File.ReadAllBytes(pathToContract);
			string contentType = MimeMapping.GetMimeMapping(pathToContract);
			var cd = new System.Net.Mime.ContentDisposition
			{
				FileName = pathToContract,
				Inline = true,
			};
			Response.AppendHeader("Content-Disposition", cd.ToString());
			return File(filedata, contentType);
		}

		private void saveLawContractInDatabase(ApplicationUser user, string pathToContract,
			LawContractTemplate lawContractTemplate)
		{
			LawContract lawContract = new LawContract
			{
				Path = pathToContract,
				User = user,
				LawContractTemplate = lawContractTemplate
			};
			db.LawContracts.Add(lawContract);
			db.SaveChanges();
		}

		private string saveDocumentOnDisc(HttpPostedFileBase file, string dir, string location = "/App_Data/")
		{
			var ext = Path.GetExtension(file.FileName);
			var fileName = User.Identity.GetUserId() + "_" + DateTime.Now.Ticks.ToString() + ext;
			var path = Path.Combine(Server.MapPath("~" + location + dir + "/"), fileName);
			file.SaveAs(path);
			return location + dir + "/" + fileName;
		}

		private DocumentPackageModels getDocuments()
		{
			return getDocumentsForUser(db.Users.Find(User.Identity.GetUserId()));
		}

		private DocumentPackageModels getDocumentsForUser(ApplicationUser user)
		{
			DocumentPackageModels documents = user.DocumentPackage;
			if (documents == null)
			{
				documents = new DocumentPackageModels { IsApproved = user.IsApprovedByCoordinator, Bank = new BankInfo(), General = new GeneralInfo(), Passport = new PassportInfo(), Photos = new Photos() };
				documents.Freelancer = user;
				db.DocumentPackageModels.Add(documents);
				db.SaveChanges();
				user.DocumentPackage = documents;
				db.SaveChanges();
			}
			return documents;
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
			return RedirectToAction("Profile");
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