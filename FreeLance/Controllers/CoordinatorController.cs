using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Novacode;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.IO;
using FreeLance.Code;
using Microsoft.Ajax.Utilities;
using System.Data.Entity.Validation;

namespace FreeLance.Controllers
{
	[Authorize(Roles = "Coordinator")]
	public class CoordinatorController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		public class HomeViewModel
		{
			public List<ApplicationUser> IncognitosSmallList { get; set; }
			public List<ApplicationUser> WithoutDocumentsSmallList { get; set; }

			public List<ContractModels> ContractsList { get; set; }
			public List<ContractModels> ContractsToPay { get; set; }
			public List<ApplicationUser> NewEmployers { get; set; }
			public List<ApplicationUser> NewFreelancers { get; set; }
			public List<ContractModels> ContractWithComments { get; set; }
			public List<DocumentPackageModels> DocumetsUnapproved { get; set; }
		}

		public class EmployerViewModel
		{
			public String Name { get; set; }
			public string Id { get; set; }
			public string Email { get; set; }
			public int ClosedContractsCount { get; set; }
			public int OpenContractsCount { get; set; }
			public virtual ApplicationUser Employer { get; set; }
		}

		public ActionResult Index()
		{
			return RedirectToAction("Home");
		}

		public ActionResult Home()
		{
			var model = new HomeViewModel();
			model.ContractsList = db.ContractModels.Where(x => x.IsApprovedByCoordinator == false).ToList();
			model.ContractsToPay = db.ContractModels.Where(x => x.Status == ContractStatus.ClosedNotPaid).ToList();
			model.NewEmployers = getApplicationUsersApproved(null, "Employer").ToList();
			model.NewFreelancers = getApplicationUsersApproved(null, "Freelancer").ToList();
			model.ContractWithComments = db.ContractModels.Where(x => x.Comment != null).ToList();
			model.DocumetsUnapproved = db.DocumentPackageModels.Where(x => x.IsApproved == null).ToList();
			ViewBag.LawFaceChooseView = new LawModelsManager.LawFaceChooseView();
			return View(model);
		}

