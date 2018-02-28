using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.AliPay
{
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //Notify
            routeBuilder.MapRoute("Plugin.Payments.AliPay.Notify",
                 "Plugins/PaymentAliPay/Notify",
                 new { controller = "PaymentAliPay", action = "Notify" });

            //Notify
            routeBuilder.MapRoute("Plugin.Payments.AliPay.Return",
                 "Plugins/PaymentAliPay/Return",
                 new { controller = "PaymentAliPay", action = "Return" });
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
