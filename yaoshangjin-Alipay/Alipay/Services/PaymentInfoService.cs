using DaBoLang.Nop.Plugin.Payments.AliPay.Domain;
using Nop.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Services
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay.Services
    /// 名    称：PaymentInfoService
    /// 功    能：服务类
    /// 详    细：
    /// 版    本：1.0.0.0
    /// 文件名称：PaymentInfoService.cs
    /// 创建时间：2017-08-03 02:48
    /// 修改时间：2017-08-04 01:39
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public partial class PaymentInfoService : IPaymentInfoService
    {
        #region 属性
        private readonly IRepository<PaymentInfo> _paymentInfoRepository;
        #endregion
        #region 构造
        public PaymentInfoService(IRepository<PaymentInfo> paymentInfoRepository)
        {
            this._paymentInfoRepository = paymentInfoRepository;
        }

        public void Delete(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null)
                throw new ArgumentNullException("paymentInfo");

            _paymentInfoRepository.Delete(paymentInfo);
        }

        public IList<PaymentInfo> GetAll()
        {
            var query = from p in _paymentInfoRepository.Table
                        orderby p.Id
                        select p;
            var records = query.ToList();
            return records;
        }

        public PaymentInfo GetById(int paymentInfoId)
        {
            if (paymentInfoId == 0)
                return null;

            return _paymentInfoRepository.GetById(paymentInfoId);
        }

        public PaymentInfo GetByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;
            var query = from p in _paymentInfoRepository.Table
                        where p.OrderId== orderId
                        select p;
            var records = query.FirstOrDefault();
            return records;
        }


        public void Insert(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null)
                throw new ArgumentNullException("paymentInfo");

            _paymentInfoRepository.Insert(paymentInfo);
        }

        public void Update(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null)
                throw new ArgumentNullException("paymentInfo");

            _paymentInfoRepository.Update(paymentInfo);
        }
        #endregion
    }
}
