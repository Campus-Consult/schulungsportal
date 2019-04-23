using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schulungsportal_2.ViewModels
{
    /// <summary>
    /// Das ArchivViewModel enthält eine Liste von Schulungen.
    /// Das ViewModel wird von Views verwendet, um eine tabellarische Darstellung aller vergangenen Schulungen anzuzeigen.
    /// </summary>
    public class ArchivViewModel
    {
        public IEnumerable<Models.Schulungen.Schulung> schulung { get; set; }
    }
}
