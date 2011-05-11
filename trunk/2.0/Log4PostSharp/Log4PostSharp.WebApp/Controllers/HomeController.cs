using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Log4PostSharp.WebApp.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";
            Session["MyValueKey"] = "Some session value";
            GetSessionValue("MyValueKey");

            return View();
        }

        public ActionResult About()
        {
            GetSessionValue("MyValueKey");
            return View();
        }

        private string GetSessionValue(string key)
        {
            var value = Session[key];

            if (value != null)
            {
                return value as string;
            }

            return String.Format("There are no value with key {0}", key);
        }
    }
}
