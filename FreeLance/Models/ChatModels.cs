using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.EnterpriseServices;

namespace FreeLance.Models {
	public class Chat {
		[Key]
		public int Id { get; set; }
	}

	public class ChatMessage {
		[Key]
		public int Id { get; set; }
		public int? ParentId { get; set; }
		[Required]
		public int ChatId { get; set; }
		[Required]
		public string Content { get; set; }
		[Required]
		public virtual ApplicationUser User { get; set; }
		[Required]
		[DataType(DataType.DateTime)]
		public DateTime CreationDate { get; set; }
		[DataType(DataType.DateTime)]
		public DateTime? ModificationDate { get; set; }
	}

}