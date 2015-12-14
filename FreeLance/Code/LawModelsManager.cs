using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FreeLance.Models;

namespace FreeLance.Code
{
    public class LawModelsManager
    {

        public class ContractView
        {
            public ContractModels Contract { get; set; }
            public List<LawFace> LawFaces { get; set; } 
        }

        //For DropDownList LawFace Choose
        //Using in Coordinator.Home and Employer.Details
        public class LawFaceChooseView
        {
            public virtual List<LawFace> LawFaces { get; set; }

            public LawFaceChooseView()
            {
                ApplicationDbContext db = new ApplicationDbContext();
                LawFaces = db.LawFaces.ToList();
                //For null value in field LawFace
                //If this selected, nothing is changed
                LawFace noLawFace = new LawFace()
                {
                    Id = -1,
                    Name = "Ничего не выбрано",
                };
                LawFaces.Add(noLawFace);
            }
        }
		
		//		public class FillLawContractTemplateVR {
		//			public class LawFaceVR {
		//				public string Name { get; set; }
		//				public int Id { get; set; }
		//				public IEnumerable<LawContractTemplate> Templates { get; set; }
		//			}
		//			public IEnumerable<LawFaceVR> LawFaces;
		//			public class FreelancerVR {
		//				public string FIO { get; set; }
		//				public string Id { get; set; }
		//			}
		//			public IEnumerable<FreelancerVR> Freelancers;
		//		}

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

		//		// TODO: lawFaceId is assosiated with Problem or Employer
		//		// TODO: improve ugly url
		//		public ActionResult FillLawContractTemplateAndDownload(string freelancerId, int? lawFaceId=0)
		//		{
		//			ApplicationUser freelancer = db.Users.Find(freelancerId);
		//			// заглушка, TODO
		//			LawContractTemplate lawContractTemplate = db.LawContractTemplates.First();
		//			var freelancerRole = db.Roles.Where(role => role.Name == "Freelancer").ToArray()[0];
		//			if (freelancer == null || lawContractTemplate == null
		//				|| freelancer.Roles.Where(x => x.RoleId != freelancerRole.Id).Any())
		//			{
		//				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
		//			}
		//
		//			string pathToContract = Code.DocumentManager.fillContractTemplate(freelancer, lawContractTemplate);
		//			return ViewFile(pathToContract);
		//		}

		//		// Координатор скачивает загруженный им же заполненный и подписанный ГПХ.
		//		public ActionResult DownloadSignedLawContract(string lawContractPath)
		//		{
		//			return ViewFile(lawContractPath);
		//		}		
	}
}