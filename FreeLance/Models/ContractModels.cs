using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using FreeLance.Models;

namespace FreeLance.Models
{
	public enum ContractStatus
	{
		Opened, Confirmed, InProgress, Done, Closed
	}

	public class ContractModels
	{
		[Key]
		public int ContractId { get; set; }
		public virtual ApplicationUser Freelancer { get; set; }
		public string Details { get; set; }
		public virtual ProblemModels Problem { get; set; }
		public ContractStatus Status { get; set; }
	}
}