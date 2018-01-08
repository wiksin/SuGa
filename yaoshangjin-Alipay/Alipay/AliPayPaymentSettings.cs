using Nop.Core.Configuration;

namespace DaBoLang.Nop.Plugin.Payments.AliPay
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay
    /// 名    称：AliPayPaymentSettings
    /// 功    能：配置类
    /// 详    细：支付宝配置
    /// 版    本：1.0.0.0
    /// 文件名称：AliPayPaymentSettings.cs
    /// 创建时间：2017-08-02 01:26
    /// 修改时间：2017-08-04 01:52
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public class AliPayPaymentSettings : ISettings
    {
        /// <summary>
        /// 卖家Email
        /// </summary>
        public string SellerEmail { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// PID
        /// </summary>
        public string Partner { get; set; }
        /// <summary>
        /// 额外费用
        /// </summary>
        public decimal AdditionalFee { get; set; }
    }
}
