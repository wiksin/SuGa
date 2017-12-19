using Nop.Core.Domain.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.WxPay.Models
{
    public partial class ConfigurationModel : BaseNopModel
    {
        //=======【基本信息设置】=====================================
        /* 微信公众号信息配置
        * APPID：绑定支付的APPID（必须配置）
        * MCHID：商户号（必须配置）
        * KEY：商户支付密钥，参考开户邮件设置（必须配置）
        * APPSECRET：公众帐号secert（仅JSAPI支付的时候需要配置）
        */
        [NopResourceDisplayName("Plugins.Payments.WxPay.APPID")]
            
        public string APPID { get; set; }
        [NopResourceDisplayName("Plugins.Payments.WxPay.MCHID")]
        public string MCHID { get; set; }
        [NopResourceDisplayName("Plugins.Payments.WxPay.KEY")]
        public string KEY { get; set; }
        [NopResourceDisplayName("Plugins.Payments.WxPay.APPSECRET")]
        public string APPSECRET { get; set; }
        [NopResourceDisplayName("Plugins.Payments.WxPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        //=======【证书路径设置】===================================== 
        /* 证书路径,注意应该填写绝对路径（仅退款、撤销订单时需要）
         * 
        */
        // APPSECRET
        public string SSLCERT_PATH { get; set; }
        public string SSLCERT_PASSWORD { get; set; }
        /// <summary>
        /// 供应商ID
        /// </summary>
        public string VendorId { get; set; }

        //=======【支付结果通知url】===================================== 
        /* 支付结果通知回调url，用于商户接收支付结果
        */
        public string NOTIFY_URL = "http://shop.qiezipai.com.cn/Plugins/WxPay/Notify";

        //=======【商户系统后台机器IP】===================================== 
        /* 此参数可手动配置也可在程序中自动获取
        */
        public string IP = "8.8.8.8";


        //=======【代理服务器设置】===================================
        /* 默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）
        */
        public string PROXY_URL = "http://10.152.18.220:8080";

        //=======【上报信息配置】===================================
        /* 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报
        */
        public int REPORT_LEVENL = 1;

        //=======【日志级别】===================================
        /* 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息
        */
        public static int LOG_LEVENL = 0;

        public OrderDetails orderDetails { get; set; }

    }
    public partial class OrderDetails
    {
        public int OrderId { get; set; }
        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 产品描述
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// 产品供应商
        /// </summary>
        public string Attach { get; set; }
        /// <summary>
        /// 总价
        /// </summary>
        public string Total_fee { get; set; }
    }
}