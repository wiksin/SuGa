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
    /// 名    称：RefundInfoService
    /// 功    能：服务类
    /// 详    细：
    /// 版    本：1.0.0.0
    /// 文件名称：RefundInfoService.cs
    /// 创建时间：2017-08-03 05:41
    /// 修改时间：2017-08-04 01:39
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public partial class RefundInfoService : IRefundInfoService
    {
        #region 属性
        private readonly IRepository<RefundInfo> _refundInfoRepository;
        #endregion
        #region 构造
        public RefundInfoService(IRepository<RefundInfo> refundInfoRepository)
        {
            this._refundInfoRepository = refundInfoRepository;
        }

        public void Delete(RefundInfo refundInfo)
        {
            if (refundInfo == null)
                throw new ArgumentNullException("refundInfo");

            _refundInfoRepository.Delete(refundInfo);
        }

        public IList<RefundInfo> GetAll()
        {
            var query = from p in _refundInfoRepository.Table
                        orderby p.Id
                        select p;
            var records = query.ToList();
            return records;
        }

        public RefundInfo GetById(int refundInfoId)
        {
            if (refundInfoId == 0)
                return null;

            return _refundInfoRepository.GetById(refundInfoId);
        }

        public RefundInfo GetByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;
            var query = from p in _refundInfoRepository.Table
                        where p.OrderId== orderId
                        select p;
            var records = query.FirstOrDefault();
            return records;
        }


        public void Insert(RefundInfo refundInfo)
        {
            if (refundInfo == null)
                throw new ArgumentNullException("refundInfo");

            _refundInfoRepository.Insert(refundInfo);
        }

        public void Update(RefundInfo refundInfo)
        {
            if (refundInfo == null)
                throw new ArgumentNullException("refundInfo");

            _refundInfoRepository.Update(refundInfo);
        }

        public RefundInfo GetRefundInfoByBatch_no(string Batch_no)
        {
            if (String.IsNullOrEmpty(Batch_no))
            {
                throw new Exception("参数不能为空");
            }
            var query = _refundInfoRepository.Table;
            return query.Where(x => x.Batch_no == Batch_no).FirstOrDefault();
        }
        #endregion
    }
}
