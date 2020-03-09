using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Schulungsportal_2.Models.Anmeldungen;
using System.ComponentModel.DataAnnotations.Schema;
using Schulungsportal_2.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Schulungsportal_2.Models.Schulungen
{
    /// <summary>
    /// Das Schulung Model beinhaltet alle Attribute einer Schulung.
    /// Diese Klasse wird von Views zur Darstellung von Schulungen genutzt.
    /// Objekte dieser Klasse sind zum Speichern in der Datenbank vorgesehen.
    /// </summary>
    public class Schulung
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public string SchulungGUID { get; set; }

        [Required]
        [Display(Name = "Titel")]
        public String Titel { get; set; }

        [Required]
        [Display(Name = "Organisator")]
        public String OrganisatorInstitution { get; set; }

        public ICollection<Dozent> Dozenten { get; set; }

        [Required]
        public String Beschreibung { get; set; }

        [Required]
        public String Ort { get; set; }
        
        [Required]
        [Display(Name = "Anmeldefrist")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Anmeldefrist { get; set; }

        public ICollection<Termin> Termine { get; set; }

        public ICollection<Anmeldung> Anmeldungen { get; set; }

        [NotMapped]
        public bool Check = false;

        [HiddenInput]
        public bool IsAbgesagt { get; set; }

        [HiddenInput]
        public bool IsGeprüft { get; set; }

        [HiddenInput]
        public bool GeprüftReminderSent { get; set; }

        [Required]
        [MaxLength(40)]
        public string AccessToken { get; set; }

        /// <summary>
        /// Übersetzt das Model in ein CreateView Model zum bearbeiten
        /// </summary>
        /// <returns>Ein SchulungCreateViewModel</returns>
        public SchulungCreateViewModel ToSchulungCreateViewModel(IMapper mapper)
        {
            SchulungCreateViewModel result = mapper.Map<SchulungCreateViewModel>(this);

            result.TermineVM = Termine.Select(x => x.ToTerminViewModel()).ToList();

            result.AnmeldefristDate = Anmeldefrist.Date;
            result.AnmeldefristTime = Anmeldefrist.TimeOfDay;

            return result;
        }

        
    }

    public class Dozent
    {
        public string SchulungGUID { get; set; }

        public Schulung Schulung { get; set; }
        
        [Required]
        [Display(Name = "Name des Dozenten")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Handynummer des Dozenten")]
        public String Nummer { get; set; }

        [Required]
        [Display(Name = "E-Mail-Adresse des Dozenten")]
        [DataType(DataType.EmailAddress)]
        public string EMail { get; set; }

    }

    public class Termin
    {
        public string SchulungGUID { get; set; }

        public Schulung Schulung { get; set; }

        [Display(Name = "Start-Termin")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Start { get; set; }

        [Display(Name = "End-Termin")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime End { get; set; }

        ///
        /// Formatiert das Datum zu "11.11.2011 11:11 Uhr bis 13:11 Uhr" bzw
        /// 11.11.2011 20:11 Uhr bis 12.11.2011 01:11 Uhr
        ///
        public String FormatFromStartToEnd() {
            if (Start.Date.Equals(End.Date)) {
                return $"{Start.ToString("dd.MM.yyyy HH:mm")} Uhr bis {End.ToString("HH:mm")} Uhr";
            } else {
                return $"{Start.ToString("dd.MM.yyyy HH:mm")} Uhr bis {End.ToString("dd.MM.yyyy HH:mm")} Uhr";
            }
        }

        public TerminViewModel ToTerminViewModel()
        {
            return new TerminViewModel { EndDate = End.Date, EndTime = End.TimeOfDay, StartDate = Start.Date, StartTime = Start.TimeOfDay };
        }
    }
}
