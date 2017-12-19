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
using Com.Alipay;
using System.Collections.Specialized;
using Nop.Services.Customers;

namespace Nop.Plugin.Payments.AliPay.Controllers
{
    public class PaymentAliPayController : BasePaymentController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
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
            ICustomerService customerService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            ILocalizationService localizationService,
            AliPayPaymentSettings aliPayPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._customerService = customerService;
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
            #region 老版本备份
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
            #endregion

            SortedDictionary<string, string> sPara = GetRequestPost();

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify(partner, key, _input_charset, Config.sign_type);
                bool verifyResult = aliNotify.Verify(sPara, Request.Form["notify_id"], Request.Form["sign"]);

                if (verifyResult)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //请在这里加上商户的业务逻辑程序代码


                    //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                    //获取支付宝的通知返回参数，可参考技术文档中服务器异步通知参数列表

                    //商户订单号

                    string out_trade_no = Request.Form["out_trade_no"];

                    //支付宝交易号

                    string trade_no = Request.Form["trade_no"];

                    //交易状态
                    string trade_status = Request.Form["trade_status"];


                    switch (trade_status)
                    {
                        case "TRADE_FINISHED":
                        case "TRADE_SUCCESS":
                        case "TRADE_HAS_SUCCESS":
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



                    //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                    Response.Write("success");  //请不要修改或删除

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                else//验证失败
                {
                    Response.Write("fail");
                }
            }
            else
            {
                Response.Write("无通知参数");
            }











            return Content("");



        }

        [ValidateInput(false)]
        public ActionResult Return()
        {
            //var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.AliPay") as AliPayPaymentProcessor;

            //if (processor == null 
            //    || !processor.IsPaymentMethodActive(_paymentSettings) 
            //    || !processor.PluginDescriptor.Installed)
            //    throw new NopException("AliPay module cannot be loaded");

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


            SortedDictionary<string, string> sPara = GetRequestGet();

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify(partner, key, _input_charset, Config.sign_type);
                bool verifyResult = aliNotify.Verify(sPara, Request.QueryString["notify_id"], Request.QueryString["sign"]);

                if (verifyResult)//验证成功
                {

                    //商户订单号

                    string out_trade_no = Request.QueryString["out_trade_no"];

                    //支付宝交易号

                    string trade_no = Request.QueryString["trade_no"];

                    //交易状态
                    string trade_status = Request.QueryString["trade_status"];

                    switch (trade_status)
                    {
                        case "TRADE_FINISHED":
                        case "TRADE_SUCCESS":
                        case "TRADE_HAS_SUCCESS":
                            {
                                var strOrderNo = Request.QueryString["out_trade_no"];
                                var strPrice = Request.QueryString["total_fee"];
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

                    //打印页面
                    Response.Write("验证成功<br />");

                    //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                else//验证失败
                {
                    Response.Write("验证失败");
                }
            }
            else
            {
                Response.Write("无返回参数");
            }








            return RedirectToAction("Index", "Home", new { area = "" });
        }

        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }
        /// <summary>
        /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestGet()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }

            return sArray;
        }


        #endregion
    }
}