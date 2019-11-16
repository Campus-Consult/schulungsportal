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
    /// <summary>
    /// Das AnmeldungViewModel beinhaltet alle Attribute einer Anmeldung, sowie eine Liste für ein Dropdown-Menü für CC-Stati und eine Liste von Checkboxen für alle aktuellen Schulungen
    /// Diese Klasse wird von Views zur Erstellung von Eingabemasken für Anmeldungen genutzt.
    /// </summary>
    public class AnmeldungViewModel
    {
        public static readonly List<string> AllStati = new List<string>
            {
                "Trainee",
                "Junior Mitglied",
                "Mitglied",
                "Senior Mitglied",
                "Alumnus/Alumna",
                "Sonstiges"
            };

        /// <summary>
        /// leerer Konstruktor
        /// </summary>
        public AnmeldungViewModel()
        {

        }

        /// <summary>
        /// Konstruktor, dem eine Liste von Schulungen übergeben wird.
        /// Aus den Schulungen werden die Checkboxen erstellt, des Weiteren wird das Dropdownmenü mit Inhalt gefüllt.
        /// </summary>
        /// <param name="schulungen"></param>
        public AnmeldungViewModel(IEnumerable<Schulung> schulungen)
        {
            this.Schulungen = schulungen as List<Schulung>;

            // dem Viewmodel die Stati als SelectListItems für das Dropdown-Menü übergeben
            Stati = GetSelectListItems(AllStati); 

            // Liste mit Checkboxen für die Schulungen erstellen
            SchulungsCheckboxen = new List<SchulungsCheckBox>();
            foreach (Schulung schulung in Schulungen)
            {
                SchulungsCheckBox checkbox = new SchulungsCheckBox(schulung.Titel + " am " + schulung.Termine.Min(x => x.Start).ToString("dd.MM.yyyy HH:mm") + " Uhr", schulung.SchulungGUID);
                SchulungsCheckboxen.Add(checkbox);
            }
        }

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

        [Display(Name = "Handynummer")]
        public String Nummer { get; set; }

        [Required]
        [Display(Name = "Status bei CC")]
        public String Status { get; set; }
        
        public IEnumerable<SelectListItem> Stati { get; set; }
        
        [Required]
        [Display(Name = "Schulungen")]
        public List<SchulungsCheckBox> SchulungsCheckboxen { get; set; }

        public List<Schulung> Schulungen { get; set; }

        private static MapperConfiguration config = new MapperConfiguration(cfg => cfg.CreateMap<AnmeldungViewModel, Anmeldung>());
        private static IMapper mapper = config.CreateMapper();

        /// <summary>
        /// Diese Methode gibt den Inhalt des AnmeldungViewModel-Objekts konvertiert in ein Anmeldung-Objekt zurück.
        /// </summary>
        /// <returns> Anmeldung-Objekt mit Inhalt des AnmeldungViewModel-Objekts </returns>
        public Anmeldung ToAnmeldung()
        {
            Anmeldung result = mapper.Map<Anmeldung>(this);

            // don't use null, use empty string
            if (result.Nummer == null) {
                result.Nummer = "";
            }

            return result;
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

    /// <summary>
    /// Diese Klasse enthält Attribute einer Checkbox für eine Schulung
    /// </summary>
    public class SchulungsCheckBox
    {
        public SchulungsCheckBox() : this(false, null, null) {}

        public SchulungsCheckBox(string Titel, string Guid) : this(false, Titel, Guid) {}

        public SchulungsCheckBox(bool Checked, string Titel, string Guid)
        {
            this.Titel = Titel;
            this.Guid = Guid;
            this.Checked = Checked;
        }

        public string Titel { get; set; }
        public string Guid { get; set; }
        public bool Checked { get; set; }
    }
}
