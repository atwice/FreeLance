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
			public int ContractId { get; set; }
			public String Name { get; set; }
			public String EmployerName { get; set; }
			public String Details { get; set; }
		}

		public class FreelancerContractViewModel
		{
			public int ContractId { get; set; }
			public String Name { get; set; }
			public String EmployerName { get; set; }
			public String EmployerId { get; set; }
			public String Comment { get; set; }
			public decimal Rate { get; set; }
			public DateTime EndingDate { get; set; }
			public decimal Cost { get; set; }
		}

		public class FreelancerLawContractViewModel
		{
			public String LawFace { get; set; }
			public DateTime StartingDate { get; set; }
			public DateTime EndingDate { get; set; }
		}

		public class ArchiveViewModel
		{
			public List<ArchivedContractViewModel> SuccessfulContracts { get; set; } // with Closed status
			public List<ArchivedContractViewModel> FailedContracts { get; set; } // with CancelledByEmpoyer, CancelledByFreelancer & Failed status
		}

		public class DetailsView
		{
			public ApplicationUser freelancer { get; set; }
			public decimal Rate { get; set; }
			public List<FreelancerLawContractViewModel> LawContracts { get; set; }
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
				DeadlineDate = DateTime.Now.AddDays(100).ToShortDateString(), // TODO
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
				DeadlineDate = DateTime.Now.AddDays(100), // TODO
				Cost = subscription.Problem.Cost,
				EmployerName = subscription.Problem.Employer.FIO,
				EmployerId = subscription.Problem.Employer.Id
			};
			return info;
		}

		public class OpenProblemsInfo
		{
			public String lastSort { get; set; }
			public string showSubscriptions { get; set; }
			public List<OpenProblemInfo> openProblems { get; set; }
		}

		public class OpenProblemInfo
		{
			public String ProblemName { get; set; }
			public String ProblemShortDescription { get; set; }
			public String EmployerName { get; set; }
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
				CreationDate = problem.CreationDate,
				DeadlineDate = DateTime.Now.AddDays(50), // TODO
				Cost = problem.Cost,
				AmountOfWorkers = 10, // TODO
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

		public bool checkIfContractRated(ContractStatus status)
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

		public decimal getFreelancerRate(List<ContractModels> contracts)
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
				PhotoPath = "/Content/placeholder_avatar.png", //TODO
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


		private int getContractCommentsNumber(long contractId)
		{
			// todo get right new messages number
			return 6;
		}

		private int getProblemCommentsNumber(long problemId)
		{
			// todo get right new messages number
			return 5;
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
					CommentNumber = getContractCommentsNumber(contract.ContractId),
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
					CommentNumber = getProblemCommentsNumber(problem.ProblemId),
					Money = problem.Cost,
					Name = problem.Name,
					ProblemId = problem.ProblemId,
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

			/*var viewModel = new HomeView
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
			};*/

			return View(model);
		}

		/// <param name="info">Профиль -> profile, Задачи -> problems, Отзывы -> comments</param>
		public ActionResult Details(string id, String sortOrder, String info, String lastSort)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ApplicationUser freelancerModel = db.Users.Find(id);
			if (freelancerModel == null)
			{
				return HttpNotFound();
			}

			if (User.IsInRole("Employer"))
			{
				return PartialView("_DetailsForEmployer", getDetailsForEmployer(freelancerModel, info, sortOrder, lastSort));
			}

			DetailsView view = new DetailsView
			{
				freelancer = freelancerModel,
				LawContracts = db.LawContracts
				.Where(
					c => c.User.Id == id)
				.Select(
					c => new FreelancerLawContractViewModel
					{
						LawFace = c.LawContractTemplate.LawFace.Name,
						StartingDate = c.EndData,
						EndingDate = c.EndData
					})
				.ToList(),
				Rate = countRating(id)
			};
			return View(view);
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
					&& c.Status == ContractStatus.Closed)
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
			public decimal Cost { get; set; }
			public DateTime EndingDate { get; set; }
		}

		public class ProblemsView
		{
			public ApplicationUser freelancer { get; set; }
			public decimal Rate { get; set; }
			public List<FreelancerContractViewModel> OpenContracts { get; set; }
			public List<FreelancerContractViewModel> UnpaidContracts { get; set; }
			public List<FreelancerProblemViewModel> SubscribedProblems { get; set; }
		}

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
						Name = c.Problem.Name,
						EndingDate = c.Problem.CreationDate,
						Cost = c.Problem.Cost
					})
				.ToList();
		}

		/*
			Координатор. Фрилансер. Вкладка Задачи
		*/
		[Authorize(Roles = "Admin, Coordinator")]
		public ActionResult Problems(string id, String sortOrder)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ApplicationUser freelancerModel = db.Users.Find(id);
			if (freelancerModel == null)
			{
				return HttpNotFound();
			}
			ProblemsView view = new ProblemsView
			{
				freelancer = freelancerModel,
				Rate = countRating(id),
				UnpaidContracts = extractUnpaidContracts(id),
				OpenContracts = extractOpenContracts(id),
				SubscribedProblems = extractSubscribedProblems(id)
			};
			ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
			switch (sortOrder)
			{
				case "name_desc":
					view.SubscribedProblems = view.SubscribedProblems.OrderByDescending(с => с.Name).ToList();
					break;
				case "Date":
					view.SubscribedProblems = view.SubscribedProblems.OrderBy(s => s.EndingDate).ToList();
					break;
				case "date_desc":
					view.SubscribedProblems = view.SubscribedProblems.OrderByDescending(s => s.EndingDate).ToList();
					break;
				default:
					view.SubscribedProblems = view.SubscribedProblems.OrderBy(s => s.Name).ToList();
					break;
			}
			return View(view);
		}

		[Authorize(Roles = "Admin, Freelancer, Coordinator, WithoutDocuments")]
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
						EmployerName = c.Problem.Employer.FIO,
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
						EmployerName = c.Problem.Employer.FIO,
						Name = c.Problem.Name,
						Details = c.Details
					})
				.ToList();
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
				.Where(p => p.Status == ProblemStatus.InProgress || p.Status == ProblemStatus.Opened)
				.ToList();

			OpenProblemsInfo model = new OpenProblemsInfo
			{
				openProblems = new List<OpenProblemInfo>(),
				showSubscriptions = showSubscriptionsParam
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
			if (User.IsInRole("Employer"))
			{
				return PartialView("_OpenProblemsForEmployer", getOpenProblemsInfo(sortOrder, lastSort, "show"));
			}

			if (User.IsInRole("Freelancer"))
			{
				return View(getOpenProblemsInfo(sortOrder, lastSort, showSubscriptionsParam));
			}

			return Redirect("index");
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

		public ActionResult Documents()
		{
			string userId = User.Identity.GetUserId();
			ApplicationUser freelancer = db.Users.Find(userId);
			DocumentPackageViewModel model = new DocumentPackageViewModel();
			if (freelancer.DocumentPackage != null)
			{
				DocumentPackageModels documents = freelancer.DocumentPackage;
				model.Phone = documents.Phone;
				model.PaymentDetails = documents.PaymentDetails;
				model.Adress = documents.Adress;
				model.PassportFace = documents.FilePassportFace != null;
				model.PassportRegistration = documents.FilePassportRegistration != null;
			}
			return View(model);
		}

		public class DocumentPackageViewModel
		{
			public string Adress { get; set; }
			public string Phone { get; set; }
			public string PaymentDetails { get; set; }
			public bool PassportFace { get; set; }
			public bool PassportRegistration { get; set; }
		}

		[HttpPost]
		public ActionResult CreateDocumentTextFields(DocumentPackageViewModel documentFromView)
		{
			if (documentFromView != null)
			{
				DocumentPackageModels documents = getDocuments();
				documents.Adress = documentFromView.Adress;
				documents.Phone = documentFromView.Phone;
				documents.PaymentDetails = documentFromView.PaymentDetails;
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
					getDocuments().FilePassportFace = saveDocumentOnDisc(file, "passports");
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
					getDocuments().FilePassportRegistration = saveDocumentOnDisc(file, "registrations");
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

		private string saveDocumentOnDisc(HttpPostedFileBase file, string dir)
		{
			var ext = Path.GetExtension(file.FileName);
			var fileName = User.Identity.GetUserId() + "_" + DateTime.Now.Ticks.ToString() + ext;
			var path = Path.Combine(Server.MapPath("~/App_Data/" + dir + "/"), fileName);
			file.SaveAs(path);
			return "/App_Data/" + dir + "/" + fileName;
		}

		private DocumentPackageModels getDocuments()
		{
			ApplicationUser user = db.Users.Find(User.Identity.GetUserId());
			DocumentPackageModels documents = user.DocumentPackage;
			if (documents == null)
			{
				documents = new DocumentPackageModels { IsApproved = user.IsApprovedByCoordinator };
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
			return View(user.EmailNotificationPolicy);
		}

	}
}