using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace MiniApp2.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        public IActionResult GetInvoice()
        {
            var userName = HttpContext.User.Identity.Name;
            //veri tabanında istediğimiz kullanıcıya ait userId veya userName alanları üzerinden gerekli dataları alabiliriz.
            //Token service üzerinden hangi claim tipinde verdiysek burada da aynı şekilde almak gerekir
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return Ok($" Invoice işlemleri => Username :{userName} - UserId :{userIdClaim.Value}");
        }
    }
}
