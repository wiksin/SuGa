using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ExceptionHandling;

namespace Nop.Plugin.Misc.WebApiServices.Logger
{
    public class SimpleExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            if (context.Exception == null)
                return;
            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            //ignore 404 HTTP errors
            var httpException = context.Exception as HttpException;
            if (httpException != null && httpException.GetHttpCode() == 404 &&
                !EngineContext.Current.Resolve<CommonSettings>().Log404Errors)
                return;
            try
            {
                var _logger = EngineContext.Current.Resolve<ILogger>();
                _logger.Error(context.Exception.Message, context.Exception);
            }
            catch (Exception)
            {
                //don't throw new exception if occurs
            }
        }
    }
}