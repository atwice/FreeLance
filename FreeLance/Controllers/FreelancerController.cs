using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FreeLance.Code;
using Microsoft.AspNet.Identity;
using Novacode;

namespace FreeLance.Controllers
{
	[Authorize(Roles = "Admin, Freelancer, Incognito, Coordinator, WithoutDocuments")]
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
            ProblemModels[] openProblems = db.ProblemModels
				.Where(x => x.Status == 0 && x.Employer.IsApprovedByCoordinator).ToArray();
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


		public class DocumentPackageViewModel
		{
			public string Adress { get; set; }
			public string Phone { get; set; }
			public string PaymentDetails { get; set; }
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
			return RedirectToAction("Profile");
		}

		[HttpPost]
		public ActionResult UploadPassportFace()
		{
			if (Request.Files.Count > 0)
			{
				var file = Request.Files[0];
				if (file != null && file.ContentLength > 0)
				{
					getDocuments().FilePassportFace = SaveDocumentOnDisc(file, "passports");
					db.SaveChanges();
				}

			}
			return RedirectToAction("Profile");
		}

		[HttpPost]
		public ActionResult UploadPassportRegistration()
		{
			if (Request.Files.Count > 0)
			{
				var file = Request.Files[0];
				if (file != null && file.ContentLength > 0)
				{
					getDocuments().FilePassportRegistration = SaveDocumentOnDisc(file, "registrations");
					db.SaveChanges();
				}
			}
			return RedirectToAction("Profile");
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


        private string SaveDocumentOnDisc(HttpPostedFileBase file, string dir)
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

	}
}