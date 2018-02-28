using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.AliPay.Components
{
    [ViewComponent(Name = "PaymentAliPay")]
    public class PaymentAliPayViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.AliPay/Views/PaymentInfo.cshtml");
        }
    }
}
