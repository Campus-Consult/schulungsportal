using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Schulungsportal_2.ViewModels
{
    public class TeilnehmerSucheResultViewModel
    {
        public string AnmeldungIDs { get; set; }
        public string VornameTeilnehmer { get; set; }
        public string NachnameTeilnehmer { get; set; }
        public string EmailTeilnehmer { get; set; }
        public string TelefonTeilnehmer { get; set; }
        public int matchCount { get; set; }
        public int number { get; set; }
        public bool Checked { get; set; }
    }
}