		public ActionResult Freelancers(String searchString, String sortOrder)
		{
			List<string> Ids = AccountController.GetApplicationUsersInRole(db, "Freelancer").Select(
				u => u.Id).ToList();
			List<FreelancerViewModel> model = new List<FreelancerViewModel>();
			foreach (var id in Ids)
			{
				model.Add(new FreelancerViewModel(id));
			}

			if (!String.IsNullOrEmpty(searchString))
			{
				model = model.Where(c => c.Name.Contains(searchString) || c.Email.Contains(searchString)).ToList();
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

		private IEnumerable<ApplicationUser> getApplicationUsersApproved(bool? approved, string roleName)
		{
			return getApplicationUsersInRole(roleName)
				.Where(user => user.IsApprovedByCoordinator == approved);
		}

		private IEnumerable<ApplicationUser> getApplicationUsersInRole(string roleName)
		{
			return from role in db.Roles
				   where role.Name == roleName
				   from userRoles in role.Users
				   join user in db.Users
					   on userRoles.UserId equals user.Id
				   select user;
		}

		public class FreelancerDocuments
		{
			public DocumentPackageModels Document;
			// other info;
		}

		public ActionResult ViewFreelancerDocuments(String userId)
		{
			ApplicationUser freelancer = db.Users.Find(userId);
			FreelancerDocuments documents = new FreelancerDocuments();
			documents.Document = freelancer.DocumentPackage;
			return View(documents);
		}

		public ActionResult DownloadDoumentImage(string documentPath)
		{
			string filepath = AppDomain.CurrentDomain.BaseDirectory + documentPath;
			byte[] filedata = System.IO.File.ReadAllBytes(filepath);
			string contentType = MimeMapping.GetMimeMapping(filepath);
			return File(filedata, contentType);
		}

		public ActionResult ApproveFreelancer(string freelancerId)
		{
			ApplicationUser freelancer = db.Users.Find(freelancerId);
			freelancer.IsApprovedByCoordinator = !freelancer.IsApprovedByCoordinator;
			db.SaveChanges();
			return RedirectToAction("ViewFreelancerDocuments", new { userId = freelancerId });
		}

		public ActionResult ChangeProblemVisibility(int problemId)
		{
			ProblemModels problem = db.ProblemModels.Include(p => p.Employer).Single(p => p.ProblemId == problemId);
			problem.IsHidden = !problem.IsHidden;
			db.SaveChanges();
			return Redirect("\\Problem\\Details\\" + problemId);
		}

		public ActionResult ChangeContractVisibility(int contractId)
		{
			ContractModels contract =
				db.ContractModels.Include(c => c.Freelancer).Include(c => c.Problem).Single(c => c.ContractId == contractId);
			contract.IsHidden = !contract.IsHidden;
			db.SaveChanges();
			return Redirect("\\Contract\\Details\\" + contractId);
		}

		public ActionResult Employers(String searchString, String sortOrder)
		{
			List<string> Ids = AccountController.GetApplicationUsersInRole(db, "Employer")
				.Select(u => u.Id)
				.ToList();
			List<EmployerViewModel> model = new List<EmployerViewModel>();
			foreach (var id in Ids)
			{
				model.Add(getInfoAboutEmployer(id));
			}

			if (!String.IsNullOrEmpty(searchString))
			{
				model = model.Where(c => c.Name.Contains(searchString) || c.Email.Contains(searchString)).ToList();
			}

			ViewBag.sortEmail = "email";
			ViewBag.sortName = "name";
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

		private EmployerViewModel getInfoAboutEmployer(String id)
		{
			var model = new EmployerViewModel();
			ApplicationUser employer = db.Users.Find(id);
			List<SmallContractInfoModel> contracts = db.ContractModels
				.Where(
					c => c.Problem.Employer.Id == id)
				.Select(
					c => new SmallContractInfoModel
					{
						Rate = c.Rate,
						Status = c.Status
					})
				.ToList();

			model.Name = employer.FIO;
			model.Email = employer.Email;
			model.ClosedContractsCount = 0;
			model.OpenContractsCount = 0;
			model.Id = id;
			model.Employer = employer;
			int cancelByEmployer = 0;
			foreach (var contract in contracts)
			{
				if (contract.Status == ContractStatus.Closed
					|| contract.Status == ContractStatus.Failed
					|| contract.Status == ContractStatus.СancelledByEmployer
					|| contract.Status == ContractStatus.ClosedNotPaid)
				{
					model.ClosedContractsCount += 1;
				}
				else if (contract.Status == ContractStatus.InProgress ||
						 contract.Status == ContractStatus.Opened)
				{
					model.OpenContractsCount += 1;
				}
				else if (contract.Status == ContractStatus.СancelledByEmployer)
				{
					cancelByEmployer += 1;
				}
			}
			model.ClosedContractsCount += cancelByEmployer;

			return model;
		}

		[HttpPost]
		public ActionResult ChangeEmployerApprovalStatus(string employerId, bool isApproved, string redirect)
		{
			ApplicationUser employer = db.Users.Find(employerId);
			var employerRole = db.Roles.Where(role => role.Name == "employer").ToArray()[0];

			if (employer == null || employer.Roles.Where(role => role.RoleId == employerRole.Id).Count() == 0)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			employer.IsApprovedByCoordinator = !isApproved;
			db.SaveChanges();
			return Redirect(redirect == null ? "/Coordinator/Home" : redirect);
		}

		[HttpPost]
		public ActionResult ApproveUserByCoordinator(string userId, bool value)
		{
			ApplicationUser user = db.Users.Find(userId);
			if (user == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			user.IsApprovedByCoordinator = value;
			db.SaveChanges();
			return Redirect("/Coordinator/Home");
		}

		[HttpPost]
		public ActionResult ApproveDocuments(string docId, bool value)
		{
			int id = Int32.Parse(docId);
			DocumentPackageModels doc = db.DocumentPackageModels.Include(x => x.Freelancer).Single(x => x.Id == id);
			if (doc == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			doc.IsApproved = value;
			db.SaveChanges();
			return Redirect("/Coordinator/Home");
		}

		public enum Approvable
		{
			Contract,
			Payment,
		};

		[HttpPost]
		public ActionResult ApproveInContractByCoordinator(string contractId, Approvable approvable)
		{
			int contractIdNum = Int32.Parse(contractId);
			ContractModels contract =
				db.ContractModels.Include(t => t.LawFace)
					.Include(t => t.Problem)
					.Include(t => t.Freelancer)
					.Single(t => t.ContractId == contractIdNum);
			if (contract == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			switch (approvable)
			{
				case Approvable.Contract:
					{
						contract.IsApprovedByCoordinator = true;
						break;
					}
				case Approvable.Payment:
					{
						contract.Status = ContractStatus.Closed;
						break;
					}
			}
			db.SaveChanges();
			return Redirect("/Coordinator/Home");
		}

		[HttpPost]
		public ActionResult IncognitoToFreelancer(string usernameID, string redirect)
		{
			ApplicationUser freelancer = db.Users.Find(usernameID);
			var withoutDoc = db.Roles.Where(role => role.Name == "Freelancer").ToArray()[0];
			if (freelancer == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			freelancer.Roles.Clear();
			freelancer.IsApprovedByCoordinator = false;
			freelancer.Roles.Add(new IdentityUserRole { RoleId = withoutDoc.Id, UserId = freelancer.Id });
			db.SaveChanges();
			return RedirectToAction(redirect);
		}

		[HttpPost]
		public ActionResult IncognitoToTrash(string usernameID, string redirect)
		{
			ApplicationUser freelancer = db.Users.Find(usernameID);
			var withoutDoc = db.Roles.Where(role => role.Name == "Trash").ToArray()[0];
			if (freelancer == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			freelancer.Roles.Clear();
			freelancer.IsApprovedByCoordinator = false;
			freelancer.Roles.Add(new IdentityUserRole { RoleId = withoutDoc.Id, UserId = freelancer.Id });
			db.SaveChanges();
			return RedirectToAction(redirect);
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

		public class UploadPostModel
		{
			public HttpPostedFileBase File { get; set; }
			public string UserId { get; set; }
			public string LawFaceId { get; set; }
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
		}

		public class UploadSignedLawContractModel
		{
			public List<ApplicationUser> Freelancers { get; set; }
			public List<LawContractTemplate> LawContractTemplates { get; set; }
			public List<LawFace> LawFaces { get; set; }
			public UploadPostModel PostModel { get; set; }
		}

		// Координатор загружает подписанный ГПХ
		public ActionResult UploadSignedLawContract()
		{
			var model = new UploadSignedLawContractModel();
			model.LawContractTemplates = db.LawContractTemplates.ToList();
			model.Freelancers = getApplicationUsersInRole("Freelancer").OrderBy(f => f.FIO).ToList();
			model.LawFaces = db.LawFaces.ToList();
			model.PostModel = new UploadPostModel();
			return View(model);
		}

		[HttpPost]
		public ActionResult UploadSignedLawContract([Bind(Prefix = "UploadPostModel")] UploadPostModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.File.ContentLength > 0)
				{
					string path = saveLawContract(model.File);
					addLawContractInDb(path, model.UserId, model.LawFaceId, model.StartDate, model.EndDate);

				}
			}
			return RedirectToAction("Home");
		}

		private void addLawContractInDb(string Path, string UserId, string lawFaceId, DateTime startDate, DateTime EndDate)
		{
			int lawFaceId_ = Int32.Parse(lawFaceId);
			LawFace lawFace = db.LawFaces.Include(c => c.ActiveLawContractTemplate).Single(c => c.Id == lawFaceId_);
			LawContract contract = new LawContract();
			contract.Path = Path;
			contract.StartDate = startDate;
			contract.EndDate = EndDate;
			contract.User = db.Users.Find(UserId);
			contract.LawFace = lawFace;
			contract.LawContractTemplate = lawFace.ActiveLawContractTemplate;
			db.LawContracts.Add(contract);
			db.SaveChanges();
		}


		private string saveLawContract(HttpPostedFileBase file)
		{
			string path = null;
			var fileName = Path.GetFileName(file.FileName);
			path = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\LawContracts\\" + fileName;
			Response.Write(path.ToString());
			file.SaveAs(path);
			return path;
		}

		[HttpPost]
		public void ChangeLawFaceInContract(int contractId, int lawFaceId)
		{
			FreeLance.Models.ContractModels contract =
				db.ContractModels.Include(c => c.LawFace).Single(c => c.ContractId == contractId);
			LawFace lawFace = db.LawFaces.Single(l => l.Id == lawFaceId);
			contract.LawFace = lawFace;
			db.SaveChanges();
		}

		[HttpPost]
		public void ChangeLawFaceEmployer(string employerId, int lawFaceId)
		{
			ApplicationUser employer = getApplicationUsersInRole("Employer").Single(x => x.Id == employerId);
			LawFace lawFace = db.LawFaces.Single(l => l.Id == lawFaceId);
			employer.LawFace = lawFace;
			db.SaveChanges();
		}

		public ActionResult ToggleApprove(string id, string nextAction)
		{
			ApplicationUser user = db.Users.Where(u => u.Id == id).ToList()[0];
			if (user.IsApprovedByCoordinator.HasValue)
			{
				user.IsApprovedByCoordinator = !user.IsApprovedByCoordinator;
			}
			else
			{
				user.IsApprovedByCoordinator = true;
			}
			db.SaveChanges();
			return RedirectToAction(nextAction);
		}

		[HttpGet]
		public ActionResult UnverifiedContracts()
		{
			ViewBag.LawFaceChooseView = new LawModelsManager.LawFaceChooseView();
			return View(db.ContractModels.Where(x => x.IsApprovedByCoordinator == false).ToList());
		}

		[HttpGet]
		public ActionResult UnpaidContracts()
		{
			ViewBag.LawFaceChooseView = new LawModelsManager.LawFaceChooseView();
			return View(db.ContractModels.Where(x => x.Status == ContractStatus.ClosedNotPaid).ToList());
		}

		[HttpGet]
		public ActionResult UnverifiedDocuments()
		{
			return View(db.DocumentPackageModels.Where(x => x.IsApproved == null).ToList());
		}

		[HttpGet]
		public ActionResult UnverifiedFeedback()
		{
			return View(db.ContractModels.Where(x => x.Comment != null).ToList());
		}
	}
}

