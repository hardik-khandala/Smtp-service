namespace PortfolioBackend.Contacts;

public sealed record ContactRequest(
    string Name,
    string Email,
    string Subject,
    string Message,
    string? HoneypotToken = null
);

public sealed record ContactResponse(
    bool Success,
    string Message
);