using DaBoLang.Nop.Plugin.Payments.AliPay.Domain;
using Nop.Data.Mapping;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Data
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay.Data
    /// 名    称：PaymentInfoMap
    /// 功    能：实体映射
    /// 详    细：支付表映射
    /// 版    本：1.0.0.0
    /// 文件名称：PaymentInfoMap.cs
    /// 创建时间：2017-08-03 12:05
    /// 修改时间：2017-08-04 01:30
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public partial class PaymentInfoMap : NopEntityTypeConfiguration<PaymentInfo>
    {
        public PaymentInfoMap()
        {
            this.ToTable("dbl_PaymentInfo");
            this.HasKey(x => x.Id);
        }
    }
}