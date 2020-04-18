using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Schulungsportal_2.Models;
using Schulungsportal_2.Models.Schulungen;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Schulungsportal_2.ViewModels
{
    /// <summary>
    /// Das SchulungViewModel beinhaltet alle Attribute einer Schulung.
    /// Diese Klasse wird von Views zur Erstellung von Eingabemasken für Schulungen genutzt.
    /// </summary>
    public class SchulungCreateViewModel
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

        public List<Dozent> Dozenten { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public String Beschreibung { get; set; }

        [Required]
        public String Ort { get; set; }

        public List<TerminViewModel> TermineVM { get; set; }

        [Required]
        [Display(Name = "Start Anmeldefrist")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartAnmeldefristDate { get; set; }

        [Required]
        [Display(Name = "Start Anmeldefrist")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan StartAnmeldefristTime { get; set; }

        [Required]
        [Display(Name = "Anmeldefrist")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AnmeldefristDate { get; set; }

        [Required]
        [Display(Name = "Anmeldefrist")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan AnmeldefristTime { get; set; }

        [HiddenInput]
        public bool IsAbgesagt { get; set; }

        [HiddenInput]
        public bool IsGeprüft { get; set; }

        private static MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<SchulungCreateViewModel, Schulung>());
        private static IMapper mapper = config.CreateMapper();

        public SchulungCreateViewModel()
        {
            TermineVM = new List<TerminViewModel>();
            StartAnmeldefristDate = DateTime.Now.Date;
            StartAnmeldefristTime = DateTime.Now.TimeOfDay;
            TermineVM.Add(new TerminViewModel { EndDate = DateTime.Now.Date, StartDate = DateTime.Now.Date });
            Dozenten = new List<Dozent>();
            Dozenten.Add(new Dozent() {
                Name = "",
                Nummer = "",
                EMail = "",
            });
        }

        /// <summary>
        /// Diese Methode gibt den Inhalt des SchulungViewModel-Objekts konvertiert in ein Schulung-Objekt zurück.
        /// </summary>
        /// <returns> Schulung-Objekt mit Inhalt des SchulungViewModel-Objekts </returns>
        public Schulung ToSchulung()
        {
            Schulung result = mapper.Map<Schulung>(this);

            result.Termine = TermineVM.ConvertAll(x => x.ToTermin(SchulungGUID));
            result.Anmeldefrist = AnmeldefristDate.Add(AnmeldefristTime);
            result.StartAnmeldefrist = StartAnmeldefristDate.Add(StartAnmeldefristTime);
            return result;
        }
    }
}
