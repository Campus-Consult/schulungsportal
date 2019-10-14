using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Schulungsportal_2.Data;
using Schulungsportal_2.Models;
using Schulungsportal_2.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Schulungsportal_2.Controllers
{
    public class ManageController : Controller
    {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private ISchulungsportalEmailSender mailSender;
        private InviteRepository InviteRepository;
        private ApplicationDbContext _context;

        public ManageController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ISchulungsportalEmailSender mailSender,
            ApplicationDbContext _context)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.mailSender = mailSender;
            this._context = _context;
            this.InviteRepository = new InviteRepository(_context);
        }

        [Authorize(Roles="Verwaltung")]
        public IActionResult AddManager() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddManager(ManagerAddModel manAdd) {
            if (await userManager.FindByEmailAsync(manAdd.EMailAdress) != null) {
                ViewBag.errorMessage = "User is already registered!";
                return View();
            }
            var rootUrl = string.Format("https://{0}",Request.Host);
            // create invite
            Invite invite = await InviteRepository.CreateForMail(manAdd.EMailAdress, DateTime.Now.AddDays(2));
            // send invite
            await MailingHelper.GenerateAndSendInviteMail(invite, rootUrl, getVorstand(), mailSender);
            ViewBag.successMessage = "Successfully sent invite Mail, this is valid for 2 days";
            return View();
        }

        [Route("Manage/Register/{inviteID}")]
        public async Task<IActionResult> Register(string inviteID) {
            // try to load proper invite and check if expired
            Invite invite = await InviteRepository.GetById(inviteID);
            if (invite == null || invite.ExpirationTime < DateTime.Now) {
                ViewBag.invalidInvite = true;
            } else {
                ViewBag.invalidInvite = false;
                ViewBag.loginName = invite.EMailAdress;
            }
            return View();
        }

        [HttpPost]
        [Route("Manage/Register/{inviteID}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string inviteID, RegisterModel register) {
            ViewBag.invalidInvite = false;
            if (register.Password != register.PasswordRepeat) {
                ViewBag.errorMessage = "Passwörter unterschiedlich!";
                return View();
            }
            // try to load proper invite and check if expired
            var invite = await InviteRepository.GetById(inviteID);
            if (invite == null || invite.ExpirationTime < DateTime.Now) {
                ViewBag.invalidInvite = true;
                return View();
            } else {
                var newUser = new IdentityUser{
                    Email = invite.EMailAdress,
                    UserName = invite.EMailAdress,
                };
                var result = await this.userManager.CreateAsync(newUser, register.Password);
                if (result.Succeeded) {
                    result = await this.userManager.AddToRoleAsync(newUser, "Verwaltung");
                    if (result.Succeeded) {
                        await InviteRepository.Remove(invite.InviteGUID);
                        // redirect to login on success
                        return Redirect("/Identity/Account/Login");
                    } else {
                        logger.Error(result.Errors);
                        ViewBag.errorMessage = "Ein unbekannter Fehler ist beim Zuweisen der Rolle aufgetreten";
                    }
                } else {
                    ViewBag.errorMessage = "Ein unbekannter Fehler ist beim Erstellen des Benutzers aufgetreten";
                    logger.Error(result.Errors);
                }
            }
            return View();
        }

        public class ManagerAddModel {
            
            [DataType(DataType.EmailAddress)]
            public string EMailAdress { get; set; }
        }

        public class RegisterModel {

            [Required]
            [Display(Name = "Passwort")]
            [StringLength(100, MinimumLength=8)]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            
            [Required]
            [Display(Name = "Passwort bestätigen")]
            [StringLength(100, MinimumLength=8)]
            [Compare("Password", ErrorMessage = "Passwörter stimmen nicht überein.")]
            [DataType(DataType.Password)]
            public string PasswordRepeat { get; set; }
        }
        
        /// <summary>
        /// Gibt den aktuellen gespeicherten Vorstand zurück,
        /// </summary>
        /// <returns></returns>
        public string getVorstand()
        {
            var impressum = _context.Impressum.FirstOrDefault();
            if (impressum == null)
            {
                return "";
            } else
            {
                return impressum.Vorstand;
            }
        }
    }
}