using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FreeLance.Models {
	public class DocumentPackageModels {

		[Key]
		public int Id { get; set; }
		[Required]
		public virtual ApplicationUser Freelancer { get; set; }
		public bool? IsApproved { get; set; }
		public string FilePassportFace { get; set; }
		public string FilePassportRegistration { get; set; }
		public string Adress { get; set; }
		public string Phone { get; set; }
		public string PaymentDetails { get; set; }
	}
}