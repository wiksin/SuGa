using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.AliPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Com.Alipay;
using System.Web;
using System.Net.Http;

namespace Nop.Plugin.Payments.AliPay
{
    /// <summary>
    /// AliPay payment processor
    /// </summary>
    public class AliPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Constants

        private const string ShowUrl = "http://www.alipay.com/";
        private const string Service = "create_direct_pay_by_user";
        private const string SignType = "MD5";
        private const string InputCharset = "utf-8";

        #endregion

        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly AliPayPaymentSettings _aliPayPaymentSettings;

        #endregion

        #region Ctor

        public AliPayPaymentProcessor(
            ISettingService settingService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            AliPayPaymentSettings aliPayPaymentSettings)
        {
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            this._aliPayPaymentSettings = aliPayPaymentSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets MD5 hash
        /// </summary>
        /// <param name="input">Input</param>
        /// <param name="inputCharset">Input charset</param>
        /// <returns>Result</returns>
        internal string GetMD5(string prestr, string inputCharset)
        {
            StringBuilder sb = new StringBuilder(32);

            //prestr = prestr + _aliPayPaymentSettings.Key;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(inputCharset).GetBytes(prestr));
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create URL
        /// </summary>
        /// <param name="para">Para</param>
        /// <param name="inputCharset">Input charset</param>
        /// <param name="key">Key</param>
        /// <returns>Result</returns>
        private string CreatUrl(string[] para, string inputCharset, string key)
        {
            Array.Sort(para, StringComparer.InvariantCulture);

            int i;
            var prestr = new StringBuilder();

            for (i = 0; i < para.Length; i++)
            {
                prestr.Append(para[i]);

                if (i < para.Length - 1)
                {
                    prestr.Append("&");
                }
            }

            prestr.Append(key);

            var sign = GetMD5(prestr.ToString(), inputCharset);

            return sign;
        }

        /// <summary>
        /// Gets HTTP
        /// </summary>
        /// <param name="strUrl">Url</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Result</returns>
        internal string GetHttp(string strUrl, int timeout)
        {
            var strResult = string.Empty;

            try
            {
                var myReq = (HttpWebRequest)WebRequest.Create(strUrl);

                myReq.Timeout = timeout;

                var httpWResp = (HttpWebResponse)myReq.GetResponse();
                var myStream = httpWResp.GetResponseStream();
                if (myStream != null)
                {
                    using (var sr = new StreamReader(myStream, Encoding.Default))
                    {
                        var strBuilder = new StringBuilder();

                        while (-1 != sr.Peek())
                        {
                            strBuilder.Append(sr.ReadLine());
                        }

                        strResult = strBuilder.ToString();
                    }
                }
            }
            catch (Exception exc)
            {
                strResult = string.Format("Error: {0}", exc.Message);
            }

            return strResult;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending };

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var sellerEmail = _aliPayPaymentSettings.SellerEmail;
            var key = _aliPayPaymentSettings.Key;
            var partner = _aliPayPaymentSettings.Partner;
            var outTradeNo = postProcessPaymentRequest.Order.Id.ToString();
            var subject = _storeContext.CurrentStore.Name;
            var body = "Order from " + _storeContext.CurrentStore.Name;
            var totalFee = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture);
            var notifyUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentAliPay/Notify";
            var returnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentAliPay/Return";
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("service", Service);
            sParaTemp.Add("partner", partner);
            sParaTemp.Add("seller_id", partner);
            sParaTemp.Add("_input_charset", Config.input_charset.ToLower());
            sParaTemp.Add("payment_type", Config.payment_type);
            sParaTemp.Add("notify_url", notifyUrl);
            sParaTemp.Add("return_url", returnUrl);
            sParaTemp.Add("anti_phishing_key", Config.anti_phishing_key);
            sParaTemp.Add("exter_invoke_ip", Config.exter_invoke_ip);
            sParaTemp.Add("out_trade_no", outTradeNo);
            sParaTemp.Add("subject", subject);
            sParaTemp.Add("total_fee", totalFee);
            sParaTemp.Add("body", body);
            string sHtmlText = Submit.BuildRequest(sParaTemp, "post", "确认");
            var post = new RemotePost();
            post.Post(sHtmlText);



        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// 获得额外手续费
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _aliPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// 捕捉付款
        /// </summary>
        /// <param name="capturePaymentRequest">捕捉请求支付</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            result.AddError("Capture method not supported");

            return result;
        }

        /// <summary>
        /// 退款支付
        /// </summary>
        /// <param name="refundPaymentRequest">请求</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            result.AddError("Refund method not supported");

            return result;
        }

        /// <summary>
        /// 空的支付
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            result.AddError("Void method not supported");

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            result.AddError("Recurring payment not supported");

            return result;
        }

        /// <summary>
        /// 取消定期付款
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();

            result.AddError("不支持经常性付款");

            return result;
        }

        /// <summary>
        /// 获取一个值，该值指示客户是否可以在订单被放置但未完成后完成付款（重定向支付方法）
        /// </summary>
        /// <param name="order">订单</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("订单");

            //AliPay是重定向支付方法
            //它也验证订单是否也支付（重定向后），这样客户就不能支付两次
            //付款状态应待定
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //让我们确保订单后至少1分钟通过
            return !((DateTime.Now - order.CreatedOnUtc).TotalMinutes < 1);
        }

        /// <summary>
        /// 获取提供者配置的路由
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentAliPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.AliPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// 获取付款信息的路由
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentAliPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.AliPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentAliPayController);
        }

        public override void Install()
        {
            //settings
            var settings = new AliPayPaymentSettings()
            {
                SellerEmail = "",
                Key = "",
                Partner = "",
                AdditionalFee = 0,
            };

            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.RedirectionTip", "You will be redirected to AliPay site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail", "Seller email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail.Hint", "Enter seller email.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Key.Hint", "Enter key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Partner", "Partner");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Partner.Hint", "Enter partner.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.AdditionalFee.Hint", "Enter additional fee to charge your customers.");

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.SellerEmail.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Partner");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Partner.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.AdditionalFee.Hint");

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// 获取一个值，该值指示是否支持捕获
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否支持部分退款
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否支持退款
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 获取一个值，该值指示是否支持空白
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 获取定期付款类型的付款方法
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        ///获取付款方法类型
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// 该值指示是否应显示这个插件支付信息页面
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        #endregion
    }
}
