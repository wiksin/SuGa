using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Misc.WebApiServices.Models
{
    public static class Constants
    {
        public static class MediaTypeNames
        {
            public const string ApplicationXml = "application/xml";
            public const string TextXml = "text/xml";
            public const string ApplicationJson = "application/json";
            public const string TextJson = "text/json";
        }
        public static class CommonRoutingDefinitions
        {
            public const string ApiSegmentName = "api";
            public const string ApiVersionSegmentName = "apiVersion";
            public const string CurrentApiVersion = "v1";
        }
         public static class CommonLinkRelValues        
         {           
             public const string Self = "self";        
             public const string All = "all";           
         } 
        public static class SchemeTypes { public const string Basic = "basic"; }

        public const string DefaultLegacyNamespace = "http://tempuri.org/";
         public static Uri GetLocationLink(ILinkContaining linkContaining)        
         {            
             var locationLink = linkContaining.Links.FirstOrDefault(x => x.Rel == Constants.CommonLinkRelValues.Self);           
             return locationLink == null ? null : new Uri(locationLink.Href);       
         } 
    }
}