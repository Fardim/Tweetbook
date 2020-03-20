using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Tweetbook.Authorization;
using Tweetbook.Options;
using Tweetbook.Services;

namespace Tweetbook.Installers
{
    public class MvcInstaller: IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.Bind(nameof(jwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);
            
            services.AddScoped<IIdentityService, IdentityService>();

            services.AddMvc(options => { options.EnableEndpointRouting = false; }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ValidateLifetime = true
            };
            services.AddSingleton(tokenValidationParameters);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.TokenValidationParameters = tokenValidationParameters;
            });
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("TagViewer", builder => builder.RequireClaim("tags.view", "true"));
            //});
            //services.AddAuthorization();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MustWorkForKaiser", builder =>
                    {
                        builder.AddRequirements(new WorksForCompanyRequirement("kaiser.com"));
                    });
            });
            services.AddSingleton<IAuthorizationHandler, WorksForCompanyHandler>();
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Tweetbook API", Version = "v1" });
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] {} }
                };
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement 
                {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Id="Bearer",
                                Type= ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}
