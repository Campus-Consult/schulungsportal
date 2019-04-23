using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Schulungsportal_2.Models.Anmeldungen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Dieses ViewModel wird verwendet um Anmeldungen zu Schulungen nachzutragen. Es beinhaltet alle Informationen die zu einer Anmeldung gehoeren.
/// </summary>

namespace Schulungsportal_2.ViewModels
{
    public class AnmeldungNachtragenViewModel
    {
        public AnmeldungNachtragenViewModel()
        {
            // dem Viewmodel die Stati als SelectListItems für das Dropdown-Menü übergeben
            Stati = GetSelectListItems(GetAllStati()); 
        }
        

        [Key]
        [HiddenInput(DisplayValue = false)]
        public int anmeldungId { get; set; }
        
        public String SchulungGuid { get; set; }

        public String SchulungTitel { get; set; }

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
        
        public IEnumerable<SelectListItem> Stati { get; set; }


        private static MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<AnmeldungNachtragenViewModel, Anmeldung>());
        private static IMapper mapper = config.CreateMapper();


        public Anmeldung ToAnmeldung()
        {
            Anmeldung result = mapper.Map<Anmeldung>(this);

            return result;
        }

        /// <summary>
        /// Diese Methode gibt eine Liste von Strings mit den Bezeichnungen aller CC-Stati zurück
        /// </summary>
        /// <returns> Liste von Strings mit den Bezeichnungen aller CC-Stati </returns>
        private IEnumerable<string> GetAllStati()
        {
            return new List<string>
            {
                "Mentee",
                "Trainee",
                "Junior Mitglied",
                "Mitglied",
                "Senior Mitglied",
                "Alumnus",
                "Sonstiges"
            };
        }

        /// <summary>
        /// Diese Methode erzeugt aus einer übergebenen Liste von Strings eine Liste von SelectListItems, die zum Erzeugen eines Dropdown-Menüs genutzt werden kann.
        /// </summary>
        /// <param name="elements"> SelectListItem-Liste </param>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<string> elements)
        {
            // Erstellt eine neue Liste
            var selectList = new List<SelectListItem>();

            // Für jeden String in der "elements"-Liste, wird ein neues SelectListItem-Objekt erstellt
            foreach (var element in elements)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element,
                    Text = element
                });
            }

            return selectList;
        }
    }

}
