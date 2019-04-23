using Schulungsportal_2.Models.Anmeldungen;
using Schulungsportal_2.Models.Schulungen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schulungsportal_2.ViewModels
{
    /// <summary>
    /// Das TeilnehmerlisteViewModel beinhaltet eine Schulung und eine Liste von Anmeldungen zu dieser.
    /// Diese Klasse wird von Views zur Erstellung von Teilnehmerlisten genutzt.
    /// </summary>
    public class TeilnehmerlisteViewModel
    {
        public Schulung Schulung { get; set; }
        public IEnumerable<Anmeldung> Anmeldungen { get; set; }
        public String RundmailLink { get; set; }
    }
}
