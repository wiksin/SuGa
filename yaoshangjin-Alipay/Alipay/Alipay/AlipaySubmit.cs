using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using System.Xml;
using Nop.Web.Framework;

namespace Com.Alipay
{
    /// <summary>
    /// 类名：Submit
    /// 功能：支付宝各接口请求提交类
    /// 详细：构造支付宝各接口表单HTML文本，获取远程HTTP数据
    /// 版本：3.3
    /// 修改日期：2011-07-05
    /// 说明：
    /// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
    /// 该代码仅供学习和研究支付宝接口使用，只是提供一个参考
    /// </summary>
    public class AlipaySubmit
    {

        /// <summary>
        /// 生成请求时的签名
        /// </summary>
        /// <param name="sPara">请求给支付宝的参数数组</param>
        /// <returns>签名结果</returns>
        public static string BuildRequestMysign(Dictionary<string, string> sPara,AlipayConfig alipayConfig)
        {
            //把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
            string prestr = AlipayCore.CreateLinkString(sPara);

            string _sign_type = alipayConfig.sign_type;
            string _key = alipayConfig.key;
            string _input_charset = alipayConfig.input_charset;
            //把最终的字符串签名，获得签名结果
            string mysign = "";
            switch (_sign_type)
            {
                case "MD5":
                    mysign = AlipayMd5.Sign(prestr, _key, _input_charset);
                    break;
                default:
                    mysign = "";
                    break;
            }

            return mysign;
        }

        /// <summary>
        /// 生成要请求给支付宝的参数数组
        /// </summary>
        /// <param name="sParaTemp">请求前的参数数组</param>
        /// <returns>要请求的参数数组</returns>
        public static Dictionary<string, string> BuildRequestPara(SortedDictionary<string, string> sParaTemp,AlipayConfig alipayConfig)
        {
            //待签名请求参数数组
            Dictionary<string, string> sPara = new Dictionary<string, string>();
            //签名结果
            string mysign = "";

            //过滤签名参数数组
            sPara = AlipayCore.FilterPara(sParaTemp);

            //获得签名结果
            mysign = BuildRequestMysign(sPara, alipayConfig);

            //签名结果与签名方式加入请求提交参数组中
            sPara.Add("sign", mysign);
            sPara.Add("sign_type", alipayConfig.sign_type);

            return sPara;
        }

        /// <summary>
        /// 生成要请求给支付宝的参数数组
        /// </summary>
        /// <param name="sParaTemp">请求前的参数数组</param>
        /// <param name="code">字符编码</param>
        /// <returns>要请求的参数数组字符串</returns>
        public static string BuildRequestParaToString(SortedDictionary<string, string> sParaTemp, Encoding code, AlipayConfig alipayConfig)
        {
            //待签名请求参数数组
            Dictionary<string, string> sPara = new Dictionary<string, string>();
            sPara = BuildRequestPara(sParaTemp, alipayConfig);

            //把参数组中所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串，并对参数值做urlencode
            string strRequestData = AlipayCore.CreateLinkStringUrlencode(sPara, code);

            return strRequestData;
        }
  
        /// <summary>
        ///  建立支付宝请求
        /// </summary>
        /// <param name="sParaTemp">请求参数数组</param>
        /// <param name="alipayConfig">配置文件</param>
        /// <param name="strMethod">提交方式。两个值可选：post、get</param>
        /// <returns>nop远程访问辅助类</returns>
        public static RemotePost BuildRequest(SortedDictionary<string, string> sParaTemp, AlipayConfig alipayConfig,string strMethod="GET")
        {
            var post = new RemotePost
            {
                FormName = "alipaysubmit",
                Url = string.Format("{0}?_input_charset={1}", "https://mapi.alipay.com/gateway.do", alipayConfig.input_charset),
                Method = strMethod
            };
            var  requestParas=BuildRequestPara(sParaTemp, alipayConfig);
            foreach (var item in requestParas)
            {
                post.Add(item.Key,item.Value);
            }
            return post;
        }
    }
}