using Microsoft.EntityFrameworkCore.Internal;
using MimeKit;
using RazorLight;
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

        private static RazorLightEngine engine = new RazorLightEngineBuilder()
                .UseMemoryCachingProvider()
                .Build();

        /// <summary>
        /// Diese Methode generiert und schickt eine Bestaetigungsmail an einen Nutzer, der sich zu einer Schulung angemeldet hat.
        /// </summary>
        /// <param name="anmeldung">Die Anmeldung, die beim anmelden erstellt wird.</param>
        /// <param name="schulung">Die Schulung, zu der sich der Nutzer angemeldet hat.</param>
        public static async Task GenerateAndSendBestätigungsMailAsync(Anmeldung anmeldung, Schulung schulung, string vorstand, string rootUrl, ISchulungsportalEmailSender emailSender)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailSender.GetAbsendeAdresse())); //Absender
                message.To.Add(new MailboxAddress(anmeldung.Vorname + " " + anmeldung.Nachname, anmeldung.Email)); // Empfaenger
                message.Subject = "[INFO/noreply] Schulungsanmeldung " + schulung.Titel; //Betreff

                var selbstmanagementUrl = rootUrl + "/Anmeldung/Selbstmanagement/" + anmeldung.AccessToken;
                    
                var attachments = GetAppointment(schulung, anmeldung.Email, emailSender.GetAbsendeAdresse(), istAbsage: false);

                MailViewModel mvm = new MailViewModel
                {
                    //CCLogoFile = "cclogo.png@"+Guid.NewGuid().ToString(),
                    //FacebookLogoFile = "fblogo.png@" + Guid.NewGuid().ToString(),
                    //InstaLogoFile = "instalogo.png@" + Guid.NewGuid().ToString(),
                    SelbstmanagementUrl = selbstmanagementUrl,
                    Schulung = schulung,
                    Vorstand = vorstand,
                };

                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("BestaetigungsMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };

                var outmultipart = new Multipart("mixed");
                outmultipart.Add(body);
                //inmultipart.Add(attachments.First());
                // Bilder für Corporate Design, funktioniert leider nicht
                //outmultipart.Add(inmultipart);
                //outmultipart.Add(LoadInlinePicture("CCLogo.png", mvm.CCLogoFile));
                //outmultipart.Add(LoadInlinePicture("FBLogo.png", mvm.FacebookLogoFile));
                //outmultipart.Add(LoadInlinePicture("InstaLogo.png", mvm.InstaLogoFile));
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
        public static async Task GenerateAndSendAnlegeMailAsync(Schulung schulung, string rootUrl, string vorstand, ISchulungsportalEmailSender emailSender)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
                message.To.Add(new MailboxAddress(schulung.NameDozent, schulung.EmailDozent));//Empfaenger 
                message.Subject = "[INFO/noreply] Schulung angelegt"; //Betreff

                var teilnehmerListeUrl = rootUrl + "/Schulung/Teilnehmer/" + schulung.AccessToken;

                var attachments = GetAppointment(schulung, schulung.EmailDozent, emailSender.GetAbsendeAdresse(), istAbsage: false);

                MailViewModel mvm = new MailViewModel
                {
                    //CCLogoFile = "cclogo.png@"+Guid.NewGuid().ToString(),
                    //FacebookLogoFile = "fblogo.png@" + Guid.NewGuid().ToString(),
                    //InstaLogoFile = "instalogo.png@" + Guid.NewGuid().ToString(),
                    Schulung = schulung,
                    Vorstand = vorstand,
                    TeilnehmerListeUrl = teilnehmerListeUrl,
                };

                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("AnlegeMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };

                var outmultipart = new Multipart("mixed");
                outmultipart.Add(body);
                //inmultipart.Add(attachments.First());
                // Bilder für Corporate Design, funktioniert leider nicht
                //outmultipart.Add(inmultipart);
                //outmultipart.Add(LoadInlinePicture("CCLogo.png", mvm.CCLogoFile));
                //outmultipart.Add(LoadInlinePicture("FBLogo.png", mvm.FacebookLogoFile));
                //outmultipart.Add(LoadInlinePicture("InstaLogo.png", mvm.InstaLogoFile));
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
        public static async Task GenerateAndSendAbsageMailAsync(Anmeldung anmeldung, Schulung schulung, string vorstand, ISchulungsportalEmailSender emailSender)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
                message.To.Add(new MailboxAddress(anmeldung.Vorname + " " + anmeldung.Nachname, anmeldung.Email)); // Empfaenger
                message.Subject = "[INFO/noreply] Schulung abgesagt " + schulung.Titel; //Betreff
                
                MailViewModel mvm = new MailViewModel
                {
                    //CCLogoFile = "cclogo.png@"+Guid.NewGuid().ToString(),
                    //FacebookLogoFile = "fblogo.png@" + Guid.NewGuid().ToString(),
                    //InstaLogoFile = "instalogo.png@" + Guid.NewGuid().ToString(),
                    Schulung = schulung,
                    Vorstand = vorstand,
                };
                
                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("AbsageMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };
                var attachments = GetAppointment(schulung, anmeldung.Email, emailSender.GetAbsendeAdresse(), istAbsage: true);
                    
                var outmultipart = new Multipart("mixed");
                outmultipart.Add(body);
                //inmultipart.Add(attachments.First());
                // Bilder für Corporate Design, funktioniert leider nicht
                //outmultipart.Add(inmultipart);
                //outmultipart.Add(LoadInlinePicture("CCLogo.png", mvm.CCLogoFile));
                //outmultipart.Add(LoadInlinePicture("FBLogo.png", mvm.FacebookLogoFile));
                //outmultipart.Add(LoadInlinePicture("InstaLogo.png", mvm.InstaLogoFile));
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

        public static async Task GenerateAndSendSchulungsNewsletterAsync(List<Schulung> schulungen, string vorstand, ISchulungsportalEmailSender emailSender)
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
                    CCLogoFile = "cclogo.png@"+Guid.NewGuid().ToString(),
                    FacebookLogoFile = "fblogo.png@" + Guid.NewGuid().ToString(),
                    InstaLogoFile = "instalogo.png@" + Guid.NewGuid().ToString(),
                    Schulungen = schulungen,
                    Vorstand = vorstand,
                };

                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("NewsletterMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };

                var multipart = new MultipartRelated();
                multipart.Add(body);
                // Bilder für Corporate Design
                multipart.Add(LoadInlinePicture("CCLogo.png", mvm.CCLogoFile));
                multipart.Add(LoadInlinePicture("FBLogo.png", mvm.FacebookLogoFile));
                multipart.Add(LoadInlinePicture("InstaLogo.png", mvm.InstaLogoFile));
                
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

        public static async Task GenerateAndSendAbsageAnSchulungsdozentMailAsync(Anmeldung anmeldung, String begruendung, String vorstand, ISchulungsportalEmailSender emailSender) {
            var schulung = anmeldung.Schulung;
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
            message.To.Add(new MailboxAddress(schulung.NameDozent, schulung.EmailDozent)); // Empfaenger
            message.Subject = "Schulung "+anmeldung.Schulung.Titel + ": Abmeldung eines Teilnehmers"; //Betreff

            MailViewModel mwm = new MailViewModel {
                Vorstand = vorstand,
                Begruendung = begruendung,
                Anmeldung = anmeldung,
                Schulung = schulung,
                CCLogoFile = "cclogo.png@"+Guid.NewGuid().ToString(),
                FacebookLogoFile = "fblogo.png@" + Guid.NewGuid().ToString(),
                InstaLogoFile = "instalogo.png@" + Guid.NewGuid().ToString(),
            };

            var body = new TextPart("html") //Inhalt
            {
                Text = await RunCompileAsync("AbsageAnSchulungsdozentMail", mwm),
                ContentTransferEncoding = ContentEncoding.Base64,
            };

            var multipart = new MultipartRelated();
            multipart.Add(body);
            // Bilder für Corporate Design
            multipart.Add(LoadInlinePicture("CCLogo.png", mwm.CCLogoFile));
            multipart.Add(LoadInlinePicture("FBLogo.png", mwm.FacebookLogoFile));
            multipart.Add(LoadInlinePicture("InstaLogo.png", mwm.InstaLogoFile));
            
            message.Body = multipart;

            await emailSender.SendEmailAsync(message);
        }

        public static async Task GenerateAndSendInviteMailAsync(Invite invite, string rootUrl, string vorstand, ISchulungsportalEmailSender emailSender) {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
            message.To.Add(new MailboxAddress(invite.EMailAdress)); // Empfaenger
            message.Subject = "Invite Schulungsportal"; //Betreff

            InviteMailViewModel imwm = new InviteMailViewModel {
                Vorstand = vorstand,
                InviteURL = rootUrl + "/Manage/Register/" + invite.InviteGUID,
                CCLogoFile = "cclogo.png@"+Guid.NewGuid().ToString(),
                FacebookLogoFile = "fblogo.png@" + Guid.NewGuid().ToString(),
                InstaLogoFile = "instalogo.png@" + Guid.NewGuid().ToString(),
            };

            var body = new TextPart("html") //Inhalt
            {
                Text = await RunCompileAsync("InviteMail", imwm),
                ContentTransferEncoding = ContentEncoding.Base64,
            };

            var multipart = new MultipartRelated();
            multipart.Add(body);
            // Bilder für Corporate Design
            multipart.Add(LoadInlinePicture("CCLogo.png", imwm.CCLogoFile));
            multipart.Add(LoadInlinePicture("FBLogo.png", imwm.FacebookLogoFile));
            multipart.Add(LoadInlinePicture("InstaLogo.png", imwm.InstaLogoFile));
            
            message.Body = multipart;

            await emailSender.SendEmailAsync(message);
        }

        /// <summary>
        /// Diese Methode generiert und schickt eine Absagemail an einen Nutzer, der zu einer Schulung angemeldet ist, die abgesagt wird.
        /// </summary>
        /// <param name="anmeldung">Die Anmeldung.</param>
        /// <param name="schulung">Die Schulung, zu die abgesagt wird.</param>
        public static async Task GenerateAndSendAbmeldungMailAsync(Anmeldung anmeldung, Schulung schulung, string vorstand, ISchulungsportalEmailSender emailSender)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
                message.To.Add(new MailboxAddress(anmeldung.Vorname + " " + anmeldung.Nachname, anmeldung.Email)); // Empfaenger
                message.Subject = "[INFO/noreply] Abmeldung von der Schulung " + schulung.Titel; //Betreff
                
                MailViewModel mvm = new MailViewModel
                {
                    //CCLogoFile = "cclogo.png@"+Guid.NewGuid().ToString(),
                    //FacebookLogoFile = "fblogo.png@" + Guid.NewGuid().ToString(),
                    //InstaLogoFile = "instalogo.png@" + Guid.NewGuid().ToString(),
                    Schulung = schulung,
                    Vorstand = vorstand,
                };
                
                var body = new TextPart("html") //Inhalt
                {
                    Text = await RunCompileAsync("AbmeldungMail", mvm),
                    ContentTransferEncoding = ContentEncoding.Base64,
                };
                var attachments = GetAppointment(schulung, anmeldung.Email, emailSender.GetAbsendeAdresse(), istAbsage: true);
                
                var outmultipart = new Multipart("mixed");
                outmultipart.Add(body);
                //inmultipart.Add(attachments.First());
                // Bilder für Corporate Design, funktioniert leider nicht
                //outmultipart.Add(inmultipart);
                //outmultipart.Add(LoadInlinePicture("CCLogo.png", mvm.CCLogoFile));
                //outmultipart.Add(LoadInlinePicture("FBLogo.png", mvm.FacebookLogoFile));
                //outmultipart.Add(LoadInlinePicture("InstaLogo.png", mvm.InstaLogoFile));
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
        public static async Task GenerateAndSendGeprueftReminderMail(Schulung schulung, string vorstand, ISchulungsportalEmailSender emailSender)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Schulungsportal", emailSender.GetAbsendeAdresse())); //Absender
            message.To.Add(new MailboxAddress(schulung.NameDozent, schulung.EmailDozent)); // Empfaenger
            message.Subject = "[INFO/noreply] Reminder Teilnehmerliste " + schulung.Titel; //Betreff

            var multipart = new MultipartRelated();
            
            MailViewModel mvm = new MailViewModel
            {
                CCLogoFile = "cclogo.png@"+Guid.NewGuid().ToString(),
                FacebookLogoFile = "fblogo.png@" + Guid.NewGuid().ToString(),
                InstaLogoFile = "instalogo.png@" + Guid.NewGuid().ToString(),
                Schulung = schulung,
                Vorstand = vorstand,
            };
            
            var body = new TextPart("html") //Inhalt
            {
                Text = await RunCompileAsync("GeprueftReminder", mvm),
                ContentTransferEncoding = ContentEncoding.Base64,
            };

            multipart.Add(body);
            // Bilder für Corporate Design
            multipart.Add(LoadInlinePicture("CCLogo.png", mvm.CCLogoFile));
            multipart.Add(LoadInlinePicture("FBLogo.png", mvm.FacebookLogoFile));
            multipart.Add(LoadInlinePicture("InstaLogo.png", mvm.InstaLogoFile));
            message.Body = multipart;

            await emailSender.SendEmailAsync(message);
        }


        /// <summary>
        /// Generiert den Termin als Anhang (.ics-Datei)
        /// </summary>
        /// <param name="schulung"> Die Schulung für den Termin</param>
        /// <returns> Termin Anhang </returns>
        public static List<MimePart> GetAppointment(Schulung schulung, string empfaengerMail, string absender, Boolean istAbsage)
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
                writer.WriteLine(string.Format("ATTENDEE:{0}", empfaengerMail));

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

        private static async Task<string> RunCompileAsync<T>(string filename, T mvm)
        {
            var cacheResult = engine.TemplateCache.RetrieveTemplate(filename);
            if (cacheResult.Success)
            {
                return await engine.RenderTemplateAsync(cacheResult.Template.TemplatePageFactory(), mvm);
            } else
            {
                var stream = Assembly.GetAssembly(typeof(MailingHelper)).GetManifestResourceStream("Schulungsportal_2.MailTemplates." + filename + ".cshtml");
                var template = await new StreamReader(stream).ReadToEndAsync();
                return await engine.CompileRenderAsync(filename, template, mvm);
            }
        }

        private static MimePart LoadInlinePicture(string filename, string contentID)
        {
            return new MimePart("image", "png")
            {
                Content = new MimeContent(Assembly.GetAssembly(typeof(MailingHelper)).GetManifestResourceStream("Schulungsportal_2.MailTemplates."+filename)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
                ContentId = contentID,
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = filename
            };
        }
    }

    public class MailViewModel
    {
        public string TeilnehmerListeUrl { get; set; }
        public string SelbstmanagementUrl { get; set; }
        public string Vorstand { get; set; }
        public string Begruendung { get; set; }
        public Anmeldung Anmeldung { get; set; }
        public Schulung Schulung { get; set; }
        public List<Schulung> Schulungen { get; set; }
        public string CCLogoFile { get; set; }
        public string FacebookLogoFile { get; set; }
        public string InstaLogoFile { get; set; }
    }

    public class InviteMailViewModel {
        public string InviteURL { get; set; }
        public string Vorstand { get; set; }
        public string CCLogoFile { get; set; }
        public string FacebookLogoFile { get; set; }
        public string InstaLogoFile { get; set; }
    }
}
