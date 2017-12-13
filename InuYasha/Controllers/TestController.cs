using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
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

        [HttpGet]
        public async Task<IActionResult> GetGuangZhouTianqi(string yearMonthDay)
        {
            if (DateTime.TryParse(yearMonthDay, out var _))
            {
                yearMonthDay = yearMonthDay.Replace("-", "").Replace(@"/", "");
                string url = $"http://www.tianqihoubao.com/lishi/guangzhou/{yearMonthDay}.html";
                HttpClient client = new HttpClient();
                var result = await client.GetStringAsync(url);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(result);
                StringBuilder stringBuilder = new StringBuilder();
                var div = document.DocumentNode.ChildNodes.FirstOrDefault(x => x.Id == "bd");
                if (div != null)
                {
                    var table = div.ChildNodes.FirstOrDefault(x => x.Attributes.Contains("cellpadding"));
                    //var table = document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[2]/div[6]/div[1]/div[1]/table[1]");
                    if (table != null)
                    {
                        var trs = table.SelectNodes("tr").Where(x => x.ChildNodes.Any(y => y.Name == "td"));
                        foreach (var tr in trs)
                        {
                            var trText = tr.ChildNodes.Where(x => x.Name == "td").Select(x => x.InnerText).Aggregate((x, y) => x + y);
                            stringBuilder.Append(trText);
                        }
                    }
                }
                return Content(stringBuilder.ToString());
            }
            return NotFound();
        }
    }
}
