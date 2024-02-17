using AuthServer.Core.Configuration;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data;
using AuthServer.Data.Repositories;
using AuthServer.Service.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SharedLibrary.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedLibrary.Services;
using FluentValidation.AspNetCore;
using SharedLibrary.Extensions;

namespace AuthServer.API
{
    public class Startup
    {



        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //DI Register
            //Tek bir istekte bir tane nesne �rne�i olu�acak 
            //bir requestte bir kere olu�sun.
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            //iki tane generic entity kullan�ld��� i�in bir virg�l kulland�k.
            services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlServer"), sqlOptions =>
                {
                    //migrationun burada olaca��n� belirtiyorum.
                    sqlOptions.MigrationsAssembly("AuthServer.Data");
                });
            });
            services.AddIdentity<UserApp, IdentityRole>(Opt =>
            {
                Opt.User.RequireUniqueEmail = true;
                Opt.Password.RequireNonAlphanumeric = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
            //�ifre s�f�rlama gibi i�lemlerde token �retebilmek i�in addDefaultToken methodunu kulland�m.
            //Options Pattern
            services.Configure<CustomTokenOption>(Configuration.GetSection("TokenOption"));
         
            services.Configure<List<Client>>(Configuration.GetSection("Clients"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //Benim Authenticationdan gelen �emam ile jwt den gelen �emam� birbiriyle konu�turmam laz�m ki benim authenticationum  bir jwt � kullanaca��n� bilsin. bunun i�in de 
                //DefaultChallengeSchemema ayn�s�n� veriyorum.
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
            {
                //bir token geldi�inde buradaki ayarlara g�re do�rulama i�lemlerini ger�ekle�tirecek.
                #region TokenAyarlar�
                var tokenOptions = Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
                opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience[0],
                    IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

                    ValidateIssuerSigningKey=true,
                    ValidateAudience=true,
                    ValidateIssuer =true,
                    ValidateLifetime = true,
                    //normalde siz tokena bir �m�r verdi�inizde default olarak 5 dakika ekler 
                    //sen bu api yi farkl� zonelarda farkl� zaman aral�klar� olan yerlerde kurabilirsin
                    //bu iki server aras�ndaki zaman fark�n� tolere etmek i�in ekleme yapar.
                    //benim tek bir serverim oldu�u i�in bunu zeroya setleyebilirim.
                    ClockSkew=TimeSpan.Zero

                };
                #endregion
            });
            services.AddControllers().AddFluentValidation(options =>
            {
                options.RegisterValidatorsFromAssemblyContaining<Startup>();
            });
            services.UseCustomValidationResponse();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthServer.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthServer.API v1"));
            }
            else
            {
                
            }
            //s�ralama onemli
            app.UseCustomException();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
