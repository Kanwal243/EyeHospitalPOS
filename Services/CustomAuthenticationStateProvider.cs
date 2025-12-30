using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using EyeHospitalPOS.Helper;
using EyeHospitalPOS.Models;
using EyeHospitalPOS.Interfaces;
using System;

namespace EyeHospitalPOS.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly LoginManager _loginManager;
        private readonly IJwtService _jwtService;
        private readonly IServiceProvider _serviceProvider;

        public CustomAuthenticationStateProvider(LoginManager loginManager, IJwtService jwtService, IServiceProvider serviceProvider)
        {
            _loginManager = loginManager;
            _jwtService = jwtService;
            _serviceProvider = serviceProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Ensure LoginManager is initialized from session storage
            await _loginManager.InitializeAsync();

            var token = _loginManager.AuthToken;

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // 1. Check if token is valid
            if (_jwtService.IsTokenValid(token))
            {
                var principal = _jwtService.ValidateToken(token);
                if (principal != null)
                {
                    return new AuthenticationState(principal);
                }
            }

            // 2. Token expired? Try Refresh
            var refreshToken = _loginManager.RefreshToken;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    // Create a new scope to resolve IAuthService (to avoid potential circular issues or DB context concurrency if scoped wrong, though here it's Scoped-in-Scoped so fine)
                    // Actually, direct injection of IAuthService is fine since this is Scoped.
                    // But to be safe with EF Core context usage if multiple things use it:
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var authService = scope.ServiceProvider.GetRequiredService<Interfaces.IAuthService>();
                        
                        var refreshRequest = new Models.DTOs.RefreshTokenRequestDto
                        {
                            AccessToken = token,
                            RefreshToken = refreshToken
                        };

                        var result = await authService.RefreshTokenAsync(refreshRequest);

                        if (result.Success)
                        {
                            // Update stored tokens
                            _loginManager.AuthToken = result.AccessToken;
                            _loginManager.RefreshToken = result.RefreshToken;
                            await _loginManager.SaveUserAsync();

                            // Validate and return new principal
                            var principal = _jwtService.ValidateToken(result.AccessToken);
                            if (principal != null)
                            {
                                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
                                return new AuthenticationState(principal);
                            }
                        }
                    }
                }
                catch
                {
                    // Refresh failed
                }
            }

            // 3. If we get here, user is logged out
            await _loginManager.LogoutAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void NotifyUserAuthentication(User user)
        {
            // When user logs in manually, we trust the LoginManager state for the immediate UI update
            // proper validation happens on next GetAuthenticationStateAsync call (e.g. refresh or nav)
            // But we should use the Token's claims for consistency if possible, or build from User.
            // Let's build from User like before, but ensure we have the token available.
            
            // Better: Parse the token we just got!
            // But for now, sticking to the manual build is faster for UI feedback.
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
                new Claim("UserId", user.Id.ToString())
            }, "BaseAuth");
            
            var userPrincipal = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(userPrincipal)));
        }

        public void NotifyUserLogout()
        {
            var guest = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(guest)));
        }
    }
}
