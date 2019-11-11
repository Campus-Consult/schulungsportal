using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Schulungsportal_2.Models.Anmeldungen;
using Schulungsportal_2.Models.Schulungen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Schulungsportal_2.ViewModels
{
    public class AbmeldungViewModel
    {
        
        /// <summary>
        /// leerer Konstruktor
        /// </summary>
        public AbmeldungViewModel()
        {

        }

        [HiddenInput(DisplayValue = false)]
        public int anmeldungId { get; set; }

        public Schulung Schulung { get; set; }

        [Display(Name = "Nachricht an den Dozenten")]
        public String Nachricht { get; set; }
    }
}
