﻿using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Novacode;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.IO;

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
            public List<ApplicationUser> FreelancersWithLawContract { get; set; }
            public List<ApplicationUser> FreelancersWithoutLawContract { get; set; }
        }

        public class UploadViewModel
        {
            public List<ApplicationUser> Freelancers { get; set; }
            public LawContract contract { get; set; }
        }

        public class LawFacesViewModel
        {
            public List<LawFace> LawFaces { get; set; }
        }

        // GET: Coordinator
        public ActionResult Index()
        {
            return RedirectToAction("Home");
        }

        public ActionResult Download(string filename)
        {
//            string filename = db.LawContractTemplates.ToArray()[1].Path;
            string filepath = AppDomain.CurrentDomain.BaseDirectory + filename;
            using (DocX doc = DocX.Load(filepath))
            {
                doc.ReplaceText("Name", "%%NAME%%");
                doc.Save();
            }

            byte[] filedata = System.IO.File.ReadAllBytes(filepath);
            string contentType = MimeMapping.GetMimeMapping(filepath);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true,
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(filedata, contentType);

         }

        public ActionResult Upload()
        {
            var model = new UploadViewModel();
            model.Freelancers = getApplicationUsersInRole("Freelancer").OrderBy(f => f.FIO).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            string path = null;
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                path = AppDomain.CurrentDomain.BaseDirectory + "Files\\LawContracts\\" + fileName;
                Response.Write(path.ToString());
                file.SaveAs(path);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddLawFace(LawFace model)
        {
            db.LawFaces.Add(model);
            db.SaveChanges();
            return RedirectToAction("LawFaces");
        }

        [HttpGet]
        public ActionResult AddLawFace()
        {
            ViewBag.LawContractTemplates = db.LawContractTemplates.ToList();
            return View(new LawFace());
        }


        [HttpPost]
        public ActionResult AddLawContractTemplate(LawContractTemplate model)
        {
            db.LawContractTemplates.Add(model);
            db.SaveChanges();
            ViewBag.ErrorMessage = "Thank you!";
            ViewBag.LawContractTemplates = db.LawContractTemplates.ToList();
            return View();
        }


        public ActionResult AddLawContractTemplate()
        {
            return View();
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
			model.WithoutDocumentsSmallList = getApplicationUsersApproved(false).ToList();
			if (model.WithoutDocumentsSmallList.Count() > 3)
			{
				model.WithoutDocumentsSmallList = model.WithoutDocumentsSmallList.GetRange(0, 3);
				ViewBag.ManyWithoutDocuments = true;
            }
			return View(model);
        }

        public ActionResult Freelancers()
        {
            string userId = User.Identity.GetUserId();
            var model = new FreelancersViewModel();
			List<ApplicationUser> freelancers = getApplicationUsersInRole("Freelancer").OrderBy(f => f.FIO).ToList();
			model.FreelancersWithLawContract = new List<ApplicationUser>();
			model.FreelancersWithoutLawContract = new List<ApplicationUser>();
			foreach (var freelancer in freelancers)
			{
				var contracts = db.LawContracts.Where(c => c.User.Id == freelancer.Id);
                if (contracts.Count() > 0 && contracts.Last().EndData < DateTime.Now)
				{
					model.FreelancersWithLawContract.Add(freelancer);
				} 
				else
				{
					model.FreelancersWithoutLawContract.Add(freelancer);
				}
			}			
			return View(model);
        }

      
        public ActionResult LawFaces()
        {
            var model =  new LawFacesViewModel();
            model.LawFaces = db.LawFaces.ToList();
            return View(model);
        }

	    private IEnumerable<ApplicationUser> getApplicationUsersApproved(bool approved)
	    {
		    return db.Users.Where(u => u.IsApprovedByCoordinator == approved);
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

		public class EmployersVR {
			public ApplicationUser Employer { get; set; }
			public class ApprovationForm {
				public string ButtonText { get; set; }
				public string IsApproved { get; set; }
				public string Redirect { get; set; }
				public string EmployerId { get; set; }
			}
			public ApprovationForm Form { get; set; }
        }

		public ActionResult Employers() {
			return View(
				Enumerable.Select(AccountController.GetApplicationUsersInRole(db, "employer"),
					employer => new EmployersVR {
						Employer = employer,
						Form = new EmployersVR.ApprovationForm {
							ButtonText = !employer.IsApprovedByCoordinator ? "Подтвердить" : "Отменить подтверждение",
							IsApproved = (!employer.IsApprovedByCoordinator).ToString(),
							Redirect = "/Coordinator/Employers",
							EmployerId = employer.Id
						}
					}
				).OrderBy(data => data.Employer.IsApprovedByCoordinator)
			);
		}

		[HttpPost]
		public ActionResult ChangeEmployerApprovalStatus(string employerId, bool isApproved, string redirect) {
			ApplicationUser employer = db.Users.Find(employerId);
			var employerRole = db.Roles.Where(role => role.Name == "employer").ToArray()[0];
			
			if (employer == null || employer.Roles.Where(role => role.RoleId == employerRole.Id).Count() == 0) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			employer.IsApprovedByCoordinator = isApproved;
			db.SaveChanges();
			return Redirect(redirect == null ? "/Coordinator/Home" : redirect);
		}

		[HttpPost]
		public ActionResult IncognitoToFreelancer(string usernameID)
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
			return RedirectToAction("Home");
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

	}
}