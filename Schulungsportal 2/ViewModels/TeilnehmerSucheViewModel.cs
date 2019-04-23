using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Schulungsportal_2.ViewModels
{
    public class TeilnehmerSucheViewModel
    {
        [Display(Name = "Vorname:")]
        public String VornameTeilnehmer { get; set; }
        [Display(Name = "Nachname:")]
        public String NachnameTeilnehmer { get; set; }
        [Display(Name = "Email-Adresse:")]
        public String EmailTeilnehmer { get; set; }
        [Display(Name = "Telefonnummer:")]
        public String TelefonTeinehmer { get; set; }

        /// <summary>
        /// Removes nulls and replaces them with ""
        /// </summary>
        public void CleanNulls()
        {
            if (VornameTeilnehmer == null)
            {
                VornameTeilnehmer = "";
            }
            if (NachnameTeilnehmer == null)
            {
                NachnameTeilnehmer = "";
            }
            if (EmailTeilnehmer == null)
            {
                EmailTeilnehmer = "";
            }
            if (TelefonTeinehmer == null)
            {
                TelefonTeinehmer = "";
            }
        }

    }
}