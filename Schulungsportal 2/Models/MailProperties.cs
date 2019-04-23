using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Schulungsportal_2.Models
{
    /// <summary>
    /// Das MailProperties-Model beinhaltet die Einstellungen des Mailservers.
    /// Die Klasse wird von Views genutzt, um die Einstellungen darzustellen.
    /// Es gibt immer nur ein Objekt dieser Klasse in der Datenbank.
    /// </summary>
    public class MailProperties
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int id { get; set; } // Datenbank ID

        [Required]
        [Display(Name = "Mailserver-Adresse")]
        public string mailserver { get; set; }  // Mailserver-Adresse

        [Required]
        [Display(Name = "Mailserver-Port")]
        public int port { get; set; }           // Ausgangsport des Mailservers

        [Required]
        [Display(Name = "SSL verschlüsselt? ")]
        public bool useSsl { get; set; }        // wird SSL-Verschlüsselung genutzt?

        [Required]
        [Display(Name = "Mailadresse Schulungsportal")]
        public string absender { get; set; }    // Mailadresse des Schulungsportals

        [Required]
        [Display(Name = "Passwort")]
        [DataType(DataType.Password)]
        public string passwort { get; set; }    // Passwort des Mailaccounts des Schulungsportals

        [Required]
        [Display(Name = "Mailadresse Schulungsbeauftragter")]
        public string adresseSchulungsbeauftragter { get; set; }    // Mailadresse des Schulungsbeauftragten
    }
}
