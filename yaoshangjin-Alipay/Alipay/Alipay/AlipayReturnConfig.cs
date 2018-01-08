using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;

namespace Com.Alipay
{
    /// <summary>
    /// 命名空间：Com.Alipay
    /// 名    称：AlipayReturnConfig
    /// 功    能：基础类
    /// 详    细：支付宝退款配置
    /// 版    本：1.0.0.0
    /// 创建时间：2017-08-03 10:50
    /// 修改时间：2017-08-04 01:26
    /// 修改时间：time
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public class AlipayReturnConfig:AlipayConfig
    {

        //↓↓↓↓↓↓↓↓↓↓请在这里配置您的基本信息↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

        // 退款日期 时间格式 yyyy-MM-dd HH:mm:ss
        public  string refund_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 调用的接口名，无需修改
        public  string service = "refund_fastpay_by_platform_pwd";

        //↑↑↑↑↑↑↑↑↑↑请在这里配置您的基本信息↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

    }
}