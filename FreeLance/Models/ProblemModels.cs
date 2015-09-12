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
		public virtual ApplicationUser Employer { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public ProblemStatus Status { get; set; }
		public virtual ICollection<ContractModels> Contracts { get; set; }
	}
}