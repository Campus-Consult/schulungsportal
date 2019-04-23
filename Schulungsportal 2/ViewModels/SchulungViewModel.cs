using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Schulungsportal_2.Models;
using Schulungsportal_2.Models.Schulungen;
using Microsoft.AspNetCore.Mvc;

namespace Schulungsportal_2.ViewModels
{
    /// <summary>
    /// Das SchulungViewModel beinhaltet alle Attribute einer Schulung.
    /// Diese Klasse wird von Views zum Anzeigen von Schulungen genutzt.
    /// </summary>
    public class SchulungViewModel
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public string SchulungGUID { get; set; }
        
        [Display(Name = "Titel")]
        public String Titel { get; set; }
        
        [Display(Name = "Name des Dozenten")]
        public String NameDozent { get; set; }

        [Display(Name = "Organisator")]
        public String OrganisatorInstitution { get; set; }

        [Display(Name = "Handynummer des Dozenten")]
        public String NummerDozent { get; set; }
        
        [Display(Name = "E-Mail-Adresse des Dozenten")]
        [DataType(DataType.EmailAddress)]
        public String EmailDozent { get; set; }
        
        [DataType(DataType.MultilineText)]
        public String Beschreibung { get; set; }
        
        public String Ort { get; set; }
        
        [Display(Name = "Start-Termin")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy hh:mm}")]
        public DateTime Start { get; set; }
        
        [Display(Name = "End-Termin")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy hh:mm}")]
        public DateTime End { get; set; }
        
        [Display(Name = "Anmeldefrist")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public int Anmeldefrist { get; set; }

        [HiddenInput]
        public bool IsAbgesagt { get; set; }

        [HiddenInput]
        public bool IsGeprüft { get; set; }
    }
}
