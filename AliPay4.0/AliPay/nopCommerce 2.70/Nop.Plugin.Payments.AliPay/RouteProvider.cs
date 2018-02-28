using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.AliPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.AliPay.Configure",
                 "Plugins/PaymentAliPay/Configure",
                 new { controller = "PaymentAliPay", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.AliPay.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.AliPay.PaymentInfo",
                 "Plugins/PaymentAliPay/PaymentInfo",
                 new { controller = "PaymentAliPay", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.AliPay.Controllers" }
            );

            //Notify
            routes.MapRoute("Plugin.Payments.AliPay.Notify",
                 "Plugins/PaymentAliPay/Notify",
                 new { controller = "PaymentAliPay", action = "Notify" },
                 new[] { "Nop.Plugin.Payments.AliPay.Controllers" }
            );

            //Notify
            routes.MapRoute("Plugin.Payments.AliPay.Return",
                 "Plugins/PaymentAliPay/Return",
                 new { controller = "PaymentAliPay", action = "Return" },
                 new[] { "Nop.Plugin.Payments.AliPay.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
