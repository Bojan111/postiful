using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using postiful.Models;

namespace postiful.Controllers
{
    public class DashboardController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}