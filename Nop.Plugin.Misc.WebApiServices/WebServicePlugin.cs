using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Plugin.Misc.WebApiServices.Security;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Plugin.Misc.WebApiServices.Domain;

namespace Nop.Plugin.Misc.WebApiServices
{
    public class WebServicePlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly IRepository<Nop.Core.Domain.Customers.CustomerRole> _customerRoleRepository;

        #endregion

        #region Ctor

        public WebServicePlugin(IPermissionService permissionService,
            IRepository<Nop.Core.Domain.Customers.CustomerRole> customerRoleRepository,
            ICustomerService customerService)
        {
            this._permissionService = permissionService;
            this._customerRoleRepository = customerRoleRepository;
            this._customerService = customerService;
        }

        #endregion

        #region Methods

        public override void Install()
        {
            //install new permissions
            _permissionService.InstallPermissions(new WebServicePermissionProvider());
            //install new role
            var crWebService = new CustomerRole
            {
                Name = "WebService Manager",
                Active = true,
                IsSystemRole = true,
                SystemName = CustomSystemCustomerRoleName.WebServiceManager,
            };
            _customerRoleRepository.Insert(crWebService);
            //insert admin user to the web service role
            var adminUser = _customerService.GetCustomerBySystemName(SystemCustomerRoleNames.Administrators);
            if (adminUser != null)
                adminUser.CustomerRoles.Add(crWebService);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.WebServices.Description1", "Actually configuration is not required. Just some notes:");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.WebServices.Description2", 
                "After the installation, you will see a new role (Customers -> Customers Role) \"WebService Manager\" and Four default permissions i.e Access, Delete, Put and Post. Ensure that permissions are properly configured on Access Control List page (maybe disabled by default)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.WebServices.Description3", "To access service use {0}");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.WebServices.Description4", "For versionning use {0}");

            base.Install();
        }

        public override void Uninstall()
        {
            //uninstall permissions
            _permissionService.UninstallPermissions(new WebServicePermissionProvider());

            //uninstall WebService role
            //var crWebService = _customerRoleRepository.GetById(1);
            //if (crWebService != null)
            //    _customerRoleRepository.Delete(crWebService);

            //locales
            this.DeletePluginLocaleResource("Plugins.Misc.WebServices.Description1");
            this.DeletePluginLocaleResource("Plugins.Misc.WebServices.Description2");
            this.DeletePluginLocaleResource("Plugins.Misc.WebServices.Description3");
            this.DeletePluginLocaleResource("Plugins.Misc.WebServices.Description4");

            base.Uninstall();
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "MiscWebApiServices";
            routeValues = new RouteValueDictionary() 
            { 
                { "Namespaces", "Nop.Plugin.Misc.WebApiServices.Controllers" },
                { "area", null } 
            };
        }

        #endregion
    }
}
