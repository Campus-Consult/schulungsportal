﻿using Microsoft.EntityFrameworkCore.Internal;
using MimeKit;
using Schulungsportal_2.Models.Anmeldungen;
using Schulungsportal_2.Models.Schulungen;
using Schulungsportal_2.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Schulungsportal_2.Models
{
    public class MailingHelper
    {
        // logger
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            
        private IRazorViewToStringRenderer viewRenderer;
        private ISchulungsportalEmailSender emailSender;

        public MailingHelper(IRazorViewToStringRenderer viewRenderer, ISchulungsportalEmailSender emailSender) {
            this.viewRenderer = viewRenderer;
            this.emailSender = emailSender;
        }

        /// <summary>
        /// Diese Methode generiert und schickt eine Bestaetigungsmail an einen Nutzer, der sich zu einer Schulung angemeldet hat.
        /// </summary>
        /// <param name="anmeldung">Die Anmeldung, die beim anmelden erstellt wird.</param>
        /// <param name="schulung">Die Schulung, zu der sich der Nutzer angemeldet hat.</param>
        public async Task GenerateAndSendBestätigungsMailAsync(Anmeldung anmeldung, Schulung schulung, string vorstand, string rootUrl)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailSender.GetAbsendeAdresse())); //Absender
                message.To.Add(GetSafeMailboxAddress(anmeldung.Vorname + " " + anmeldung.Nachname, anmeldung.Email)); // Empfaenger
                message.Subject = "[INFO/noreply] Schulungsanmeldung " + schulung.Titel; //Betreff

                var selbstmanagementUrl = rootUrl + "/Anmeldung/Selbstmanagement/" + anmeldung.AccessToken;
                    
                var attachments = GetAppointment(schulung, new []{anmeldung.Email}, emailSender.GetAbsendeAdresse(), istAbsage: false);

                MailViewModel mvm = new MailViewModel
                {
                    SelbstmanagementUrl = selbstmanagementUrl,
                    Schulung = schulung,
                };

                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("BestaetigungsMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };

                var outmultipart = new Multipart("mixed");
                outmultipart.Add(body);
                foreach (var attachment in attachments)
                {
                    outmultipart.Add(attachment);
                }

                message.Body = outmultipart;

                await emailSender.SendEmailAsync(message);
            } catch(Exception e)
            {
                logger.Error(e);
                string code = "#601";
                e = new Exception("Fehler beim Versenden der Bestätigungsmail (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Diese Methode wird aufgerufen, wenn eine Schulung angelegt wurde und generiert und schickt eine Mail an den Dozenten der Schulung.
        /// </summary>
        /// <param name="schulung">Die Schulung, die angelegt wurde</param>
        public async Task GenerateAndSendAnlegeMailAsync(Schulung schulung, string rootUrl, string vorstand)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
                foreach(var dozent in schulung.Dozenten) {
                    message.To.Add(GetSafeMailboxAddress(dozent.Name, dozent.EMail)); // Empfaenger
                }
                message.Subject = "[INFO/noreply] Schulung angelegt"; //Betreff

                var teilnehmerListeUrl = rootUrl + "/Schulung/Teilnehmer/" + schulung.AccessToken;

                var dozentenEmails = schulung.Dozenten.Select(d => d.EMail);
                var attachments = GetAppointment(schulung, dozentenEmails, emailSender.GetAbsendeAdresse(), istAbsage: false);

                MailViewModel mvm = new MailViewModel
                {
                    Schulung = schulung,
                    TeilnehmerListeUrl = teilnehmerListeUrl,
                };

                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("AnlegeMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };

                var outmultipart = new Multipart("mixed");
                outmultipart.Add(body);
                foreach (var attachment in attachments)
                {
                    outmultipart.Add(attachment);
                }

                message.Body = outmultipart;

                await emailSender.SendEmailAsync(message);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#601";
                e = new Exception("Fehler beim Versenden der Anlegemail (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Diese Methode generiert und schickt eine Absagemail an einen Nutzer, der zu einer Schulung angemeldet ist, die abgesagt wird.
        /// </summary>
        /// <param name="anmeldung">Die Anmeldung.</param>
        /// <param name="schulung">Die Schulung, zu die abgesagt wird.</param>
        public async Task GenerateAndSendAbsageMailAsync(Anmeldung anmeldung, Schulung schulung, string vorstand)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
                message.To.Add(GetSafeMailboxAddress(anmeldung.Vorname + " " + anmeldung.Nachname, anmeldung.Email)); // Empfaenger
                message.Subject = "[INFO/noreply] Schulung abgesagt " + schulung.Titel; //Betreff
                
                MailViewModel mvm = new MailViewModel
                {
                    Schulung = schulung,
                };
                
                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("AbsageMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };
                var attachments = GetAppointment(schulung, new []{anmeldung.Email}, emailSender.GetAbsendeAdresse(), istAbsage: true);
                    
                var outmultipart = new Multipart("mixed");
                outmultipart.Add(body);
                foreach (var attachment in attachments)
                {
                    outmultipart.Add(attachment);
                }
                

                message.Body = outmultipart;

                await emailSender.SendEmailAsync(message);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#601";
                e = new Exception("Fehler beim Versenden der Absagemail (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        public async Task GenerateAndSendSchulungsNewsletterAsync(List<Schulung> schulungen, string vorstand)
        {
            // Kein newsletter ohne Schulungen
            if (schulungen.Count() == 0)
            {
                return;
            }
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
                // message.To.Add(new MailboxAddress("@everyone")); // Empfaenger
                message.Subject = "[INFO/noreply] Schulungsnewsletter"; //Betreff

                MailViewModel mvm = new MailViewModel
                {
                    Schulungen = schulungen,
                };

                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("NewsletterMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };

                var multipart = new MultipartRelated();
                multipart.Add(body);
                
                message.Body = multipart;

                await emailSender.SendEmailAsync(message);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#601";
                e = new Exception("Fehler beim Versenden des Newsletters (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        public async Task GenerateAndSendAbsageAnSchulungsdozentMailAsync(Anmeldung anmeldung, String begruendung, String vorstand) {
            var schulung = anmeldung.Schulung;
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
            foreach(var dozent in schulung.Dozenten) {
                message.To.Add(GetSafeMailboxAddress(dozent.Name, dozent.EMail)); // Empfaenger
            }
            message.Subject = "Schulung "+anmeldung.Schulung.Titel + ": Abmeldung eines Teilnehmers"; //Betreff

            MailViewModel mwm = new MailViewModel {
                Begruendung = begruendung,
                Anmeldung = anmeldung,
                Schulung = schulung,
            };

            var body = new TextPart("html") //Inhalt
            {
                Text = await RunCompileAsync("AbsageAnSchulungsdozentMail", mwm),
                ContentTransferEncoding = ContentEncoding.Base64,
            };

            var multipart = new MultipartRelated();
            multipart.Add(body);
            
            message.Body = multipart;

            await emailSender.SendEmailAsync(message);
        }

        public async Task GenerateAndSendInviteMailAsync(Invite invite, string rootUrl, string vorstand) {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
            message.To.Add(new MailboxAddress(invite.EMailAdress)); // Empfaenger
            message.Subject = "Invite Schulungsportal"; //Betreff

            InviteMailViewModel imwm = new InviteMailViewModel {
                InviteURL = rootUrl + "/Manage/Register/" + invite.InviteGUID,
            };

            var body = new TextPart("html") //Inhalt
            {
                Text = await RunCompileAsync("InviteMail", imwm),
                ContentTransferEncoding = ContentEncoding.Base64,
            };

            var multipart = new MultipartRelated();
            multipart.Add(body);
            
            message.Body = multipart;

            await emailSender.SendEmailAsync(message);
        }

        /// <summary>
        /// Diese Methode generiert und schickt eine Absagemail an einen Nutzer, der zu einer Schulung angemeldet ist, die abgesagt wird.
        /// </summary>
        /// <param name="anmeldung">Die Anmeldung.</param>
        /// <param name="schulung">Die Schulung, zu die abgesagt wird.</param>
        public async Task GenerateAndSendAbmeldungMailAsync(Anmeldung anmeldung, Schulung schulung, string vorstand)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
                message.To.Add(GetSafeMailboxAddress(anmeldung.Vorname + " " + anmeldung.Nachname, anmeldung.Email)); // Empfaenger
                message.Subject = "[INFO/noreply] Abmeldung von der Schulung " + schulung.Titel; //Betreff
                
                MailViewModel mvm = new MailViewModel
                {
                    Schulung = schulung,
                };
                
                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("AbmeldungMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };
                var attachments = GetAppointment(schulung, new []{anmeldung.Email}, emailSender.GetAbsendeAdresse(), istAbsage: true);
                
                var outmultipart = new Multipart("mixed");
                outmultipart.Add(body);
                foreach (var attachment in attachments)
                {
                    outmultipart.Add(attachment);
                }
                

                message.Body = outmultipart;

                await emailSender.SendEmailAsync(message);
            }
            catch (Exception e)
            {
                logger.Error(e);
                string code = "#601";
                e = new Exception("Fehler beim Versenden der Abeldungmail (" + e.Message + ") " + code, e);
                throw e;
            }
        }

        /// <summary>
        /// Diese Methode generiert und schickt eine Mail an die Dozenten der Schulungen einen Tag nach dieser um zu erinnern,
        /// die Anwesenheitsliste an den Schulungsbeauftragten zu senden
        /// </summary>
        /// <param name="anmeldung">Die Anmeldung.</param>
        /// <param name="schulung">Die Schulung, zu die abgesagt wird.</param>
        public async Task GenerateAndSendGeprueftReminderMail(Schulung schulung, string vorstand)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
            foreach(var dozent in schulung.Dozenten) {
                message.To.Add(GetSafeMailboxAddress(dozent.Name, dozent.EMail)); // Empfaenger
            }
            message.Subject = "[INFO/noreply] Reminder Teilnehmerliste " + schulung.Titel; //Betreff

            var multipart = new MultipartRelated();
            
            MailViewModel mvm = new MailViewModel
            {
                Schulung = schulung,
            };
            
            var body = new TextPart("html") //Inhalt
            {
                Text = await RunCompileAsync("GeprueftReminder", mvm),
                ContentTransferEncoding = ContentEncoding.Base64,
            };

            multipart.Add(body);
            message.Body = multipart;

            await emailSender.SendEmailAsync(message);
        }

        /// <summary>
        /// Creates a mailbox address using the name and the email address
        /// if the name is invalid, use just the email address
        /// </summary>
        public static MailboxAddress GetSafeMailboxAddress(string name, string email) {
            try {
                return new MailboxAddress(name, email);
            } catch(ParseException e) {
                logger.Warn("Problem parsing mailbox address :"+name, e);
                return new MailboxAddress(email);
            }
        }

        /// <summary>
        /// Generiert den Termin als Anhang (.ics-Datei)
        /// </summary>
        /// <param name="schulung"> Die Schulung für den Termin</param>
        /// <returns> Termin Anhang </returns>
        public static List<MimePart> GetAppointment(Schulung schulung, IEnumerable<string> empfaengerMails, string absender, Boolean istAbsage)
        {
            // Konstruieren der ics-Datei
            List<MimePart> parts = new List<MimePart>(schulung.Termine.Count);

            for (int i = 0; i < schulung.Termine.Count; i++)
            {
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);

                writer.WriteLine("BEGIN:VCALENDAR");
                writer.WriteLine("PRODID:-//Schedule a Meeting");
                writer.WriteLine("VERSION:2.0");
                writer.WriteLine("CALSCALE:GREGORIAN");
                if (istAbsage)
                {
                    writer.WriteLine("METHOD:CANCEL");
                }
                else
                {
                    writer.WriteLine("METHOD:REQUEST");
                }
                writer.WriteLine("BEGIN:VEVENT");
                writer.WriteLine(string.Format("DTSTART:{0:yyyyMMddTHHmmssZ}",
                    schulung.Termine.ElementAt(i).Start.ToUniversalTime()));
                writer.WriteLine(string.Format("DTSTAMP:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
                writer.WriteLine(string.Format("DTEND:{0:yyyyMMddTHHmmssZ}",
                    schulung.Termine.ElementAt(i).End.ToUniversalTime()));
                writer.WriteLine("LOCATION: " + schulung.Ort);
                var uid = "";
                if (i != 0)
                {
                    uid = i.ToString();
                }
                writer.WriteLine(string.Format("UID:{0}", schulung.SchulungGUID + uid));
                writer.WriteLine(string.Format("DESCRIPTION:{0}", HttpUtility.JavaScriptStringEncode(schulung.Beschreibung)));
                writer.WriteLine(string.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", HttpUtility.JavaScriptStringEncode(schulung.Beschreibung)));
                writer.WriteLine(string.Format("SUMMARY:{0}", schulung.Titel));
                writer.WriteLine(string.Format("ORGANIZER:MAILTO:{0}", absender));

                // Fun fact dieses Feld ist in der offiziellen Dokumentation
                // nicht als verpflichtend gekennzeichnet, lässt man es aber weg
                // ist office365 zu blöd, zu lesen an wen die Mail geht und erwartet das
                // dies hier steht. Ein sinnvoller Fehler wird auch nicht geworfen
                foreach(var empfaengerMail in empfaengerMails) {
                    writer.WriteLine(string.Format("ATTENDEE:{0}", empfaengerMail));
                }

                if (!istAbsage)
                {
                    writer.WriteLine("BEGIN:VALARM");
                    writer.WriteLine("TRIGGER:-PT15M");
                    writer.WriteLine("ACTION:DISPLAY");
                    writer.WriteLine("DESCRIPTION:Reminder");
                    writer.WriteLine("END:VALARM");
                }
                writer.WriteLine("END:VEVENT");
                writer.WriteLine("END:VCALENDAR");

                ContentType contype = new ContentType("text", "calendar");
                if (istAbsage)
                {
                    contype.Parameters.Add("method", "CANCEL");
                }
                else
                {
                    contype.Parameters.Add("method", "REQUEST");
                }
                contype.Parameters.Add("name", "Schulung" + i + ".ics");

                writer.Flush();
                stream.Position = 0;

                var attachment = new MimePart(contype)
                {
                    Content = new MimeContent(stream, ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = "Schulung" + i + ".ics"
                };
                parts.Add(attachment);
            }

            return parts;
        }

        private async Task<string> RunCompileAsync<T>(string filename, T mvm)
        {
            return await viewRenderer.RenderViewToStringAsync("/MailTemplates/"+filename+".cshtml",mvm);
        }
    }

    public class MailViewModel
    {
        public string TeilnehmerListeUrl { get; set; }
        public string SelbstmanagementUrl { get; set; }
        public string Begruendung { get; set; }
        public Anmeldung Anmeldung { get; set; }
        public Schulung Schulung { get; set; }
        public List<Schulung> Schulungen { get; set; }
    }

    public class InviteMailViewModel {
        public string InviteURL { get; set; }
    }
}
