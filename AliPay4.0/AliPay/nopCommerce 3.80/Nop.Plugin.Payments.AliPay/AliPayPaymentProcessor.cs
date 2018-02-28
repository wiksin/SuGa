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
        internal string GetMD5(string input, string inputCharset)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(inputCharset).GetBytes(input));
            var sb = new StringBuilder(32);

            foreach (var b in t)
            {
                sb.AppendFormat("{0:X}", b);
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
            var result = new ProcessPaymentResult {NewPaymentStatus = PaymentStatus.Pending};

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //string gateway = "https://www.alipay.com/cooperate/gateway.do?";

            var sellerEmail = _aliPayPaymentSettings.SellerEmail;
            var key = _aliPayPaymentSettings.Key;
            var partner = _aliPayPaymentSettings.Partner;
            var outTradeNo = postProcessPaymentRequest.Order.Id.ToString();
            var subject = _storeContext.CurrentStore.Name;
            var body = "Order from " + _storeContext.CurrentStore.Name;
            var totalFee = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture);
            var notifyUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentAliPay/Notify";
            var returnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentAliPay/Return";

            string[] para ={
                               "service=" + Service,
                               "partner=" + partner,
                               "seller_email=" + sellerEmail,
                               "out_trade_no=" + outTradeNo,
                               "subject=" + subject,
                               "body=" + body,
                               "total_fee=" + totalFee,
                               "show_url=" + ShowUrl,
                               "payment_type=1",
                               "notify_url=" + notifyUrl,
                               "return_url=" + returnUrl,
                               "_input_charset=" + InputCharset
                           };

            var aliayUrl = CreatUrl(para, InputCharset, key);

            var post = new RemotePost
            {
                FormName = "alipaysubmit",
                Url = "https://www.alipay.com/cooperate/gateway.do?_input_charset=utf-8",
                Method = "POST"
            };

            post.Add("service", Service);
            post.Add("partner", partner);
            post.Add("seller_email", sellerEmail);
            post.Add("out_trade_no", outTradeNo);
            post.Add("subject", subject);
            post.Add("body", body);
            post.Add("total_fee", totalFee);
            post.Add("show_url", ShowUrl);
            post.Add("return_url", returnUrl);
            post.Add("notify_url", notifyUrl);
            post.Add("payment_type", "1");
            post.Add("sign", aliayUrl);
            post.Add("sign_type", SignType);

            post.Post();
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
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _aliPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            result.AddError("Capture method not supported");

            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            result.AddError("Refund method not supported");

            return result;
        }

        /// <summary>
        /// Voids a payment
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
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();

            result.AddError("Recurring payment not supported");

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //AliPay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice
            
            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            return !((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1);
        }

        /// <summary>
        /// Gets a route for provider configuration
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
        /// Gets a route for payment info
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
                Partner= "",
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
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        #endregion
    }
}
