using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AliPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.AliPay.Controllers
{
    public class PaymentAliPayController : BasePaymentController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly AliPayPaymentSettings _aliPayPaymentSettings;
        private readonly PaymentSettings _paymentSettings;

        #endregion

        #region Ctor

        public PaymentAliPayController(ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger,
            ILocalizationService localizationService,
            AliPayPaymentSettings aliPayPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._localizationService = localizationService;
            this._aliPayPaymentSettings = aliPayPaymentSettings;
            this._paymentSettings = paymentSettings;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                SellerEmail = _aliPayPaymentSettings.SellerEmail,
                Key = _aliPayPaymentSettings.Key,
                Partner = _aliPayPaymentSettings.Partner,
                AdditionalFee = _aliPayPaymentSettings.AdditionalFee
            };

            return View("~/Plugins/Payments.AliPay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _aliPayPaymentSettings.SellerEmail = model.SellerEmail;
            _aliPayPaymentSettings.Key = model.Key;
            _aliPayPaymentSettings.Partner = model.Partner;
            _aliPayPaymentSettings.AdditionalFee = model.AdditionalFee;

            _settingService.SaveSetting(_aliPayPaymentSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
        
        public ActionResult Notify()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AliPay") as AliPayPaymentProcessor;

            if (processor == null || !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("AliPay module cannot be loaded");

            var partner = _aliPayPaymentSettings.Partner;

            if (string.IsNullOrEmpty(partner))
                throw new Exception("Partner is not set");

            var key = _aliPayPaymentSettings.Key;

            if (string.IsNullOrEmpty(key))
                throw new Exception("Partner is not set");

            var alipayNotifyUrl = $"https://www.alipay.com/cooperate/gateway.do?service=notify_verify&partner={partner}&notify_id={Request.Form["notify_id"]}";

            var responseTxt = string.Empty;

            try
            {
                var myReq = (HttpWebRequest)WebRequest.Create(alipayNotifyUrl);
                myReq.Timeout = 120000;

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

                        responseTxt = strBuilder.ToString();
                    }
                }
            }
            catch (Exception exc)
            {
                responseTxt = $"Error: {exc.Message}";
            }

            int i;
            var coll = Request.Form;
            var sortedStr = coll.Keys.ToArray();

            Array.Sort(sortedStr, StringComparer.InvariantCulture);
            var prestr = new StringBuilder();

            for (i = 0; i < sortedStr.Length; i++)
            {
                if (coll[sortedStr[i]] == "" || sortedStr[i] == "sign" || sortedStr[i] == "sign_type")
                    continue;

                prestr.AppendFormat("{0}={1}", sortedStr[i], coll[sortedStr[i]]);

                if (i < sortedStr.Length - 1)
                {
                    prestr.Append("&");
                }
            }

            prestr.Append(key);

            var mySign = processor.GetMD5(prestr.ToString());

            var sign = coll["sign"];

            byte[] data = null;
            if (mySign == sign && responseTxt == "true")
            {
                if (coll["trade_status"] == "TRADE_FINISHED" || coll["trade_status"] == "TRADE_SUCCESS")
                {
                    var strOrderNo = Request.Form["out_trade_no"];
                    int orderId;

                    if (int.TryParse(strOrderNo, out orderId))
                    {
                        var order = _orderService.GetOrderById(orderId);

                        if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                }

                data = Encoding.UTF8.GetBytes("success");
                
            }
            else
            {
                data = Encoding.UTF8.GetBytes("fail");

                var logStr = $"MD5:mysign={mySign},sign={sign},responseTxt={responseTxt}";

                _logger.Error(logStr);
            }
            
            Response.Body.Write(data, 0, data.Length);
            return Content("");
        }

        public ActionResult Return()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AliPay") as AliPayPaymentProcessor;

            if (processor == null || !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("AliPay module cannot be loaded");

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        #endregion
    }
}