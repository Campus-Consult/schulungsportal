using System.Threading.Tasks;

/// <summary>
/// Dieses Interface wird nicht benutzt.
/// </summary>

namespace Schulungsportal_2.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
