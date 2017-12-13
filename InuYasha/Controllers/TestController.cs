using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InuYasha.Option;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InuYasha.Controllers
{
    public class TestController : Controller
    {
        private readonly MyOption _option;
        private readonly MyOption _optionsSnapshot;

        public TestController(IOptions<MyOption> option, IOptionsSnapshot<MyOption> optionsSnapshot)
        {
            _option = option.Value;
            this._optionsSnapshot = optionsSnapshot.Get("codeConfig");
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return Content($"{_option.Name} {_option.Age} Hello {_optionsSnapshot.Name + " " + _optionsSnapshot.Age}!");
        }
    }
}
