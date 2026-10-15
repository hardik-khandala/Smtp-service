using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PortfolioBackend.Contacts;
using PortfolioBackend.Services;

namespace PortfolioBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("ContactFormPolicy")]
public sealed class ContactController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IValidator<ContactRequest> _validator;
    private readonly ILogger<ContactController> _logger;

    public ContactController(
        IEmailService emailService,
        IValidator<ContactRequest> validator,
        ILogger<ContactController> logger)
    {
        _emailService = emailService;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitContactForm(
        [FromBody] ContactRequest request,
        CancellationToken cancellationToken)
    {
        // 1. FluentValidation check
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            // Honeypot check: silently accept to trick spam bots without sending mail
            if (validationResult.Errors.Any(e => e.PropertyName == nameof(request.HoneypotToken)))
            {
                _logger.LogWarning("Spam detected via honeypot field from sender: {Email}", request.Email);
                return Ok(new ContactResponse(true, "Message received successfully."));
            }

            return BadRequest(new
            {
                Errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }

        // 2. Dispatch email via Zoho MailKit
        try
        {
            await _emailService.SendContactEmailAsync(request, cancellationToken);
            return Ok(new ContactResponse(true, "Your message has been sent successfully!"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception sending contact email.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ContactResponse(false, "An error occurred while dispatching your message. Please try again later."));
        }
    }
}