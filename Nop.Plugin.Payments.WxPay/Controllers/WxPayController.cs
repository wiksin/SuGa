using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Payments;
using System.Web.Mvc;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Logging;
using Nop.Services.Localization;
using Nop.Plugin.Payments.WxPay.Models;
using ThoughtWorks.QRCode.Codec;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Nop.Plugin.Payments.WxPay.app_code;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Payments.WxPay.Controllers
{
    public class WxPayController : BasePaymentController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly WxPayPaymentSettings _wxPayPaymentSettings;
        private readonly PaymentSettings _paymentSettings;


        #endregion

        #region Ctor

        public WxPayController(ISettingService settingService,
            ICustomerService customerService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            ILocalizationService localizationService,
            WxPayPaymentSettings aliPayPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._customerService = customerService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._localizationService = localizationService;
            this._wxPayPaymentSettings = aliPayPaymentSettings;
            this._paymentSettings = paymentSettings;
        }

        #endregion

        #region 方法

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                APPID = _wxPayPaymentSettings.APPID,
                MCHID = _wxPayPaymentSettings.MCHID,
                KEY = _wxPayPaymentSettings.KEY,
                APPSECRET = _wxPayPaymentSettings.APPSECRET,
                SSLCERT_PATH = _wxPayPaymentSettings.SSLCERT_PATH,
                AdditionalFee = _wxPayPaymentSettings.AdditionalFee,
                VendorId = _wxPayPaymentSettings.VendorId,
                SSLCERT_PASSWORD = _wxPayPaymentSettings.SSLCERT_PASSWORD

            };
            return View("~/Plugins/Payments.WxPay/Views/WxPay/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(Models.ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _wxPayPaymentSettings.APPID = model.APPID;
            _wxPayPaymentSettings.MCHID = model.MCHID;
            _wxPayPaymentSettings.KEY = model.KEY;
            _wxPayPaymentSettings.APPSECRET = model.APPSECRET;

            _settingService.SaveSetting(_wxPayPaymentSettings);

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.WxPay/Views/WxPay/PaymentInfo.cshtml");
        }
        public FileResult MakeQRCode(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("data");

            //初始化二维码生成工具
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeVersion = 0;
            qrCodeEncoder.QRCodeScale = 4;

            //将字符串生成二维码图片
            Bitmap image = qrCodeEncoder.Encode(data, Encoding.Default);

            //保存为jpeg到内存流  
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Jpeg);
            return File(ms.ToArray(), "image/jpeg");
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
            //接收从微信后台POST过来的数据
            Stream s = Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

            //Logger.Info(this.GetType() + "Receive data from WeChat : " + builder);
            //转换数据格式并验证签名
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (WxPayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Log.Error(this.GetType().ToString(), "Sign check error : " + res.ToXml());
                Response.Write(res.ToXml());
                Response.End();
            }
            ProcessNotify(data);
            return View();
        }
        public void ProcessNotify(WxPayData data)
        {
            WxPayData notifyData = data;

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                //Logger.Error(this.GetType() + "The Pay result is error : " + res.ToXml());
                Response.Write(res.ToXml());
                Response.End();
            }
            else
            {
                string transaction_id = notifyData.GetValue("transaction_id").ToString();
                //查询订单，判断订单真实性
                //if (!QueryOrder(transaction_id))
                //{
                //    //若订单查询失败，则立即返回结果给微信支付后台
                //    WxPayData res = new WxPayData();
                //    res.SetValue("return_code", "FAIL");
                //    res.SetValue("return_msg", "订单查询失败");
                //    //Logger.Error(this.GetType() + "Order query failure : " + res.ToXml());
                //    SetPaymentResult(data.GetValue("out_trade_no").ToString(), PaymentStatus.Paid, transaction_id);
                //    Response.Write(res.ToXml());
                //    Response.End();
                //}
                ////查询订单成功
                //else
                //{
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                //Logger.Info(this.GetType() + "order query success : " + res.ToXml());
                SetPaymentResult(data.GetValue("out_trade_no").ToString(), PaymentStatus.Paid, transaction_id);
                Response.Write(res.ToXml());
                Response.End();
            }

            // }
        }
        [ValidateInput(false)]
        public int IsPaid()
        {
            var id = Request.QueryString["id"];
            var order = _orderService.GetOrderById(Convert.ToInt32(id));
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return 1;
            }
            return -1;
        }
        //查询订单
        private bool QueryOrder(string transaction_id)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = new WxPayApi().OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 设置订单支付状态
        /// </summary>
        /// <param name="tradeno"></param>
        /// <param name="status"></param>
        /// <param name="transaction_id"></param>
        /// <returns></returns>
        public ActionResult SetPaymentResult(string tradeno, PaymentStatus status, string transaction_id)
        {
            //Logger.Info("订单号:" + tradeno);
            var order = _orderService.GetOrderById(Convert.ToInt32(tradeno));
            if (order != null)
            {
                order.PaymentStatus = status;
                if (status == PaymentStatus.Paid)
                {
                    order.PaidDateUtc = DateTime.Now;
                }
                // order.OrderStatus=status;
                order.WxTransactionId = transaction_id;
                _orderService.UpdateOrder(order);
                //Logger.Info("订单：" + tradeno + "成功更新状态为" + status);
            }
            return Redirect("/checkout/completed/" + tradeno);
        }
        [ValidateInput(false)]
        public ActionResult Return(int id)
        {
            ViewBag.orderId = id;
            return View(id);
        }
        #endregion
    }
}
