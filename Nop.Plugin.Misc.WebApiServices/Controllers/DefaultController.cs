using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.WebApiServices.Security;
using Nop.Services.Catalog;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Nop.Plugin.Misc.WebApiServices.Controllers
{
    public class DefaultController : BaseApiController
    {
        private readonly ICategoryService _categoryService;
        private readonly IPermissionService _permissionSettings;
        private readonly IProductService _productService;
        public DefaultController(ICategoryService categoryService, IPermissionService permissionSettings, IProductService productService)
        {
            this._categoryService = categoryService;
            this._permissionSettings = permissionSettings;
            this._productService = productService;
            //_categoryService = EngineContext.Current.Resolve<ICategoryService>();
        }


        // GET: api/Default
        public List<string> Get()
        {
            //valdiate whether we can access this web service
            //if (!_permissionSettings.Authorize(WebServicePermissionProvider.AccessWebApiService))
            //    throw new ApplicationException("Not allowed to access web service");

            return _categoryService.GetAllCategories()
                                    .Select(c => c.Name)
                                    .ToList();

        }
        //[Route("manufacturer/{id}")]
        public string Get(int id)
        {
            //if (!_permissionSettings.Authorize(WebServicePermissionProvider.PostWebApiService))
            //    throw new ApplicationException("Not allowed to access web service");
            return _categoryService.GetAllCategories().SingleOrDefault(c => c.Id == id).Name;
        }

        // POST: api/Default
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Default/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Default/5
        public void Delete(int id)
        {
        }
    }
}
