using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Models
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay.Models
    /// 名    称：ConfigurationModel
    /// 功    能：模型类
    /// 详    细：插件配置类
    /// 版    本：1.0.0.0
    /// 文件名称：ConfigurationModel.cs
    /// 创建时间：2017-08-02 01:38
    /// 修改时间：2017-08-04 01:32
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("DaBoLang.Plugins.Payments.AliPay.SellerEmail")]
        public string SellerEmail { get; set; }
        public bool SellerEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("DaBoLang.Plugins.Payments.AliPay.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }
        [NopResourceDisplayName("DaBoLang.Plugins.Payments.AliPay.Partner")]
        public string Partner { get; set; }
        public bool Partner_OverrideForStore { get; set; }
        [NopResourceDisplayName("DaBoLang.Plugins.Payments.AliPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}