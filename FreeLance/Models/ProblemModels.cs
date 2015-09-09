using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FreeLance.Models
{
	public class ProblemModels
	{
		[Key]
		public int ProblemId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Type { get; set; }
	}
}