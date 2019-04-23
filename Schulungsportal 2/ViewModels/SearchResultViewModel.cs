using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Schulungsportal_2.ViewModels
{
    /// <summary>
    /// Das SearchResultViewModel enthält eine Liste von Schulungen und Metadaten zur Suche
    /// </summary>
    public class SearchResultViewModel
    {
        [Display(Name = "Titel")]
        public String titel { get; set; }

        [Display(Name = "Suchanfrage")]
        public String suchAnfrage { get; set; }

        public IEnumerable<Models.Schulungen.Schulung> schulung { get; set; }
    }
}
