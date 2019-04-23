using Schulungsportal_2.Models.Schulungen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Schulungsportal_2.ViewModels
{
    public class TerminViewModel
    {
        [Required]
        [Display(Name = "Start-Termin")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Start-Termin")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "End-Termin")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "End-Uhrzeit")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan EndTime { get; set; }

        public Termin ToTermin(string SchulungGUID)
        {
            return new Termin { SchulungGUID = SchulungGUID, Start = StartDate.Add(StartTime), End = EndDate.Add(EndTime) };
        }
    }
}