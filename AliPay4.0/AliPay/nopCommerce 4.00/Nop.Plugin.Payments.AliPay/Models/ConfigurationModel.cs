using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Payments.AliPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.AliPay.SellerEmail")]
        public string SellerEmail { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliPay.Key")]
        public string Key { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliPay.Partner")]
        public string Partner { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
    }
}