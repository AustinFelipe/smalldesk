using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmallDesk.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}