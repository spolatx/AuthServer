using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Configurations;
using SharedLibrary.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Extensions
{
    public static class CustomTokenAuth
    {

        public static void AddCustomTokenAuth(this IServiceCollection services,CustomTokenOption tokenOptions)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //Benim Authenticationdan gelen şemam ile jwt den gelen şemamı birbiriyle konuşturmam lazım ki benim authenticationum  bir jwt ı kullanacağını bilsin. bunun için de 
                //DefaultChallengeSchemema aynısını veriyorum.
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
            {
                //bir token geldiğinde buradaki ayarlara göre doğrulama işlemlerini gerçekleştirecek.
                #region TokenAyarları 
                opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidIssuer = tokenOptions.Issuer,
                    ValidAudience = tokenOptions.Audience[0],
                    IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    //normalde siz tokena bir ömür verdiğinizde default olarak 5 dakika ekler 
                    //sen bu api yi farklı zonelarda farklı zaman aralıkları olan yerlerde kurabilirsin
                    //bu iki server arasındaki zaman farkını tolere etmek için ekleme yapar.
                    //benim tek bir serverim olduğu için bunu zeroya setleyebilirim.
                    ClockSkew = TimeSpan.Zero

                };
                #endregion
            });


        }
    }
}
