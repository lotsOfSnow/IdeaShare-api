using AutoMapper;
using IdeaShare.Api.Models;
using IdeaShare.Application.Interfaces;
using IdeaShare.Application.Options;
using IdeaShare.Domain;
using IdeaShare.Infrastructure;
using IdeaShare.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IdeaShare.Api
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            //});

            services.AddDbContextPool<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("LocalSqlServer"));
            });

            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("X-Total-Count");
                });
            });

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
                options.ClaimsIdentity.UserIdClaimType = "id";
            });

            var jwtSettings = new JwtSettings
            {
                SecretKey = Configuration["JwtSettings:SecretKey"],
                TokenLifetime = TimeSpan.FromDays(1)
            };
            services.AddSingleton(jwtSettings);

            var imageSettings = new ImageSettings
            {
                ProfilePicture = GetImageSettingsConfiguration("ProfilePicture"),
                FeaturedImage = GetImageSettingsConfiguration("FeaturedImage")
            };
            services.AddSingleton(imageSettings);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
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
            })
            .AddJwtBearer(x =>
                {
                    x.SaveToken = true;
                    x.TokenValidationParameters = tokenValidationParameters;
                });


            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            //services.AddCertificateForwarding(options =>
            //    options.CertificateHeader = "ideashare.mbugala.me");


            services.AddControllers();

            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "IdeaShare", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into the field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                      Reference = new OpenApiReference
                      {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                      }
                     },
                     new string[] { }
                    }
                });
            });

            services.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = actionContext =>
                    new BadRequestObjectResult(new ValidationResultModel(actionContext.ModelState));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            app.UseSwagger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCertificateForwarding();

            app.UseAuthentication();
            app.UseSwagger(option => { option.RouteTemplate = "v1/swagger.json"; });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "IdeaShare");
                //options.RoutePrefix = string.Empty;
            });
            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public ImageOptions GetImageSettingsConfiguration(string name)
        {
            var imageSettings = new ImageOptions
            {
                Width = int.Parse(Configuration[$"ImageSettings:{name}:Width"]),
                Height = int.Parse(Configuration[$"ImageSettings:{name}:Height"]),
                Location = Configuration[$"ImageSettings:{name}:Location"]
            };

            PropertyInfo prop = typeof(ImageFormat).GetProperties().Where(p => p.Name.Equals(Configuration[$"ImageSettings:{name}:Format"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (prop == null)
            {
                throw new Exception("Can't parse ImageFormat");
            }
            imageSettings.Format = prop.GetValue(prop) as ImageFormat;

            return imageSettings;
        }
    }
}
