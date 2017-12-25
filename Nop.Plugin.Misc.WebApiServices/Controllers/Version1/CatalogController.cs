using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Extensions;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Misc.WebApiServices.Controllers.Version1
{
    public class CatalogController : BaseApiController
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICopyProductService _copyProductService;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly IWorkContext _workContext;
        private readonly IPluginFinder _pluginFinder;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        public CatalogController(IManufacturerService manufacturerService, ICategoryService categoryService,
            IProductService productService, ICompareProductsService compareProductsService,
            IProductAttributeService productAttributeService, ICopyProductService copyProductService, MediaSettings mediaSettings,
            ICacheManager cacheManager, IPictureService pictureService, IWorkContext workContext, CatalogSettings catalogSettings,
            IPluginFinder pluginFinder, IStoreContext storeContext, IWebHelper webHelper, ILocalizationService localizationService)
        {
            _manufacturerService = manufacturerService;
            _mediaSettings = mediaSettings;
            _categoryService = categoryService;
            _productService = productService;
            _compareProductsService = compareProductsService;
            _productAttributeService = productAttributeService;
            _copyProductService = copyProductService;
            _mediaSettings = mediaSettings;
            _cacheManager = cacheManager;
            _pictureService = pictureService;
            _workContext = workContext;
            _pluginFinder = pluginFinder;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _catalogSettings = catalogSettings;
        }

        #region MANUFACTURER
        //http://localhost/api/v1/catalog/manufacturers
        [ActionName("manufacturers")]
        public IHttpActionResult GetManufacturers(int pageIndex = 0, int pageSize = 10)
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetAllManufacturers(pageIndex: pageIndex, pageSize: pageSize);
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = manufacturer.ToModel();

                //prepare picture model
                int pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
                var manufacturerPictureCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_PICTURE_MODEL_KEY,
                    manufacturer.Id, pictureSize, true,
                    _workContext.WorkingLanguage.Id,
                    _webHelper.IsCurrentConnectionSecured(),
                    _storeContext.CurrentStore.Id);
                modelMan.PictureModel = _cacheManager.Get(manufacturerPictureCacheKey, () =>
                {
                    var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                    var pictureModel = new PictureModel()
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), modelMan.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), modelMan.Name)
                    };
                    return pictureModel;
                });
                model.Add(modelMan);
            }
            return Ok(model);
        }
        //http://localhost/api/v1/catalog/manufacturernavigation
        [ActionName("manufacturernavigation")]
        public IHttpActionResult GetManufacturerNavigation(int currentManufacturerId = 0)
        {
            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            string cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY,
                currentManufacturerId, _workContext.WorkingLanguage.Id, string.Join(",", customerRolesIds),
                _storeContext.CurrentStore.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () =>
                {
                    var currentManufacturer = _manufacturerService.GetManufacturerById(currentManufacturerId);

                    var manufacturers = _manufacturerService.GetAllManufacturers(pageSize: _catalogSettings.ManufacturersBlockItemsToDisplay);
                    var model = new ManufacturerNavigationModel()
                    {
                        TotalManufacturers = manufacturers.TotalCount
                    };

                    foreach (var manufacturer in manufacturers)
                    {
                        var modelMan = new ManufacturerBriefInfoModel()
                        {
                            Id = manufacturer.Id,
                            Name = manufacturer.GetLocalized(x => x.Name),
                            SeName = manufacturer.GetSeName(),
                            IsActive = currentManufacturer != null && currentManufacturer.Id == manufacturer.Id,
                        };
                        model.Manufacturers.Add(modelMan);
                    }
                    return model;
                });

            return Ok(cacheModel);
        }
        [ActionName("manufacturer")]
        public IHttpActionResult GetManufacturer(int id = 0)
        {
            if (id <= 0)
                return Ok();
            var model = new List<ManufacturerModel>();
            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                return NotFound();

            var modelMan = manufacturer.ToModel();
            //prepare picture model
            int pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
            var manufacturerPictureCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_PICTURE_MODEL_KEY,
                manufacturer.Id, pictureSize, true,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured(),
                _storeContext.CurrentStore.Id);
            modelMan.PictureModel = _cacheManager.Get(manufacturerPictureCacheKey, () =>
                {
                    var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                    var pictureModel = new PictureModel()
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageLinkTitleFormat"), modelMan.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Manufacturer.ImageAlternateTextFormat"), modelMan.Name)
                    };
                    return pictureModel;
                });
            model.Add(modelMan);
            return Ok(model);
        }

        #endregion

        #region CATEGORIES

        [ActionName("categories")]
        public IHttpActionResult GetCategories(int pageIndex = 0, int pageSize = 10)
        {
            var categories = _categoryService.GetAllCategories(pageIndex: pageIndex, pageSize: pageSize)
            .Select(x =>
            {
                var catModel = x.ToModel();

                //prepare picture model
                int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                {
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    var pictureModel = new PictureModel()
                    {
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                    };
                    return pictureModel;
                });

                return catModel;
            })
            .ToList();

            return Ok(categories);
        }
        [ActionName("categorynavigation")]
        public IHttpActionResult GetCategoryNavigation(int currentCategoryId = 0, int currentProductId = 0)
        {
            return Ok();
        }

        [ActionName("homepagecategories")]
        public IHttpActionResult GetHomepageCategories()
        {
            var customerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            string categoriesCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_HOMEPAGE_KEY,
                string.Join(",", customerRolesIds),
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());

            var model = _cacheManager.Get(categoriesCacheKey, () =>
            {
                return _categoryService.GetAllCategoriesDisplayedOnHomePage()
                .Select(x =>
                {
                    var catModel = x.ToModel();

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(x.PictureId);
                        var pictureModel = new PictureModel()
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList();
            });
            return Ok(model);
        }

        #endregion

    }
}
