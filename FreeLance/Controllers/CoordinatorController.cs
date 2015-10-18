using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Migrations;
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
		}

		public class FreelancersViewModel
		{
			public List<ApplicationUser> Incognitos { get; set; }
			public List<ApplicationUser> WithoutDocuments { get; set; }
			public List<ApplicationUser> WithDocuments { get; set; }
			public List<ApplicationUser> FreelancersWithLawContract { get; set; }
			public List<ApplicationUser> FreelancersWithoutLawContract { get; set; }
		}

		public class EmployersViewModel
		{
			public List<ApplicationUser> EmployersApproved { get; set; }
			public List<ApplicationUser> EmployersNotApproved { get; set; }
		}

		public class UploadViewModel
		{
			public List<ApplicationUser> Freelancers { get; set; }
			public List<LawContractTemplate> LawContractTemplates { get; set; }
			public UploadPostModel PostModel { get; set; }
		}

		public class UploadPostModel
		{
			public HttpPostedFileBase File { get; set; }
			public string UserId { get; set; }
			public int LawContractTemplateId { get; set; }
			public DateTime EndDate { get; set; }
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



		public ActionResult Upload()
		{
			var model = new UploadViewModel();
			model.LawContractTemplates = db.LawContractTemplates.ToList();
			model.Freelancers = getApplicationUsersInRole("Freelancer").OrderBy(f => f.FIO).ToList();
			model.PostModel = new UploadPostModel();
			return View(model);
		}

		[HttpPost]
		public ActionResult Upload([Bind(Prefix = "UploadPostModel")]UploadPostModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.File.ContentLength > 0)
				{
					string path = SaveLawContract(model.File);
					AddLawContractInDb(path, model.UserId, model.LawContractTemplateId, model.EndDate);

				}
			}
			return RedirectToAction("Index");
		}

		private void AddLawContractInDb(string Path, string UserId, int LawContractTemplateId, DateTime EndDate)
		{
			LawContract contract = new LawContract();
			contract.Path = Path;
			contract.EndData = EndDate;
			contract.User = db.Users.Find(UserId);
			contract.LawContractTemplate = db.LawContractTemplates.Find(LawContractTemplateId);
			db.LawContracts.Add(contract);
			db.SaveChanges();
		}

		private string SaveLawContract(HttpPostedFileBase file)
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
			ViewBag.ManyIncognitos = false;
			model.IncognitosSmallList = getApplicationUsersInRole("Incognito").ToList();
			if (model.IncognitosSmallList.Count() > 3)
			{
				model.IncognitosSmallList = model.IncognitosSmallList.GetRange(0, 3);
				ViewBag.ManyIncognitos = true;
			}
			ViewBag.ManyWithoutDocuments = false;
			model.WithoutDocumentsSmallList = getApplicationUsersApproved(false, "Freelancer").ToList();
			if (model.WithoutDocumentsSmallList.Count() > 3)
			{
				model.WithoutDocumentsSmallList = model.WithoutDocumentsSmallList.GetRange(0, 3);
				ViewBag.ManyWithoutDocuments = true;
			}
			return View(model);
		}

		public ActionResult Freelancers()
		{
			var model = new FreelancersViewModel();
			model.Incognitos = getApplicationUsersInRole("Incognito").ToList();
			model.WithoutDocuments = getApplicationUsersApproved(false, "Freelancer").ToList();
			model.WithDocuments = getApplicationUsersApproved(true, "Freelancer").ToList();
			string userId = User.Identity.GetUserId();
			List<ApplicationUser> freelancers = getApplicationUsersInRole("Freelancer").OrderBy(f => f.FIO).ToList();
			model.FreelancersWithLawContract = new List<ApplicationUser>();
			model.FreelancersWithoutLawContract = new List<ApplicationUser>();
			foreach (var freelancer in freelancers)
			{
				var contracts = db.LawContracts.Where(c => c.User.Id == freelancer.Id).ToArray();
				if (contracts.Count() > 0 && contracts.Last().EndData < DateTime.Now)
				{
					model.FreelancersWithLawContract.Add(freelancer);
				}
				else if (!model.WithoutDocuments.Contains(freelancer))
				{
					model.FreelancersWithoutLawContract.Add(freelancer);
				}
			}
			return View(model);
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
	            Path = SaveLawContractTemplate(lawContractTemplateView)
	        };
            
	        db.LawContractTemplates.Add(lawContractTemplate);
            db.SaveChanges();
	        return RedirectToAction("LawFaces");
	    }

        private string SaveLawContractTemplate(LawContractTemplateView lawContractTemplateView)
        {
            string path = null;
            var fileName = lawContractTemplateView.LawFace.Name + "_" + lawContractTemplateView.Name + ".docx";
            path = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\LawContractTemplates\\" + fileName;
            Response.Write(path.ToString());
            lawContractTemplateView.File.SaveAs(path);
            return path;
        }
		private IEnumerable<ApplicationUser> getApplicationUsersApproved(bool approved, string roleName)
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

		public ActionResult Employers()
		{
			var model = new EmployersViewModel();
			model.EmployersApproved = getApplicationUsersApproved(true, "Employer").ToList();
			model.EmployersNotApproved = getApplicationUsersApproved(false, "Employer").ToList();

			return View(model);
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
				.Where(x => x.IsApprovedByCoordinator && (freelancerId == null || freelancerId == x.Id))
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
			if (employer == null || lawContractTemplate == null || !employer.IsApprovedByCoordinator
				|| employer.Roles.Where(x => x.RoleId == employerRole.Id).Any()) {
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			string pathToContract = Code.DocumentManager.fillContractTemplate(employer, lawContractTemplate);
		    return ViewFile(pathToContract);
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


	    [HttpPost]
        [Authorize(Roles="Coordinator")]
	    public ActionResult ToggleActiveLawContractTemplate(int templateId)
	    {
	        LawContractTemplate template = db.LawContractTemplates.Find(templateId);
            if (template == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
	        template.Active = !template.Active;
	        try
	        {
	            db.SaveChanges();
	        }
	        catch (Exception e)
	        {
	            throw;
	        }
	        return Json(new {templateId = templateId, active = template.Active});
	    }
    }
}