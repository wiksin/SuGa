using Nop.Plugin.Payments.WxPay.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ThoughtWorks.QRCode.Codec;

namespace Nop.Plugin.Payments.WxPay.app_code
{
    //生成HTML
    public class NativePayHtml
    {
        static ConfigurationModel WxPayConfig = null;

        public NativePayHtml(ConfigurationModel model)
        {
            WxPayConfig = model;
        }
        public string GetHtmlPay()
        {
            string url2 = new NativePay(WxPayConfig).GetPayUrl();
            string htm = @"<!DOCTYPE html>
                    <html xmlns='http://www.w3.org/1999/xhtml'>
                    <head>
                        <meta http-equiv='content-type' content='text/html;image/gif;charset=utf-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1' />
                        <script src='http://lib.sinaapp.com/js/jquery/1.9.1/jquery-1.9.1.min.js' type='text/javascript'></script>
                        <title>微信支付-扫码支付</title>
                    </head>
                    <body>
                       <input type='hidden' id='out_trade_no' value='" + WxPayConfig.orderDetails.OrderId.ToString() + @"' />
                       <div id='paydiv' style='margin:0 auto; text-align:center;'>
                            <div class='row'>
                                <div class='page'>
                                    <div class='page-header'>
                                        <div class='row header'>
                                            <h2 style='display: block;font-size: 1.5em;-webkit-margin-before: 0.83em;-webkit-margin-after: 0.83em;-webkit-margin-start: 0px;-webkit-margin-end: 0px; font-weight: bold;'><img src='../../Images/logo.png' style='width: 40px;' /> 收银台</h2>
                                        </div>
                                        <div class='row'>
                                            <div class='col-md-8'>
                                                <p>订单编号：" + WxPayConfig.orderDetails.OrderId.ToString() + @"</p>
                                            </div>
                                            <div class='col-md-4 paymoney'>
                                                <h2>应付金额：" + Convert.ToDouble(WxPayConfig.orderDetails.Total_fee.ToString()) / 100 + @"</h2>
                                            </div>
                                        </div>
                                    </div>
                                    <div class='row'>
                                        <div class='col-md-8 payinfo'>
                                            <img src='../../Images/WePayLogo.png' style='height:30px;' />
                                            <img src='../../Images/ButtonLabel.png' style='height:30px;' />
                                            <span style='vertical-align: middle;color: gray;'>亿万用户的选择，更快更安全</span>
                                        </div>
                                        <div class='col-md-4 paymoney'>支付<span style='color:orangered;margin: 0 5px;'>" + Convert.ToDouble(WxPayConfig.orderDetails.Total_fee.ToString()) / 100 + @"</span>元</div>
                                    </div>
                                    <div>
                                        <img src='{0}' style='width: 200px;height: 200px;margin-top: 20px;margin-bottom: 20px;' /><br />
                                        <img src='../../Images/Explain.png'  style='width: 200px;' /><br />
                                    </div>
                                </div>
                            </div>
                        </div>
                          <!--支付完成-->
                          <div id='payComplent' class='page' style='margin:0 auto; text-align:center;display:none;'>
                                <div class='row'>
                                    <img src='../../Images/绿色logo.png' style='height:200px;' />
                                     <div class='row'>
                                        <div class='col-md-12'>
                                            <label style='color:#5cb85c;font-size:18px;'>支付成功</label>
                                        </div>
                                    </div >
                                    <div class='row'>
                                        <div class='col-md-8'>
                                            <p>订单编号：" + WxPayConfig.orderDetails.OrderId.ToString() + @"</p>
                                        </div>
                                        <div class='col-md-4 paymoney'>
                                            <h2>应付金额：" + Convert.ToDouble(WxPayConfig.orderDetails.Total_fee.ToString()) / 100 + @"</h2>
                                        </div>
                                    </div>
                                    <div class='row'>
                                        <div class='col-md-12'>
                                            <a href='/orderdetails/" + WxPayConfig.orderDetails.OrderId.ToString() + @"' id='okbutton' style='color: #fff;background-color: #5cb85c;border-color: #4cae4c;display: inline-block;padding: 6px 12px;
                                                margin-bottom: 0;
                                                font-size: 14px;
                                                font-weight: 400;
                                                line-height: 1.42857143;
                                                text-align: center;
                                                white-space: nowrap;
                                                vertical-align: middle;
                                                touch-action: manipulation;
                                                cursor: pointer;
                                                user-select: none;
                                                background-image: none;
                                                border: 1px solid transparent;
                                                border-radius: 4px;
                                                text-transform: none;
                                                text-decoration:none;'>支付完成</a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                    </body>
                   </html>";

            string src = "/Plugins/WxPay/MakeQRCode?data=" + HttpUtility.UrlEncode(url2);
            htm = string.Format(htm, src);

            return htm;
        }
        public string GetPayJs()
        {
            string js = @"<script language='javascript' type='text/javascript'>
                    $(document).ready(function () {
                        setInterval('ajaxstatus()', 2000);
                    });
                    function ajaxstatus() {
                        if ($('#out_trade_no').val() != 0) {
                           $.get('/Plugins/WxPay/IsPaid',{id:$('#out_trade_no').val()},
                            function(data){
                                if (data == 1) { //订单状态为1表示支付成功
                                    $('#paydiv').hide();
                                    $('#payComplent').show();
                                    //$('#okbutton').attr('href', '/orderdetails/' + $('#out_trade_no').val());
                                                                   // window.location.href = '/orderdetails/' + $('#out_trade_no').val(); //页面跳转
                                 }
                        });
                        }
                    }
                </script>";
            return js;
        }
    }
}
