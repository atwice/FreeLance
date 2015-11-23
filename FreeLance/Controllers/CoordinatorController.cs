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
using Microsoft.Ajax.Utilities;

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
		}
		
		public class LawFaceView
		{
		    public LawFace LawFace { get; set; }
            public List<LawContractTemplate> LawContractTemplates { get; set; }
        }

	    public class LawFacesViewModel
	    {
	        public List<LawFaceView> LawFaceViews { get; set; }
        }

	    public class LawContractTemplateView
	    {
	        public LawFace LawFace;
	        public string LawFaceId { get; set; }
            [Required]
            public HttpPostedFileBase File { get; set; }
            [Required]
            public string Name { get; set; }
	    }

		// GET: Coordinator
		public ActionResult Index()
		{
			return RedirectToAction("Home");
		}

		public class UploadPostModel
		{
			public HttpPostedFileBase File { get; set; }
			public string UserId { get; set; }
			public int LawContractTemplateId { get; set; }
			//public int LawFaceId { get; set; }
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
		}

		public class UploadSignedLawContractModel
		{
			public List<ApplicationUser> Freelancers { get; set; }
			public List<LawContractTemplate> LawContractTemplates { get; set; }
			//public List<LawFace> LawFaces { get; set; }
			public UploadPostModel PostModel { get; set; }
		}

		// Координатор загружает подписанный ГПХ
		public ActionResult UploadSignedLawContract()
		{
			var model = new UploadSignedLawContractModel();
			model.LawContractTemplates = db.LawContractTemplates.ToList();
			model.Freelancers = getApplicationUsersInRole("Freelancer").OrderBy(f => f.FIO).ToList();
			//model.LawFaces = db.LawFaces.ToList();
			model.PostModel = new UploadPostModel();
			return View(model);
		}

		[HttpPost]
		public ActionResult UploadSignedLawContract([Bind(Prefix = "UploadPostModel")]UploadPostModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.File.ContentLength > 0)
				{
					string path = saveLawContract(model.File);
					addLawContractInDb(path, model.UserId, model.LawContractTemplateId, model.StartDate, model.EndDate);

				}
			}
			return RedirectToAction("Home");
		}

		private void addLawContractInDb(string Path, string UserId, int LawContractTemplateId, DateTime startDate, DateTime EndDate)
		{
			LawContract contract = new LawContract();
			contract.Path = Path;
			contract.StartDate = startDate;
			contract.EndDate = EndDate;
			contract.User = db.Users.Find(UserId);
			contract.LawContractTemplate = db.LawContractTemplates.Find(LawContractTemplateId);
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

		public ActionResult Home()
		{
			var model = new HomeViewModel();
		    model.ContractsList = db.ContractModels.Where(x => x.IsApprovedByCoordinator == false).ToList();
		    model.ContractsToPay = db.ContractModels.Where(x => x.Status == ContractStatus.ClosedNotPaid).ToList();
		    model.NewEmployers = getApplicationUsersApproved(null, "Employer").ToList();
		    model.NewFreelancers = getApplicationUsersApproved(null, "Freelancer").ToList();
		    model.ContractWithComments = db.ContractModels.Where(x => x.Comment != null).ToList();
		    model.DocumetsUnapproved = db.DocumentPackageModels.Where(x => x.IsApproved == null).ToList();
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

		public ActionResult LawFaces()
		{
			var model = new LawFacesViewModel();
			model.LawFaceViews = new List<LawFaceView>();
		    var lawFaces = db.LawFaces.ToList();
            foreach (var lawFace in lawFaces)
		    {
                LawFaceView lawFaceView = new LawFaceView
                {
                    LawFace = lawFace,
                    LawContractTemplates = db.LawContractTemplates.Where(x => x.LawFace.Id == lawFace.Id).ToList() 
                    
                };
                model.LawFaceViews.Add(lawFaceView);
		        
		    }
			return View(model);
		}

	    [HttpGet]
	    public ActionResult AddLawContractTemplate(int lawFaceId)
	    {
            LawContractTemplateView model = new LawContractTemplateView();
	        model.LawFace = db.LawFaces.Where(x => x.Id == lawFaceId).ToList()[0];
	        return View(model);
	    }
   
        [HttpPost]
	    public ActionResult AddLawContractTemplate([Bind(Prefix = "LawContractTemplateView")]LawContractTemplateView lawContractTemplateView)
	    {
	        if (lawContractTemplateView.File == null || lawContractTemplateView.Name == null || lawContractTemplateView.LawFaceId == null)
	        {
	            return RedirectToAction("AddLawContractTemplate", new {lawFaceId = lawContractTemplateView.LawFaceId});
	        }
            int lawFaceId = Int32.Parse(lawContractTemplateView.LawFaceId);
            lawContractTemplateView.LawFace =
                db.LawFaces.Where(x => x.Id == lawFaceId).ToList()[0];
	        LawContractTemplate lawContractTemplate = new LawContractTemplate
	        {
	            LawFace = lawContractTemplateView.LawFace,
	            Name = lawContractTemplateView.Name,
	            Path = saveLawContractTemplate(lawContractTemplateView)
	        };
            
	        db.LawContractTemplates.Add(lawContractTemplate);
            db.SaveChanges();
	        return RedirectToAction("LawFaces");
	    }

        private string saveLawContractTemplate(LawContractTemplateView lawContractTemplateView)
        {
            string path = null;
            var fileName = lawContractTemplateView.LawFace.Name + "_" + lawContractTemplateView.Name + ".docx";
            path = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\LawContractTemplates\\" + fileName;
            Response.Write(path.ToString());
            lawContractTemplateView.File.SaveAs(path);
            return path;
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

		public ActionResult Employers(String searchString, String sortOrder)
		{
			List<string> Ids = AccountController.GetApplicationUsersInRole(db, "Employer")
				.Select( u => u.Id )
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
            ContractModels contract = db.ContractModels.Include(t => t.LawFace).Include(t => t.Problem).Include(t => t.Freelancer).Single(t => t.ContractId == contractIdNum);
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

		public class FillLawContractTemplateVR {
			public class LawFaceVR {
				public string Name { get; set; }
				public int Id { get; set; }
				public IEnumerable<LawContractTemplate> Templates { get; set; }
			}
			public IEnumerable<LawFaceVR> LawFaces;
			public class FreelancerVR {
				public string FIO { get; set; }
				public string Id { get; set; }
			}
			public IEnumerable<FreelancerVR> Freelancers;
		}

		/* seems to be unuseful
		public ActionResult FillLawContractTemplate(string freelancerId) {
			List<FillLawContractTemplateVR.LawFaceVR> lawFaces = new List<FillLawContractTemplateVR.LawFaceVR>();
			foreach (var lawFace in db.LawFaces.ToList()) {
				var templates = db.LawContractTemplates.Where(x => x.LawFace.Id == lawFace.Id  && x.Active);
				if (templates.Any()) {
					lawFaces.Add(new FillLawContractTemplateVR.LawFaceVR {
						Name = lawFace.Name,
						Id = lawFace.Id,
						Templates = templates.ToList()
					});
				}
			}
			List<FillLawContractTemplateVR.FreelancerVR> freelancers = getApplicationUsersInRole("Freelancer")
				.Where(x => (freelancerId == null || freelancerId == x.Id))
				.Select(x => new FillLawContractTemplateVR.FreelancerVR {
					FIO = x.FIO,
					Id = x.Id
				}).ToList();


			if (Request.IsAjaxRequest())
				return PartialView(new FillLawContractTemplateVR { LawFaces = lawFaces, Freelancers = freelancers });
			return View(new FillLawContractTemplateVR { LawFaces = lawFaces, Freelancers = freelancers });
		}

		[HttpPost] 
		public ActionResult FillLawContractTemplate(string employerId, int? lawContractTemplateId) {
			ApplicationUser employer = db.Users.Find(employerId);
			LawContractTemplate lawContractTemplate = db.LawContractTemplates.Find(lawContractTemplateId);
			var employerRole = db.Roles.Where(role => role.Name == "Employer").ToArray()[0];
			if (employer == null || lawContractTemplate == null 
				|| employer.Roles.Where(x => x.RoleId == employerRole.Id).Any()) {
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			string pathToContract = Code.DocumentManager.fillContractTemplate(employer, lawContractTemplate);
		    return ViewFile(pathToContract);
		}
		*/

		// TODO: lawFaceId is assosiated with Problem or Employer
		// TODO: improve ugly url
		public ActionResult FillLawContractTemplateAndDownload(string freelancerId, int? lawFaceId=0)
		{
			ApplicationUser freelancer = db.Users.Find(freelancerId);
			// заглушка, TODO
			LawContractTemplate lawContractTemplate = db.LawContractTemplates.Where(t => /*t.LawFace.Id == lawFaceId &&*/ t.Active).First();
			var freelancerRole = db.Roles.Where(role => role.Name == "Freelancer").ToArray()[0];
			if (freelancer == null || lawContractTemplate == null
				|| freelancer.Roles.Where(x => x.RoleId != freelancerRole.Id).Any())
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			string pathToContract = Code.DocumentManager.fillContractTemplate(freelancer, lawContractTemplate);
			return ViewFile(pathToContract);
		}

		// Координатор скачивает загруженный им же заполненный и подписанный ГПХ.
		public ActionResult DownloadSignedLawContract(string lawContractPath)
		{
			return ViewFile(lawContractPath);
		}

		public ActionResult ViewFile(string path)
	    {
            byte[] filedata = System.IO.File.ReadAllBytes(path);
            string contentType = MimeMapping.GetMimeMapping(path);
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = path,
                Inline = true,
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(filedata, contentType);
        }

        [HttpGet]
	    public ActionResult LawFace()
	    {
            LawFace model = new LawFace();
	        return View(model);
	    }

        [HttpPost]
	    public ActionResult LawFace(LawFace lawFace)
        {
            db.LawFaces.Add(lawFace);
            db.SaveChanges();
            return RedirectToAction("LawFaces");
        }

        [Authorize(Roles="Coordinator")]
	    public ActionResult ToggleActiveLawContractTemplate(int templateId)
	    {
	        LawContractTemplate template = db.LawContractTemplates.Include( t => t.LawFace ).Single( t => t.Id == templateId );
            if (template == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
	        template.Active = !template.Active;
            db.SaveChanges();
	        return RedirectToAction("LawFaces");
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