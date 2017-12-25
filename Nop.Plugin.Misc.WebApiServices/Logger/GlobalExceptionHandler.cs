using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.ExceptionHandling;

namespace Nop.Plugin.Misc.WebApiServices.Logger
{
    public class GlobalExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            var exception = context.Exception;
            var httpException = exception as HttpException;
            if (httpException != null)
            {
                context.Result = new ErrorResult(context.Request, (HttpStatusCode)httpException.GetHttpCode(), 
                    httpException.Message);
                return;
            }
            //if (exception is RootObjectNotFoundException)
            //{
            //    context.Result = new ErrorResult(context.Request, HttpStatusCode.NotFound, exception.Message); 
            //    return;
            //}
            //if (exception is ChildObjectNotFoundException) 
            //{ 
            //    context.Result = new ErrorResult(context.Request, HttpStatusCode.Conflict, exception.Message); 
            //    return; 
            //}
            context.Result = new ErrorResult(context.Request, HttpStatusCode.InternalServerError, exception.Message);
        }
    }
}