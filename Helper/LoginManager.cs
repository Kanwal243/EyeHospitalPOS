using EyeHospitalPOS.Models;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Tasks;

namespace EyeHospitalPOS.Helper
{
    public class LoginManager
    {
        private readonly IJSRuntime _jsRuntime;
        private User? _currentUser;
        private string? _authToken;
        private string? _refreshToken;
        private bool _initialized = false;

        public LoginManager(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
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
        /// Load user from session storage on app start
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized) return;
            
            try
            {
                var userJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "currentUser");
                if (!string.IsNullOrEmpty(userJson))
                {
                    _currentUser = JsonSerializer.Deserialize<User>(userJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _authToken = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken");
                _refreshToken = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "refreshToken");
            }
            catch
            {
                // JS interop not available during prerendering
            }
            
            _initialized = true;
        }

        /// <summary>
        /// Save user to session storage after login
        /// </summary>
        public async Task SaveUserAsync()
        {
            try
            {
                if (_currentUser != null)
                {
                    var userJson = JsonSerializer.Serialize(_currentUser, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "currentUser", userJson);
                }

                if (!string.IsNullOrEmpty(_authToken))
                {
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "authToken", _authToken);
                }

                if (!string.IsNullOrEmpty(_refreshToken))
                {
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "refreshToken", _refreshToken);
                }
            }
            catch
            {
                // JS interop not available during prerendering
            }
        }

        /// <summary>
        /// Clear session on logout
        /// </summary>
        public async Task LogoutAsync()
        {
            _currentUser = null;
            _authToken = null;
            try
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "currentUser");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "refreshToken");
            }
            catch
            {
                // JS interop not available during prerendering
            }
        }
    }
}
