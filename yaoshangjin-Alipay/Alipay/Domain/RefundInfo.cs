using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Domain
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay.Domain
    /// 名    称：RefundInfo
    /// 功    能：实体类
    /// 详    细：退款记录
    /// 版    本：1.0.0.0
    /// 创建时间：2017-08-03 05:34
    /// 修改时间：2017-08-04 01:26
    /// 修改时间：time
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public partial class RefundInfo : BaseEntity
    {

        #region Properties
        public int OrderId { get; set; }
        /// <summary>
        /// 退款状态
        /// </summary>
        public int RefundStatusId { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal AmountToRefund { get; set; }
        public string Seller_Email { get; set; }
        public string Seller_Id { get; set; }
        /// <summary>
        /// 交易号，内部交易号，支付宝交易号或者微信交易号
        /// </summary>
        public string Batch_no { get; set; }
        /// <summary>
        /// 订单标号外部交易号
        /// </summary>
        public string Out_Trade_No { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateOnUtc { get; set; }
        /// <summary>
        /// 退款成功时间
        /// </summary>
        public DateTime? RefundOnUtc { get; set; }
        
        /// <summary>
        /// 回调ID
        /// </summary>
        public string Notify_Id { get; set; }
        /// <summary>
        /// 回调类型
        /// </summary>
        public string Notify_Type { get; set; }

        public string Result_Details { get; set; }
        #endregion

        /// <summary>
        /// 订单状态
        /// </summary>
        public RefundStatus RefundStatus
        {
            get
            {
                return (RefundStatus)this.RefundStatusId;
            }
            set
            {
                this.RefundStatusId = (int)value;
            }
        }

    }
    public enum RefundStatus
    {
        /// <summary>
        /// 申请退款
        /// </summary>
        refunding = 10,
        /// <summary>
        /// 退款成功
        /// </summary>
        refund = 20,
        /// <summary>
        /// 取消退款
        /// </summary>
        cancel = 30,
        /// <summary>
        /// 退款过期
        /// </summary>
        overtime = 40,
        /// <summary>
        /// 退款失败
        /// </summary>
        error = 50,
    }
}
