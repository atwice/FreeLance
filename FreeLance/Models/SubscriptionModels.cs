using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreeLance.Models
{
	public class SubscriptionModels
	{
		[Key]
		public int SubscriptionId { get; set; }
		[Index("IX_data", 1, IsUnique = true)]
		public virtual ApplicationUser Freelancer { get; set; }
		[Index("IX_data", 2, IsUnique = true)]
		public virtual ProblemModels Problem { get; set; }
	}
}