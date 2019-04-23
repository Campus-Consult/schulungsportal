using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// Dieses ViewModel wird verwendet um das Archiv nach Schulungen zu durchsuchen. Die Attribute sind die möglichen Suchkategorien.
/// </summary>
/// 

namespace Schulungsportal_2.ViewModels
{
    public class SucheViewModel
    {
        public string Titel { get; set; }

        public string VornameDozent { get; set; }
        public string NachnameDozent { get; set; }

        public string VornameTeilnehmer { get; set; }
        public string NachnameTeilnehmer { get; set; }

        public string Jahr { get; set; }
    }
}
