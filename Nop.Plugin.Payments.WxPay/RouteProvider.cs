using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.WxPay
{
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(RouteCollection routes)
        {
            //Notify
            routes.MapRoute("Plugin.Payments.WxPay.Notify",
                 "Plugins/WxPay/Notify",
                 new { controller = "WxPay", action = "Notify" },
                 new[] { "Nop.Plugin.Payments.WxPay.Controllers" }
            );
            //Return
            routes.MapRoute("Plugin.Payments.WxPay.ReturnInfo",
                 "Plugins/WxPay/ReturnInfo/id",
                 new { controller = "WxPay", action = "ReturnInfo", id = "" },
                 new[] { "Nop.Plugin.Payments.WxPay.Controllers" }
            );
            //MakeQRCode
            routes.MapRoute("Plugin.Payments.WxPay.MakeQRCode",
                "Plugins/WxPay/MakeQRCode",
                new { controller = "WxPay", action = "MakeQRCode" },
                new[] { "Nop.Plugin.Payments.WxPay.Controllers" }
           );
            //retule
            routes.MapRoute("Plugin.Payments.WxPay.IsPaid",
                "Plugins/WxPay/IsPaid",
                new { controller = "WxPay", action = "IsPaid"},
                new[] { "Nop.Plugin.Payments.WxPay.Controllers" }
           );
        }

        #endregion

        #region Properties

        public int Priority
        {
            get
            {
                return 0;
            }
        }

        #endregion
    }
}
