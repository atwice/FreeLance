using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FreeLance.Models
{
	public enum ProblemStatus
	{
		Opened, InProgress, Closed
	}

	public class ProblemModels
	{
		[Key]
		public int ProblemId { get; set; }
		[Required]
		public virtual ApplicationUser Employer { get; set; }
		[Required]
		public string Name { get; set; }
		[Required]
		public string Description { get; set; }
		public decimal Cost { get; set; }
		[Required]
		public ProblemStatus Status { get; set; }
		public virtual ICollection<ContractModels> Contracts { get; set; }
		public virtual ICollection<SubscriptionModels> Subscriptions { get; set; }
	}
}