using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SampleServer.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ViewBag.Title = "Home Page";

            if (Utilities.Instance.IsAccessKeySet && Utilities.Instance.IsSecretKeySet &&
                Utilities.Instance.IsTableNameSet && Utilities.Instance.IsRegionNameSet)
            {
                Task.Run(async () => await Utilities.Instance.SetupTable());
            }



            return View();
        }
    }
}
