using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.APM.RedisProfiler;
using AspectCore.Extensions.Reflection;
using AspectCore.Injector;
using InuYasha.Intercptor;
using Microsoft.AspNetCore.Mvc;
using InuYasha.Models;
using InuYasha.Service;

namespace InuYasha.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICustomService _service;
        private readonly IContactService _contactService;

        [FromContainer]
        public IContactService ContactService { get; set; }

        public HomeController(ICustomService service, IContactService contactService)
        {
            _service = service;
            _contactService = contactService;
        }

        public IActionResult Index()
        {
            _service.Call();



            return View();
        }

        public IActionResult About([FromServices]IConnectionMultiplexerProvider connectionMultiplexerProvider)
        {
            var result = connectionMultiplexerProvider.ConnectionMultiplexer.GetDatabase().StringGet("message");

            var fuck = _contactService.GetMessage();

            //Debug.WriteLine(fuck);

            ViewData["Message"] = result.HasValue ? fuck + " " + result : "empty";

            //Random random = new Random();

            //for (int i = 0; i < 10; i++)
            //{
            //    Task.Run(() =>
            //    {
            //        Task.Delay(TimeSpan.FromMilliseconds(random.Next(5))).Wait();
            //    });
            //}

            return View();
        }

        [TestIntercptor]
        public virtual IActionResult Contact([FromServices]IContactService service, [FromServices]IConnectionMultiplexerProvider connectionMultiplexerProvider)
        {
            Random random = new Random();
            var num = random.Next(10);
            connectionMultiplexerProvider.ConnectionMultiplexer.GetDatabase().StringSet("message", $"我是随机数 {num}");

            var b = ReferenceEquals(service, _contactService) && ReferenceEquals(service, ContactService);

            ViewData["Message"] = service.GetMessage() + " " + b;

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Test([FromServices]ITestCheckInput testCheckInput)
        {
            var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[0]);
            var reflector = constructor.GetReflector();
            var fakes = (ConstructorFakes)reflector.Invoke();

            var v = fakes.Name;

            var constructor1 = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
            var reflector1 = constructor.GetReflector();
            var fakes1 = (ConstructorFakes)reflector.Invoke("test");

            v = v + fakes1.Name;

            testCheckInput.Register(new RegisterInput { Name = null, Email = null });
            testCheckInput.Register(new RegisterInput { Name = "lemon", Email = "lemon", Password = "****" });
            testCheckInput.Register(new RegisterInput { Name = "lemon", Email = "lemon@lemon.com", Password = "****" });
            testCheckInput.Register(new RegisterInput { Name = "lemon", Email = "lemon@lemon.com", Password = "*******" });

            return Content("ok " + v);
        }
    }


    public struct StructFakes
    {
        public string Name { get; set; }

        public StructFakes(string name)
        {
            Name = "Parametric constructor. param : " + name;
        }

        public StructFakes(ref string name, ref StructFakes fakes, string lastName)
        {
            lastName = name = Name = "Parametric constructor with ref param.";
            fakes = new StructFakes();
        }
    }

    public class ConstructorFakes
    {
        public string Name { get; set; }
        public ConstructorFakes()
        {
            Name = "Nonparametric constructor";
        }

        public ConstructorFakes(string name)
        {
            Name = "Parametric constructor. param : " + name;
        }

        public ConstructorFakes(ref string name, ref ConstructorFakes fakes, string lastName)
        {
            lastName = name = Name = "Parametric constructor with ref param.";
            fakes = new ConstructorFakes();
        }
    }
}
