using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using FreeLance.Models;
using System.Web.Mvc;

namespace FreeLance.Models
{
	public enum ContractStatus
	{
		Opened, InProgress, Done, Failed, СancelledByFreelancer, СancelledByEmployer, Closed, ClosedNotPaid
	}

	public class ContractModels
	{
		[Key]
		public int ContractId { get; set; }
		[DataType(DataType.Date)]
		public DateTime CreationDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime DeadlineDate { get; set; }
        [DataType(DataType.Date)]
		public DateTime EndingDate { get; set; }
		[Required]
		public virtual ApplicationUser Freelancer { get; set; }
		[DataType(DataType.MultilineText)]
		[AllowHtml]
		public string Details { get; set; }
		[Required]
		public virtual ProblemModels Problem { get; set; }
		[Required]
		public ContractStatus Status { get; set; }
		[Range(0, 1000000)]
		[DataType(DataType.Currency)]
		public decimal Cost { get; set; }
		public decimal Rate { get; set; }
		public string Comment { get; set; }
        public virtual LawFace LawFace { get; set; }
        public bool IsApprovedByCoordinator { get; set; }
        public bool IsPayed { get; set; }
		public bool IsHidden { get; set; } = false;
	}
}