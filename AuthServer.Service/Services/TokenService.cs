using AuthServer.Core.Configuration;
using AuthServer.Core.DTOs;
using AuthServer.Core.Models;
using AuthServer.Core.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configurations;
using SharedLibrary.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<UserApp> _userManager;

        private readonly CustomTokenOption _tokenOption;

        //CustomToken ı IOptions üzerinden alma sebebim generic yapı kullanıp ileride 
        //farklı optionlar almak istersem sadece of kısmını değiştirmem yeterli olacak.
        //Yani Generic olarak ne verirsem onun valuesu üzerinden datamı alabilirim.

        public TokenService(UserManager<UserApp> userManager, IOptions<CustomTokenOption>options)
        {
            _userManager = userManager;
            _tokenOption=options.Value;
        }
        //TokenDto nesnemdeki RefreshTokenı set etmek için buradaki methodumu kullanıcam.
        private string CreateRefreshToken()
        {
            //return Guid.NewGuid().ToString();
            var numberByte = new Byte[32];
            //bu bana random bir değer üretecek.
            using var rnd = RandomNumberGenerator.Create();
            //üretilen random değerin bytlerini al ve benim yukarıda ürettiğim rnd değerine aktar dedik.
            rnd.GetBytes(numberByte);
            //Stringe dönüştürelim. 32 bytelık random bir değer ürettim.
            return Convert.ToBase64String(numberByte);
        }

        //kullanıcıyla ilgili payload oluşturalım.
        private IEnumerable<Claim> GetClaims(UserApp userApp,List<String> audiences)
        {
            var userList = new List<Claim>
            {
                //bir token üyelik sistemiyle ilgiliyse o tokenın payloadında mutlaka kullanıcınin idsi olmak zorunda.
                new Claim (ClaimTypes.NameIdentifier,userApp.Id),
                //new Claim("email) de yazılabilir.
                new Claim(JwtRegisteredClaimNames.Email,userApp.Email),
                new Claim(ClaimTypes.Name ,userApp.UserName),
                //best practies olarak json a identity veriyoruz.
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            //Aud isimlendirmesiyle beraber siz bir api ye istek yaptığınızda bu tokenın audisine bakıcak gerçekten kendisine istek yapılmaya uygun mu değil mi kontrol edecek eğer uygun değilse tokenı geri çevirecek onu da Tokenın payloadına ki buradaki audis keyinden bulacak.   
            userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
            return userList;

        }

        //üyelik sistemi gerektirmeyen bir token oluşturmak istediğimde claimlerimi bu methodla oluşturucam. 
        private IEnumerable<Claim> GetClaimsByClient(Client client)
        {
            var claims = new List<Claim>();
            claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
            new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString());
            
            return claims;
        }

        public ClientTokenDto CreateTokenByClient(Client client)
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
            var securityKey = SignService.GetSymmetricSecurityKey(_tokenOption.SecurityKey);
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                claims: GetClaimsByClient(client),
                signingCredentials: signingCredentials
                );
            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtSecurityToken);
            var tokenDto = new ClientTokenDto
            {
                AccessToken = token,
                AccessTokenExpiration = accessTokenExpiration,

            };

            return tokenDto;


        }

        public TokenDto CreateToken(UserApp userApp)
        {
            //token oluştururkenki saati aldım var olan saate tokenoptionsda belirlediğim süreyi ekledim.
            var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
            var refreshTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.RefreshTokenExpiration);
            //tokenimi imzalicak security key 
            var securityKey =SignService.GetSymmetricSecurityKey(_tokenOption.SecurityKey);

            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                expires: accessTokenExpiration,
                notBefore: DateTime.Now,
                claims:GetClaims(userApp,_tokenOption.Audience),
                signingCredentials:signingCredentials
                );
            //bu handler bir token oluşturacak.
            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(jwtSecurityToken);
            //Gelen tokeni bir tokendto ya dönüştürmem lazım.
            var tokenDto = new TokenDto
            {
                AccessToken = token,
                RefreshToken = CreateRefreshToken(),
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration
            };
            return tokenDto;
        }
    }
}
