using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.AliPay
{
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(RouteCollection routes)
        {
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
