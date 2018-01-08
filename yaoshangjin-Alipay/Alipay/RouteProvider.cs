using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace DaBoLang.Nop.Plugin.Payments.AliPay
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay
    /// 名    称：RouteProvider
    /// 功    能：路由
    /// 详    细：定义路由
    /// 版    本：1.0.0.0
    /// 文件名称：RouteProvider.cs
    /// 创建时间：2017-08-02 01:22
    /// 修改时间：2017-08-04 01:53
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(RouteCollection routes)
        {
            //支付通知路由
            routes.MapRoute("DaBoLang.Plugin.Payments.AliPay.Notify",
                 "Plugins/AliPay/Notify",
                 new { controller = "AliPay", action = "Notify" },
                 new[] { "DaBoLang.Nop.Plugin.Payments.AliPay.Controllers" }
            );

            //支付页面跳转同步通知页面
            routes.MapRoute("DaBoLang.Plugin.Payments.AliPay.Return",
                 "Plugins/AliPay/Return",
                 new { controller = "AliPay", action = "Return" },
                 new[] { "DaBoLang.Nop.Plugin.Payments.AliPay.Controllers" }
            );

            //退款通知路由
            routes.MapRoute("Plugin.Payments.AliPay.RefundNotify",
              "Plugins/AliPay/RefundNotify",
              new { controller = "AliPay", action = "RefundNotify" },
              new[] { "DaBoLang.Nop.Plugin.Payments.AliPay.Controllers" }
            );
        }

        #endregion

        #region Properties

        public int Priority
        {
            get
            {
                return 0;
            }
        }

        #endregion
    }
}
