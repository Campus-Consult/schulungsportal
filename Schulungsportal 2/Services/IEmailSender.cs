using MimeKit;
using System.Threading.Tasks;


/// <summary>
/// Der IEmailSender ist ein Interface zum Versenden von Emails und erzeugt einen Task der asynchron ausgefuehrt wird.
/// </summary>

namespace Schulungsportal_2.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendEmailAsync(MimeMessage message);
        string GetAdresseSchulungsbeauftragter();
        string GetAbsendeAdresse();
    }
}
