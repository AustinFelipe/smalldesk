using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmallDesk.Models
{
    public class Issue
    {
        public int Id { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Criado Em")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Display(Name = "Data Máx.")]
        public DateTime ExpectedAt { get; set; }

        [Display(Name = "Data Resolução")]
        public DateTime ResolvedAt { get; set; }

        [Required]
        [Display(Name = "Desc. Problema")]
        public string ProblemData { get; set; }

        [Display(Name = "Desc. Solução")]
        public string SolutionData { get; set; }

        public bool IsSolved { get; set; }
    }
}