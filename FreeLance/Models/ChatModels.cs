using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.EnterpriseServices;

namespace FreeLance.Models {

	public enum ChatOwner {
		Problem, Contract
	}

	public class Chat {
		[Key]
		public int Id { get; set; }
		[Required]
		public ChatOwner Owner { get; set; }
	}

	public class ChatMessage {
		[Key]
		public int Id { get; set; }
		public int? ParentId { get; set; }
		[Required]
		public int ChatId { get; set; }
		[Required(AllowEmptyStrings = true)]
		public string Content { get; set; }
		[Required]
		public virtual ApplicationUser User { get; set; }
		[Required]
		[DataType(DataType.DateTime)]
		public DateTime CreationDate { get; set; }
		[DataType(DataType.DateTime)]
		public DateTime? ModificationDate { get; set; }
	}

	public class ProblemChat {
		[Key]
		public int Id { get; set; }
		[Required]
		public Chat Chat { get; set; }
		[Required]
		public virtual ProblemModels Problem { get; set; }
	}

	public class ContractChat {
		[Key]
		public int Id { get; set; }
		[Required]
		public Chat Chat { get; set; }
		[Required]
		public virtual ContractModels Contract { get; set; }
	}

	public class ChatUserStatistic {
		[Key]
		public int Id { get; set; }
		[Required]
		public virtual ApplicationUser User { get; set; }
		[Required]
		public int ChatId { get; set; }
		[Required]
		public DateTime LastVisit { get; set; }
	}

}