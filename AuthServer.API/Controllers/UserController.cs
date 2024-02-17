using AuthServer.Core.DTOs;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedLibrary.Exceptions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AuthServer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : CustomBaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        //api/user
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            //throw new CustomException("Veri tabanı ile ilgili bir hata meydana geldi");
            return ActionResultInstance(await _userService.CreateUserAsync(createUserDto));
        }
        //authcontroller kısmında token oluşturuyor
        //authorize ile işaretledim bu endpoint mutlaka bir token istiyor.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            return ActionResultInstance(await _userService.GetUserByNameAsync(HttpContext.User.Identity.Name)); 
        }

    }
}
