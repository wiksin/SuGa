using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AliPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

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

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                SellerEmail = _aliPayPaymentSettings.SellerEmail,
                Key = _aliPayPaymentSettings.Key,
                Partner = _aliPayPaymentSettings.Partner,
                AdditionalFee = _aliPayPaymentSettings.AdditionalFee
            };

            return View("~/Plugins/Payments.AliPay/Views/PaymentAliPay/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
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

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.AliPay/Views/PaymentAliPay/PaymentInfo.cshtml");
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();

            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult Notify(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AliPay") as AliPayPaymentProcessor;

            if (processor == null 
                || !processor.IsPaymentMethodActive(_paymentSettings) 
                || !processor.PluginDescriptor.Installed)
                throw new NopException("AliPay module cannot be loaded");

            var partner = _aliPayPaymentSettings.Partner;

            if (string.IsNullOrEmpty(partner))
                throw new Exception("Partner is not set");

            var key = _aliPayPaymentSettings.Key;

            if (string.IsNullOrEmpty(key))
                throw new Exception("Partner is not set");

            var _input_charset = "utf-8";

            var alipayNotifyUrl = string.Format("https://www.alipay.com/cooperate/gateway.do?service=notify_verify&partner={0}&notify_id={1}", partner, Request.Form["notify_id"]);

            var responseTxt = processor.GetHttp(alipayNotifyUrl, 120000);

            int i;
            var coll = Request.Form;
            var sortedStr = coll.AllKeys;

            Array.Sort(sortedStr, StringComparer.InvariantCulture);

            var prestr = new StringBuilder();

            for (i = 0; i < sortedStr.Length; i++)
            {
                if (coll[sortedStr[i]] != "" && sortedStr[i] != "sign" && sortedStr[i] != "sign_type")
                {
                    prestr.AppendFormat("{0}={1}", sortedStr[i], coll[sortedStr[i]]);

                    if (i < sortedStr.Length - 1)
                    {
                        prestr.Append("&");
                    }
                }
            }

            prestr.Append(key);

            var mySign = processor.GetMD5(prestr.ToString(), _input_charset);

            var sign = coll["sign"];

            if (mySign == sign && responseTxt == "true")
            {
                switch (coll["trade_status"])
                {
                    case "WAIT_BUYER_PAY":
                    {
                        var strOrderNo = coll["out_trade_no"];
                        var strPrice = coll["total_fee"];
                    }
                        break;
                    case "TRADE_FINISHED":
                    case "TRADE_SUCCESS":
                    {
                        var strOrderNo = Request.Form["out_trade_no"];
                        var strPrice = Request.Form["total_fee"];
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
                        break;
                }

                Response.Write("success");
            }
            else
            {
                Response.Write("fail");

                var logStr = string.Format("MD5:mysign={0},sign={1},responseTxt={2}", mySign, sign, responseTxt);

                _logger.Error(logStr);
            }

            return Content("");
        }

        [ValidateInput(false)]
        public ActionResult Return()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AliPay") as AliPayPaymentProcessor;

            if (processor == null 
                || !processor.IsPaymentMethodActive(_paymentSettings) 
                || !processor.PluginDescriptor.Installed)
                throw new NopException("AliPay module cannot be loaded");

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        #endregion
    }
}