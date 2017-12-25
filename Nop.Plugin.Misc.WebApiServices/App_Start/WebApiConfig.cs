using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Linq;
using System.Web.Http.Description;
using SDammann.WebApi.Versioning;
using Autofac;
using System.Reflection;
using Nop.Core.Infrastructure;
using Autofac.Integration.WebApi;
using Nop.Plugin.Misc.WebApiServices.Logger;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;

namespace Nop.Plugin.Misc.WebApiServices
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApiWithVersion",
            //    routeTemplate: "api/v{version}/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.Routes.MapHttpRoute(
                name: "DefaultApiWithVersionAndAction",
                routeTemplate: "api/v{version}/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );





            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiForPlugin",
                routeTemplate: "Plugins/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );



            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            // We have to register services specifically for the API calls!
            //builder.RegisterType<CategoryService>().AsImplementedInterfaces().InstancePerLifetimeScope();
            //Update existing, don't create a new container
            builder.Update(EngineContext.Current.ContainerManager.Container);

            //Feed the current container to the AutofacWebApiDependencyResolver
            var resolver = new AutofacWebApiDependencyResolver(EngineContext.Current.ContainerManager.Container);
            config.DependencyResolver = resolver;


            // enable API versioning
            config.Services.Replace(typeof(IApiExplorer), new VersionedApiExplorer(GlobalConfiguration.Configuration));
            config.Services.Replace(typeof(IHttpControllerSelector), new RouteVersionedControllerSelector(GlobalConfiguration.Configuration));

            //exception logger 
            config.Services.Add(typeof(IExceptionLogger), new SimpleExceptionLogger());
            //exception handler
            config.Services.Replace(typeof(IExceptionHandler), new GlobalExceptionHandler());


            //we will get JSON by default, but it will still allow you to return XML if you pass text/xml as the request Accept header
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configuration.EnsureInitialized();
        }
    }
}
