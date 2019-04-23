using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Schulungsportal_2.Models.Schulungen;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace Schulungsportal_2.Models.Anmeldungen
{
    /// <summary>
    /// Das Anmeldung Model beinhaltet alle Attribute einer Anmeldung.
    /// Diese Klasse wird von Views zur Darstellung von Anmeldungen verwendet.
    /// Objekte dieser Klasse sind zum Speichern in der Datenbank vorgesehen.
    /// </summary>
    public class Anmeldung
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int anmeldungId { get; set; }
        
        [Required]
        [Display(Name = "Vorname")]
        public String Vorname { get; set; }

        [Required]
        [Display(Name = "Nachname")]
        public String Nachname { get; set; }

        [Required]
        [Display(Name = "E-Mail-Adresse")]
        [DataType(DataType.EmailAddress)]
        public String Email { get; set; }

        [Required]
        [Display(Name = "Handynummer")]
        public String Nummer { get; set; }

        [Required]
        [Display(Name = "Status bei CC")]
        public String Status { get; set; }

        [ForeignKey("Schulung")]
        public String SchulungGuid { get; set; }

        public virtual Schulung Schulung { get; set; }
    }
}
