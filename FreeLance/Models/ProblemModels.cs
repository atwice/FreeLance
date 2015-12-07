using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FreeLance.Models
{
	public enum ProblemStatus
	{
		Opened, Closed
	}

	public class ProblemModels
	{
		[Key]
		public int ProblemId { get; set; }
		[DataType(DataType.Date)]
		public DateTime CreationDate { get; set; }
		[DataType(DataType.Date)]
		public DateTime DeadlineDate { get; set; }
		[Required]
		public virtual ApplicationUser Employer { get; set; }
		[Required]
		public string Name { get; set; }
		[Required, DataType(DataType.MultilineText)]
		[AllowHtml]
		public string Description { get; set; }
		[Required, DataType(DataType.MultilineText)]
		public string SmallDescription { get; set; }

		[Range(0, 1000000)]
		[DataType(DataType.Currency)]
		public decimal Cost { get; set; }

		[Range(0, 1000000)]
		[Required]
		public int AmountOfWorkes { get; set; }

		[Required]
		public ProblemStatus Status { get; set; }
		public virtual ICollection<ContractModels> Contracts { get; set; }
		public virtual ICollection<SubscriptionModels> Subscriptions { get; set; }
	}
}