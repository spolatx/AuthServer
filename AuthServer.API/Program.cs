using AuthServer.Core.Configuration;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data.Repositories;
using AuthServer.Data;
using AuthServer.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary.Configurations;
using SharedLibrary.Services;
using System.Collections.Generic;
using System.Configuration;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FluentValidation.AspNetCore;
using System.Reflection;
using SharedLibrary.Extensions;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers().AddFluentValidation(options =>
{
    options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.UseCustomValidationResponse();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//DI Register
//Tek bir istekte bir tane nesne �rne�i olu�acak 
//bir requestte bir kere olu�sun.
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
//iki tane generic entity kullan�ld��� i�in bir virg�l kulland�k.
builder.Services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
    {
        //migrationun burada olaca��n� belirtiyorum.
        sqlOptions.MigrationsAssembly("AuthServer.Data");
    });
});
builder.Services.AddIdentity<UserApp, IdentityRole>(Opt =>
{
    Opt.User.RequireUniqueEmail = true;
    Opt.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
//�ifre s�f�rlama gibi i�lemlerde token �retebilmek i�in addDefaultToken methodunu kulland�m.
//Options Pattern
builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));

builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //Benim Authenticationdan gelen �emam ile jwt den gelen �emam� birbiriyle konu�turmam laz�m ki benim authenticationum  bir jwt � kullanaca��n� bilsin. bunun i�in de 
    //DefaultChallengeSchemema ayn�s�n� veriyorum.
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
{
    //bir token geldi�inde buradaki ayarlara g�re do�rulama i�lemlerini ger�ekle�tirecek.
    #region TokenAyarlar�
    var tokenOptions =builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience[0],
        IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        //normalde siz tokena bir �m�r verdi�inizde default olarak 5 dakika ekler 
        //sen bu api yi farkl� zonelarda farkl� zaman aral�klar� olan yerlerde kurabilirsin
        //bu iki server aras�ndaki zaman fark�n� tolere etmek i�in ekleme yapar.
        //benim tek bir serverim oldu�u i�in bunu zeroya setleyebilirim.
        ClockSkew = TimeSpan.Zero

    };
    #endregion
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCustomException();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
