using EyeHospitalPOS.Models;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EyeHospitalPOS.Helper
{
    public class LoginManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private User? _currentUser;
        private string? _authToken;
        private string? _refreshToken;
        private bool _initialized = false;

        public LoginManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        public string? AuthToken
        {
            get => _authToken;
            set => _authToken = value;
        }

        public string? RefreshToken
        {
            get => _refreshToken;
            set => _refreshToken = value;
        }

        public bool IsLoggedIn => _currentUser != null;
        public string GetUserName() => _currentUser?.UserName ?? "Guest";
        public string GetRoleName() => _currentUser?.Role?.Name ?? "Unknown";

        /// <summary>
        /// Load user from Session on app start
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;
            
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null && context.Session != null)
                {
                    await context.Session.LoadAsync();
                    
                    var userJson = context.Session.GetString("currentUser");
                    if (!string.IsNullOrEmpty(userJson))
                    {
                        _currentUser = JsonSerializer.Deserialize<User>(userJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }

                    _authToken = context.Session.GetString("authToken");
                    _refreshToken = context.Session.GetString("refreshToken");
                }
            }
            catch
            {
                // Session not available
            }
            
            _initialized = true;
        }

        /// <summary>
        /// Save user to Session after login
        /// </summary>
        public async Task SaveUserAsync()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null && context.Session != null)
                {
                    if (_currentUser != null)
                    {
                        var userJson = JsonSerializer.Serialize(_currentUser, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        context.Session.SetString("currentUser", userJson);
                    }

                    if (!string.IsNullOrEmpty(_authToken))
                    {
                        context.Session.SetString("authToken", _authToken);
                    }

                    if (!string.IsNullOrEmpty(_refreshToken))
                    {
                        context.Session.SetString("refreshToken", _refreshToken);
                    }
                    
                    await context.Session.CommitAsync();
                }
            }
            catch
            {
                // Session not available
            }
        }

        /// <summary>
        /// Clear session on logout
        /// </summary>
        public async Task LogoutAsync()
        {
            _currentUser = null;
            _authToken = null;
            _refreshToken = null;

            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null && context.Session != null)
                {
                    context.Session.Remove("currentUser");
                    context.Session.Remove("authToken");
                    context.Session.Remove("refreshToken");
                    context.Session.Clear(); // Optionally clear all
                    await context.Session.CommitAsync();
                }
            }
            catch
            {
                // Session not available
            }
        }
    }
}
