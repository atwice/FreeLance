﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FreeLance.Models
{
    public class LawContractTemplate
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Path { get; set; }
    }

    public class LawFace
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public virtual LawContractTemplate CurrentLawContractTemplate { get; set; }
    }

    public class LawContract
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public virtual LawContractTemplate LawContractTemplate { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public virtual ApplicationUser User { get; set; }
		[Required]
		public DateTime EndData { get; set; }
	}
}