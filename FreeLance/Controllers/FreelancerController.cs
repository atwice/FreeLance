﻿using FreeLance.Models;
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

		public ActionResult Index()
		{
			return RedirectToAction("Home");
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

		public ActionResult Details(string id)
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

		[Authorize(Roles = "Admin, Freelancer, Coordinator, WithoutDocuments")]
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