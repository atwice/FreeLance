using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FreeLance.Models
{
    public class EntityModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class CivilLawContract
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public EntityModel Entity;
    }
}