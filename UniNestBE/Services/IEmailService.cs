using System.Threading.Tasks;

namespace UniNestBE.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
