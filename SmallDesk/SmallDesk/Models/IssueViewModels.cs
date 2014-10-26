using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SmallDesk.Models
{
    public class IssueIndexModel
    {
    }

    public class IssueEditModel
    {
        public int Id { get; set; }

        [Display(Name = "Técnico")]
        public string SupportUser_Id { get; set; }

        [Required]
        [Display(Name = "User Reportador")]
        public string UserThatReported_Id { get; set; }

        [Display(Name = "Data Máx.")]
        public DateTime ExpectedAt { get; set; }

        [Required]
        [Display(Name = "Desc. Problema")]
        public string ProblemData { get; set; }

        [Display(Name = "Desc. Solução")]
        public string SolutionData { get; set; }

        [Display(Name = "Resolvido?")]
        public bool IsSolved { get; set; }
    }
}