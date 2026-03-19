using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SupportDeskAPI.Context;
using SupportDeskAPI.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace SupportDeskAPI.Auth
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly SupportDeskService _service;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, SupportDeskService supportDeskService,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _service = supportDeskService;    
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                if (!authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
                    return AuthenticateResult.Fail("Invalid Authorization Scheme");

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                if (credentials.Length != 2)
                    return AuthenticateResult.Fail("Invalid Authorization Header");

                var username = credentials[0];
                var password = credentials[1];

                // Validate credentials (in real apps, check DB or config)
                var userRole = await _service.LogInUserAsync(username, password);
                if (string.IsNullOrEmpty(userRole))
                    return AuthenticateResult.Fail("Invalid Username or Password");

                var claims = new[] { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, userRole) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }
}
