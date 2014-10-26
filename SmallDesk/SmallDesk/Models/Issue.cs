using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SmallDesk.Models
{
    public class Issue
    {
        public int Id { get; set; }

        [Display(Name = "Incl. Por")]
        public virtual ApplicationUser UserThatIncluded { get; set; }

        [Required]
        [ForeignKey("UserThatIncluded")]
        [Display(Name = "Incl. Por")]
        public string UserThatIncluded_Id { get; set; }

        [Display(Name = "Técnico")]
        public virtual ApplicationUser SupportUser { get; set; }

        [ForeignKey("SupportUser")]
        [Display(Name = "Técnico")]
        public string SupportUser_Id { get; set; }

        [Display(Name = "User Reportador")]
        public virtual ApplicationUser UserThatReported { get; set; }

        [Required]
        [ForeignKey("UserThatReported")]
        [Display(Name = "User Reportador")]
        public string UserThatReported_Id { get; set; }

        [Required]
        [Display(Name = "Criado Em")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Display(Name = "Data Máx.")]
        public DateTime ExpectedAt { get; set; }

        [Display(Name = "Data Resolução")]
        public DateTime? ResolvedAt { get; set; }

        [Required]
        [Display(Name = "Desc. Problema")]
        public string ProblemData { get; set; }

        [Display(Name = "Desc. Solução")]
        public string SolutionData { get; set; }

        [Display(Name = "Resolvido?")]
        public bool IsSolved { get; set; }
    }
}