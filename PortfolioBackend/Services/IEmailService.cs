using PortfolioBackend.Contacts;

namespace PortfolioBackend.Services
{
    public interface IEmailService
    {
        Task SendContactEmailAsync(ContactRequest request, CancellationToken cancellationToken = default);
    }
}
