using EyeHospitalPOS.Data;
using EyeHospitalPOS.Interfaces;
using EyeHospitalPOS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace EyeHospitalPOS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Blazor services
            services.AddRazorPages();
            services.AddControllers(options => 
            {
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
            });
            services.AddServerSideBlazor();
            services.AddHttpClient("LocalApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7146/"); // Update port if needed
            });
            services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("LocalApi"));
            services.AddDevExpressBlazor(configure => configure.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5);

            // Session Services
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            services.AddHttpContextAccessor();

            // Custom AuthenticationStateProvider using LoginManager
            services.AddScoped<CustomAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<CustomAuthenticationStateProvider>());

            // Developer exception page
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Register LoginManager
            services.AddScoped<EyeHospitalPOS.Helper.LoginManager>();

            // Register Business Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISalesService, SalesService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<BarcodeService>();

            // Register Controllers
            services.AddScoped<EyeHospitalPOS.Controllers.LoginController>();
            services.AddScoped<EyeHospitalPOS.Controllers.DashboardController>();
            services.AddScoped<EyeHospitalPOS.Controllers.ProductController>();
            services.AddScoped<EyeHospitalPOS.Controllers.POSController>();
            services.AddScoped<EyeHospitalPOS.Controllers.UserController>();

            // Register JWT Service
            services.AddScoped<IJwtService, JwtService>();

            // Antiforgery
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.Name = "XSRF-TOKEN";
                options.Cookie.HttpOnly = false; // Accessible by JS for sending in headers
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            });

            // JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var secretKey = Configuration["Jwt:SecretKey"];
                var issuer = Configuration["Jwt:Issuer"];
                var audience = Configuration["Jwt:Audience"];

                // Validate JWT configuration
                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json. Please add 'Jwt:SecretKey' configuration.");
                }

                if (string.IsNullOrWhiteSpace(issuer))
                {
                    throw new InvalidOperationException("JWT Issuer is not configured in appsettings.json. Please add 'Jwt:Issuer' configuration.");
                }

                if (string.IsNullOrWhiteSpace(audience))
                {
                    throw new InvalidOperationException("JWT Audience is not configured in appsettings.json. Please add 'Jwt:Audience' configuration.");
                }

                // Validate SecretKey length (should be at least 32 bytes for HS256)
                if (Encoding.UTF8.GetByteCount(secretKey) < 32)
                {
                    throw new InvalidOperationException("JWT SecretKey must be at least 32 bytes (256 bits) for security. Current key is too short.");
                }

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Swagger/OpenAPI Configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EyeHospitalPOS API", Version = "v1" });
                
                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                };

                c.AddSecurityRequirement(securityRequirement);
            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EyeHospitalPOS API v1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
