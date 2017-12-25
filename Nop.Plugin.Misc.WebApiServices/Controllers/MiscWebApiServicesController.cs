using System.Web.Mvc;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.WebApiServices.Controllers
{
    [AdminAuthorize]
    public class MiscWebApiServicesController : BasePluginController
    {
        public ActionResult Configure()
        {
            return View("~/Plugins/Misc.WebApiServices/Views/MiscWebApiServices/Configure.cshtml");
        }
    }
}
