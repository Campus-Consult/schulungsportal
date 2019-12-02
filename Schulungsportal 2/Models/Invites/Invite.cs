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

namespace Schulungsportal_2.Models
{
    /// <summary>
    /// Invite die benutzt wird, um neue Personen mit Adminberechtigung Zugang zu geben
    /// </summary>
    public class Invite
    {
        [Key]
        public string InviteGUID { get; set; }

        [Required]
        public string EMailAdress { get; set; }

        public DateTime ExpirationTime { get; set; }

    }
}
