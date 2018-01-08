using DaBoLang.Nop.Plugin.Payments.AliPay.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Services
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay.Services
    /// 名    称：IPaymentInfoService
    /// 功    能：接口
    /// 详    细：支付服务接口
    /// 版    本：1.0.0.0
    /// 文件名称：IPaymentInfoService.cs
    /// 创建时间：2017-08-03 02:48
    /// 修改时间：2017-08-04 01:32
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public interface IPaymentInfoService
    {
        void Delete(PaymentInfo paymentInfo);
        void Insert(PaymentInfo paymentInfo);
        void Update(PaymentInfo paymentInfo);

        IList<PaymentInfo> GetAll();

        PaymentInfo GetById(int paymentInfoId);
        PaymentInfo GetByOrderId(int orderId);

    }
}
