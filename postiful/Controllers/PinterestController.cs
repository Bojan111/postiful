using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using postiful.Models.PinterestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using EnginaCode.Services.PinterestServices;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace postiful.Controllers
{
    public class PinterestController : Controller
    {

        private readonly IPinterestService _pinterestService;

        public PinterestController(IPinterestService pinterestService)
        {
            _pinterestService = pinterestService;
        }
        //// GET: /<controller>/
        //public IActionResult Index()
        //{
        //    return View(new CreatePinterestPin());
        //}

        // GET: /<controller>/
        public IActionResult CreatePinterestPin()
        {
            return View(new CreatePinterestPin());
        }

        [HttpPost]
        public IActionResult CreatePinterestPin(CreatePinterestPin model)
        {
            var createdPinterestPin = _pinterestService.CreatePin(model);

            return RedirectToAction("Index", "Dashboard");
        }
    }
}

