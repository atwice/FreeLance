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
		public virtual ApplicationUser User { get; set; }

		[Required]
		public bool IsApproved { get; set; }

		public string FilePassport { get; set; }
	}
}