using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.WebApiServices.Domain;

namespace Nop.Plugin.Misc.WebApiServices.Security
{
    public partial class WebServicePermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord AccessWebApiService = new PermissionRecord { Name = "Plugins. Allow Access Web Api Service", SystemName = "AccessWebApiService", Category = "Plugin" };
        public static readonly PermissionRecord PostWebApiService = new PermissionRecord { Name = "Plugins. Allow Post Web Api Service", SystemName = "PostWebApiService", Category = "Plugin" };
        public static readonly PermissionRecord PutWebApiService = new PermissionRecord { Name = "Plugins. Allow Put Web Api Service", SystemName = "PutWebApiService", Category = "Plugin" };
        public static readonly PermissionRecord DeleteWebApiService = new PermissionRecord { Name = "Plugins. Allow Delete Web Api Service", SystemName = "DeleteWebApiService", Category = "Plugin" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[] 
            {
                AccessWebApiService,
                PostWebApiService,
                PutWebApiService,
                DeleteWebApiService
            };
        }

        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return Enumerable.Empty<DefaultPermissionRecord>();
            return new[] 
            {
                new DefaultPermissionRecord 
                {
                    CustomerRoleSystemName = CustomSystemCustomerRoleName.WebServiceManager,
                    PermissionRecords = new PermissionRecord[]
                    {
                        AccessWebApiService,
                        PostWebApiService,
                        PutWebApiService,
                        DeleteWebApiService,
                    }
                }
            };
        }
    }
}