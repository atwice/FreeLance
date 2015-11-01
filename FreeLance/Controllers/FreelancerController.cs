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
			public String Comment { get; set; }
			public decimal Rate { get; set; }
			public DateTime EndingDate { get; set; }
			public ContractStatus Status { get; set; }
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
			public int OpenContractsCount { get; set; }
			public int ClosedContractsCount { get; set; }
			public List<FreelancerContractViewModel> Contracts { get; set; }
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
			foreach(var contract in contracts)
			{
				if(checkIfContractRated(contract.Status)) {
					ratedContrats += 1;
					rate += contract.Rate;
				}
			}
			if(ratedContrats != 0)
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

			foreach(var contract in contracts)
			{
				if (checkIfContractClosed(contract.Status)){
					model.ClosedContracts.Add(getContractInfoForEmployer(contract));
				} else
				{
					model.InProgressContracts.Add(getContractInfoForEmployer(contract));
				}
			}

			foreach (var subscription in subscriptions)
			{
				model.Subscriptions.Add(getSubscriptionInfoForEmployer(subscription));
			}

			model= sortSubcriptionsForEmployer(sortOrder, model, lastSort);

			return model;
		}

		// GET: Freelancer
		public ActionResult Home()
		{
			if (User.IsInRole("Incognito"))
			{
				return RedirectToAction("Profile");
			}
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
				Contracts = db.ContractModels
				.Where(
					c => c.Freelancer.Id == id)
				.Select(
					c => new FreelancerContractViewModel
					{
						ContractId = c.ContractId,
						EmployerName = c.Problem.Employer.FIO,
						Name = c.Problem.Name,
						Comment = c.Comment,
						Rate = c.Rate,
						EndingDate = c.EndingDate,
						Status = c.Status
					})
				.ToList(),
				ClosedContractsCount = 0,
				OpenContractsCount = 0
			};
			decimal rate = 0;
			foreach (var contract in view.Contracts)
			{
				if(contract.Status == ContractStatus.Closed)
				{
					rate += contract.Rate;
					view.ClosedContractsCount += 1;
				} else if(contract.Status == ContractStatus.InProgress ||
					contract.Status == ContractStatus.Opened)
				{
					view.OpenContractsCount += 1;
				}
			}
			if(view.ClosedContractsCount != 0)
			{
				view.Rate = rate / view.ClosedContractsCount;
			}
			ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
			switch (sortOrder)
			{
				case "name_desc":
					view.Contracts = view.Contracts.OrderByDescending(с => с.Name).ToList();
					break;
				case "Date":
					view.Contracts = view.Contracts.OrderBy(s => s.EndingDate).ToList();
					break;
				case "date_desc":
					view.Contracts = view.Contracts.OrderByDescending(s => s.EndingDate).ToList();
					break;
				default:
					view.Contracts = view.Contracts.OrderBy(s => s.Name).ToList();
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

		[Authorize(Roles = "Admin, Freelancer, Coordinator, WithoutDocuments, Employer")]
		public ViewResult OpenProblems(String sortOrder, string searchString)
		{
			if (User.IsInRole("Freelancer"))
			{
				string userId = User.Identity.GetUserId();
				ApplicationUser freelancer = db.Users.Find(userId);
				bool withLawContract = db.LawContracts.Where(c => c.User.Id == userId).Count() > 0 ? true : false;
				if (!withLawContract)
				{
					ViewBag.ErrorMessage = "Вам не заплатят за выполненную работу, пока вы не заключите ГПХ.";
				}
			}
			ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
			var openProblems = from s in db.ProblemModels where s.Status == 0 select s;
			if (!String.IsNullOrEmpty(searchString))
			{
				openProblems = openProblems.Where(s => s.Name.Contains(searchString) || s.Description.Contains(searchString) || s.SmallDescription.Contains(searchString));
			}

			switch (sortOrder)
			{
				case "name_desc":
					openProblems = openProblems.OrderByDescending(s => s.Name);
					break;
				case "Date":
					openProblems = openProblems.OrderBy(s => s.CreationDate);
					break;
				case "date_desc":
					openProblems = openProblems.OrderByDescending(s => s.CreationDate);
					break;
				default:
					openProblems = openProblems.OrderBy(s => s.Name);
					break;
			}
			return View(openProblems.ToList());

			//ProblemModels[] openProblems = db.ProblemModels
			//	.Where(x => x.Status == 0 && x.Employer.IsApprovedByCoordinator).ToArray();
			//return View(openProblems);
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