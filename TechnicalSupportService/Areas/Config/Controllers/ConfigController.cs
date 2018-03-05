using System.Web.Mvc;
using TechnicalSupportService.Areas.Config.Models;

namespace TechnicalSupportService.Areas.Config.Controllers
{
    public class ConfigController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(ConfigModel configModel)
        {
            return View();
        }
    }
}
