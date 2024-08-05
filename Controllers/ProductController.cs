
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using WebFunctionApp.Models;
using WebFunctionApp.Repository;
using Microsoft.AspNetCore.Authorization;

namespace WebFunctionApp.Controllers
{
   // [Authorize]
    public class ProductController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }
    }
}
