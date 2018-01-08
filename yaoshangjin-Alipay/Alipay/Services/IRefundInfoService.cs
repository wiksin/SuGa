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
    /// 名    称：IRefundInfoService
    /// 功    能：接口
    /// 详    细：退款服务
    /// 版    本：1.0.0.0
    /// 文件名称：IRefundInfoService.cs
    /// 创建时间：2017-08-03 05:41
    /// 修改时间：2017-08-04 01:33
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public interface IRefundInfoService
    {
        void Delete(RefundInfo refundInfo);
        void Insert(RefundInfo refundInfo);
        void Update(RefundInfo refundInfo);

        IList<RefundInfo> GetAll();

        RefundInfo GetById(int refundInfoId);
        RefundInfo GetByOrderId(int orderId);
        /// <summary>
        /// 根据退款批次号获取退款信息
        /// </summary>
        /// <param name="Batch_no">批次号</param>
        /// <returns></returns>
        RefundInfo GetRefundInfoByBatch_no(string Batch_no);
    }
}
