using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Schulungsportal_2.Models
{
    /// <summary>
    /// Das Impressum-Model beinhaltet den variablen Inhalt des Impressums.
    /// Die Klasse wird von Views genutzt, um die Inhalte des Impressums darzustellen.
    /// Es gibt immer nur ein Objekt dieser Klasse in der Datenbank.
    /// </summary>
    public class Impressum
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int id { get; set; }
        [Required]
        [Display(Name = "Verantwortungsbereich:")]
        public string Verantwortungsbereich { get; set; }
        [Required]
        [Display(Name = "Dienstanbieter:")]
        public string Dienstanbieter { get; set; }
        [Required]
        [Display(Name = "Vorstand:")]
        public string Vorstand { get; set; }
        [Required]
        [Display(Name = "Journalistische Verantwortung:")]
        public string JournalistischeVerantwortung { get; set; }
        [Required]
        [Display(Name = "Kommunikation:")]
        public string Kommunikation { get; set; }
        [Required]
        [Display(Name = "Anschrift:")]
        public string Anschrift { get; set; }
    }
}
