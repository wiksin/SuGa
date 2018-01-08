using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;
using DaBoLang.Nop.Plugin.Payments.AliPay.Models;
using Com.Alipay;
using DaBoLang.Nop.Plugin.Payments.AliPay.Services;
using DaBoLang.Nop.Plugin.Payments.AliPay.Domain;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using Nop.Services.Messages;
using Nop.Core.Domain.Localization;
using Nop.Services.Stores;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Controllers
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay.Controllers
    /// 名    称：AliPayController
    /// 功    能：控制器
    /// 详    细：插件控制器
    /// 版    本：1.0.0.0
    /// 创建时间：2017-08-02 01:37
    /// 修改时间：2017-08-04 01:26
    /// 修改时间：time
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：插件配置、插件显示、支付请求通知、支付宝跳转页面、退款请求通知
    /// 
    /// </summary>
    public class AliPayController : BasePaymentController
    {
        #region 属性

        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPaymentInfoService _paymentInfoService;
        private readonly IRefundInfoService _refundInfoService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        #endregion

        #region 构造

        public AliPayController(ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            ILocalizationService localizationService,
            PaymentSettings paymentSettings,
            IPaymentInfoService paymentInfoService,
            IRefundInfoService refundInfoService,
            IEventPublisher eventPublisher,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._localizationService = localizationService;
            this._paymentSettings = paymentSettings;
            this._paymentInfoService = paymentInfoService;
            this._refundInfoService = refundInfoService;
            this._eventPublisher = eventPublisher;
            this._workflowMessageService = workflowMessageService;
            this._localizationSettings = localizationSettings;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
        }

        #endregion

        #region 基础请求

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var aliPayPaymentSettings = _settingService.LoadSetting<AliPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                SellerEmail = aliPayPaymentSettings.SellerEmail,
                Key = aliPayPaymentSettings.Key,
                Partner = aliPayPaymentSettings.Partner,
                AdditionalFee = aliPayPaymentSettings.AdditionalFee,
                ActiveStoreScopeConfiguration = storeScope,
        };
            if (storeScope > 0)
            {
                model.SellerEmail_OverrideForStore = _settingService.SettingExists(aliPayPaymentSettings, x => x.SellerEmail, storeScope);
                model.Key_OverrideForStore = _settingService.SettingExists(aliPayPaymentSettings, x => x.Key, storeScope);
                model.Partner_OverrideForStore = _settingService.SettingExists(aliPayPaymentSettings, x => x.Partner, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(aliPayPaymentSettings, x => x.AdditionalFee, storeScope);
            }
            return View("~/Plugins/DaBoLang.Payments.AliPay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var aliPayPaymentSettings = _settingService.LoadSetting<AliPayPaymentSettings>(storeScope);

            //save settings
            aliPayPaymentSettings.SellerEmail = model.SellerEmail;
            aliPayPaymentSettings.Key = model.Key;
            aliPayPaymentSettings.Partner = model.Partner;
            aliPayPaymentSettings.AdditionalFee = model.AdditionalFee;

            _settingService.SaveSettingOverridablePerStore(aliPayPaymentSettings, x => x.SellerEmail, model.SellerEmail_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aliPayPaymentSettings, x => x.Key, model.Key_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aliPayPaymentSettings, x => x.Partner, model.Partner_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aliPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/DaBoLang.Payments.AliPay/Views/PaymentInfo.cshtml");
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
        #endregion

        #region 支付请求
        /// <summary>
        /// 接收支付宝支付通知
        /// </summary>
        [ValidateInput(false)]
        public ActionResult Notify(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("DaBoLang.Payments.AliPay") as AliPayPaymentProcessor;

            if (processor == null || !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("插件无法加载");
            var aliPayPaymentSettings = _settingService.LoadSetting<AliPayPaymentSettings>(_storeContext.CurrentStore.Id);
            var partner = aliPayPaymentSettings.Partner;

            if (string.IsNullOrEmpty(partner))
                throw new Exception("合作身份者ID 不能为空");

            var key = aliPayPaymentSettings.Key;

            if (string.IsNullOrEmpty(key))
                throw new Exception("MD5密钥不能为空");

            var sellerEmail = aliPayPaymentSettings.SellerEmail;

            if (string.IsNullOrEmpty(sellerEmail))
                throw new Exception("卖家Email 不能为空");

            ///↓↓↓↓↓↓↓ 获取支付宝POST过来通知消息，并以“参数名 = 参数值”的形式组成数组↓↓↓↓↓↓↓↓
            int i;
            var coll = Request.Form;
            var sortedStr = coll.AllKeys;

            SortedDictionary<string, string> sPara = new SortedDictionary<string, string>();
            for (i = 0; i < sortedStr.Length; i++)
            {
                sPara.Add(sortedStr[i], Request.Form[sortedStr[i]]);
            }
            ///↑↑↑↑↑↑↑ 获取支付宝POST过来通知消息，并以“参数名 = 参数值”的形式组成数组↑↑↑↑↑↑↑↑
            if (sPara.Count > 0)//判断是否有带返回参数
            {
                AlipayNotify aliNotify = new AlipayNotify(partner:partner, key:key,input_charset:"utf-8",sign_type: sPara["sign_type"]);
                var sign = Request.Form["sign"];
                var notify_id = Request.Form["notify_id"];
                bool verifyResult = aliNotify.Verify(sPara, notify_id, sign);
                if (verifyResult)//验证成功
                {
                    //商户订单号

                    string out_trade_no = Request.Form["out_trade_no"];

                    //支付宝交易号

                    string trade_no = Request.Form["trade_no"];

                    //交易状态
                    string trade_status = Request.Form["trade_status"];


                    if (coll["trade_status"] == "TRADE_FINISHED" || coll["trade_status"] == "TRADE_SUCCESS")
                    {
                        int orderId;

                        if (int.TryParse(out_trade_no, out orderId))
                        {
                            var order = _orderService.GetOrderById(orderId);

                            if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                            {
                                //修改订单状态
                                _orderProcessingService.MarkOrderAsPaid(order);
                                //添加付款信息
                                var  paymentInfo = new PaymentInfo()
                                {
                                    OrderId=orderId,
                                   Name = processor.PluginDescriptor.SystemName,
                                   PaymentGuid = Guid.NewGuid(),
                                   Trade_no = trade_no,
                                   Total = decimal.Parse(Request.Form["price"]),
                                   Trade_status = Request.Form["trade_status"],
                                   Buyer_email = Request.Form["buyer_email"],
                                   Buyer_id = Request.Form["buyer_id"],
                                   Seller_email = Request.Form["seller_email"],
                                   Seller_id = Request.Form["seller_id"],
                                   Note = Request.Form["subject"],
                                   Out_Trade_No = Request.Form["trade_no"],
                                   CreateDateUtc = DateTime.Now,
                               };
                                _paymentInfoService.Insert(paymentInfo);
                            }
                        }
                    }
                    Response.Write("success");  //请不要修改或删除
                }
                else //验证失败
                {
                    Response.Write("fail");

                    var logStr = string.Format("MD5:notify_id={0},sign={1}", notify_id, sign);

                    _logger.Error(logStr);
                }
            }
            return Content("无通知参数");
        }
        /// <summary>
        /// 支付页面跳转同步通知页面
        /// </summary>
        [ValidateInput(false)]
        public ActionResult Return()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("DaBoLang.Payments.AliPay") as AliPayPaymentProcessor;

            if (processor == null || !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("AliPay module cannot be loaded");

            return RedirectToAction("Index", "Home", new { area = "" });
        }
        #endregion

        #region 订单退款
        [ValidateInput(false)]
        public ActionResult RefundNotify(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("DaBoLang.Payments.AliPay") as AliPayPaymentProcessor;
            
            if (processor == null || !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("插件无法加载");
            var aliPayPaymentSettings = _settingService.LoadSetting<AliPayPaymentSettings>(_storeContext.CurrentStore.Id);

            var partner = aliPayPaymentSettings.Partner;

            if (string.IsNullOrEmpty(partner))
                throw new Exception("合作身份者ID 不能为空");

            var key = aliPayPaymentSettings.Key;

            if (string.IsNullOrEmpty(key))
                throw new Exception("MD5密钥不能为空");

            var sellerEmail = aliPayPaymentSettings.SellerEmail;

            if (string.IsNullOrEmpty(sellerEmail))
                throw new Exception("卖家Email 不能为空");

            ///↓↓↓↓↓↓↓ 获取支付宝POST过来通知消息，并以“参数名 = 参数值”的形式组成数组↓↓↓↓↓↓↓↓
            int i;
            var coll = Request.Form;
            var sortedStr = coll.AllKeys;
            SortedDictionary<string, string> sPara = new SortedDictionary<string, string>();
            for (i = 0; i < sortedStr.Length; i++)
            {
                sPara.Add(sortedStr[i], Request.Form[sortedStr[i]]);
            }
            ///↑↑↑↑↑↑↑ 获取支付宝POST过来通知消息，并以“参数名 = 参数值”的形式组成数组↑↑↑↑↑↑↑↑
            if (sPara.Count > 0)//判断是否有带返回参数
            {
                AlipayNotify aliNotify = new AlipayNotify(partner: partner, key: key, input_charset: "utf-8", sign_type: sPara["sign_type"]);
                var notify_type = Request.Form["notify_type"];
                var notify_id = Request.Form["notify_id"];
                var sign = Request.Form["sign"];
                bool verifyResult = aliNotify.Verify(sPara, notify_id, sign);

                if (verifyResult)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////                    

                    //批次号

                    string batch_no = Request.Form["batch_no"];

                    //批量退款数据中转账成功的笔数

                    string success_num = Request.Form["success_num"];

                    //批量退款数据中的详细信息
                    string result_details = Request.Form["result_details"];

                    //↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓业务处理↓↓↓↓↓↓↓↓↓↓↓↓↓↓
                    try
                    {
                        string create_time = batch_no.Substring(0, 8);
                        var refundInfo = _refundInfoService.GetRefundInfoByBatch_no(batch_no);
                        if (refundInfo != null && refundInfo.OrderId > 0)
                        {

                            if (refundInfo.RefundStatus == RefundStatus.refund || notify_id == refundInfo.Notify_Id)
                            {
                                Response.Write("success");
                                return Content("success");
                            }

                            var result_list = result_details.Split('#');
                            var item = result_list[0];

                            refundInfo.Notify_Id = notify_id;
                            refundInfo.Notify_Type = notify_type;

                            var obj = item.Split('^');
                            var out_Trade_No = obj[0];//交易号
                            var AmountToRefund = decimal.Parse(obj[1]);//退款金额
                            var note = obj[2];//退款说明

                            var order = _orderService.GetOrderById(refundInfo.OrderId);
                            var paymentInfo = _paymentInfoService.GetByOrderId(refundInfo.OrderId);
                            if (order != null)
                            {
                                if (note.ToUpper() == "SUCCESS")
                                {

                                    if (AmountToRefund >= 0 && AmountToRefund == refundInfo.AmountToRefund)
                                    {
                                        #region 成功
                                        order.OrderNotes.Add(new OrderNote
                                        {
                                            Note = string.Format("支付宝退款成功,退款编号:{0},退款金额:{1},交易号:{2},说明:{3}", batch_no, AmountToRefund, out_Trade_No, note),
                                            DisplayToCustomer = true,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });

                                        ////总退款
                                        decimal totalAmountRefunded = Math.Abs(order.RefundedAmount) + AmountToRefund;

                                        order.RefundedAmount = totalAmountRefunded;

                                        if (paymentInfo.Total > order.RefundedAmount)
                                        {
                                            order.PaymentStatus = PaymentStatus.PartiallyRefunded;
                                        }
                                        else
                                        {
                                            order.PaymentStatus = PaymentStatus.Refunded;
                                        }

                                        _orderService.UpdateOrder(order);

                                        ///改变订单状态
                                        _orderProcessingService.CheckOrderStatus(order);

                                        //修改退款记录为退款成功
                                        refundInfo.RefundStatusId = (int)RefundStatus.refund;
                                        refundInfo.RefundOnUtc = DateTime.Now;
                                        refundInfo.Result_Details = result_details;
                                        _refundInfoService.Update(refundInfo);

                                        ///通知
                                        var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, AmountToRefund, _localizationSettings.DefaultAdminLanguageId);
                                        if (orderRefundedStoreOwnerNotificationQueuedEmailId > 0)
                                        {
                                            order.OrderNotes.Add(new OrderNote
                                            {
                                                Note = string.Format("\"订单退款\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                                                DisplayToCustomer = false,
                                                CreatedOnUtc = DateTime.UtcNow
                                            });
                                            _orderService.UpdateOrder(order);
                                        }
                                        var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, AmountToRefund, order.CustomerLanguageId);
                                        if (orderRefundedCustomerNotificationQueuedEmailId > 0)
                                        {
                                            order.OrderNotes.Add(new OrderNote
                                            {
                                                Note = string.Format("\"订单退款\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                                                DisplayToCustomer = false,
                                                CreatedOnUtc = DateTime.UtcNow
                                            });
                                            _orderService.UpdateOrder(order);
                                        }

                                        //已退款事件   
                                        _eventPublisher.Publish(new OrderRefundedEvent(order, AmountToRefund));
                                        Response.Write("success");
                                        return Content("success");
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 错误
                                        //退款异常
                                        refundInfo.RefundStatusId = (int)RefundStatus.error;
                                        _refundInfoService.Update(refundInfo);
                                        order.OrderNotes.Add(new OrderNote
                                        {
                                            Note = string.Format("支付宝退款异常,退款编号:{0},退款金额:{1},交易号:{2},说明:{3}", batch_no, AmountToRefund, out_Trade_No, "退款金额错误"),
                                            DisplayToCustomer = false,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });
                                        _orderService.UpdateOrder(order);
                                        Response.Write("success");
                                        return Content("success");
                                        #endregion
                                    }

                                }
                            }
                           
                        }
                        throw new Exception(string.Format("支付宝退款通知异常,退款编号:{0},退款金额:{1},交易号:{2},说明:{3}", batch_no, refundInfo.AmountToRefund, refundInfo.Out_Trade_No, "非正常处理"));
                    }
                    catch (Exception ex)
                    {
                        Response.Write("fail");
                        _logger.Error(ex.Message);
                    }
                    //↑↑↑↑↑↑↑↑↑↑↑↑↑↑业务处理↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
                    ///结束业务处理
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

        #endregion
    }
}