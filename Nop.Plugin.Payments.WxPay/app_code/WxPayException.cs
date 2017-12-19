using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.WxPay.app_code
{
    public partial class WxPayException : Exception 
    {
        public WxPayException(string msg) : base(msg)
        {

        }
    }
}
