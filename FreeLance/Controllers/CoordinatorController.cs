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

namespace FreeLance.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public class HomeViewModel
        {
            public List<ApplicationUser> Incognitos { get; set; }
			public List<ApplicationUser> WithoutDocuments { get; set; }
		}

        public class FreelancersViewModel
        {
            public List<ApplicationUser> Freelancers { get; set; }
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

        public ActionResult Download()
        {
            string filename = db.LawContractTemplates.ToArray()[1].Path;
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

        [HttpPost]
        public ActionResult AddLawFace(LawFace model)
        {
            db.LawFaces.Add(model);
            db.SaveChanges();
            return View(model);
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
            model.Incognitos = getApplicationUsersInRole("Incognito").ToList();
			model.WithoutDocuments = getApplicationUsersInRole("WithoutDocuments").ToList();
			return View(model);
        }

        public ActionResult Freelancers()
        {
            var model = new FreelancersViewModel();
            model.Freelancers = getApplicationUsersInRole("Freelancer").ToList();
            return View(model);
        }

      
        public ActionResult LawFaces()
        {
            var model =  new LawFacesViewModel();
            model.LawFaces = db.LawFaces.ToList();
            return View(model);
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
		public ActionResult EditRole(string usernameID, string choosenRole)
		{
			ApplicationUser freelancer = db.Users.Find(usernameID);
			var withoutDoc = db.Roles.Where(role => role.Name == choosenRole).ToArray()[0];
			if (freelancer == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			freelancer.Roles.Clear();
			freelancer.Roles.Add(new IdentityUserRole { RoleId = withoutDoc.Id, UserId = freelancer.Id });
			db.SaveChanges();
			return RedirectToAction("Home");
		}

	}
}