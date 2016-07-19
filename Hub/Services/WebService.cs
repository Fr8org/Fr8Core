using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Utility;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class WebService : IWebService
    {
        private readonly Dictionary<int, WebServiceDO> _webServices = new Dictionary<int, WebServiceDO>();
        private bool _isInitialized;
        private string _serverUrl;

        public bool IsATandTCacheDisabled
        {
            get;
            private set;
        }

        private void Initialize()
        {
            if (_isInitialized && !IsATandTCacheDisabled)
            {
                return;
            }

            lock (_webServices)
            {
                if (_isInitialized && !IsATandTCacheDisabled)
                {
                    return;
                }

                if (IsATandTCacheDisabled)
                {
                    _webServices.Clear();
                }

                LoadFromDb();

                _isInitialized = true;
            }
        }

        private void LoadFromDb()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var existingWebService in uow.WebServiceRepository.GetAll())
                {
                    _webServices[existingWebService.Id] = Clone(existingWebService);
                }
            }
        }

        private WebServiceDO Clone(WebServiceDO source)
        {
            var destination = new WebServiceDO();

            CopyPropertiesHelper.CopyProperties(source, destination, false);

            return destination;
        }

        public WebServiceDO RegisterOrUpdate(WebServiceDO webServiceDo)
        {
            if (webServiceDo == null)
            {
                return null;
            }

            if (!IsATandTCacheDisabled)
            {
                Initialize();
            }

            // we are going to change webServiceDO. It is not good to corrupt method's input parameters.
            // make a copy
            var clone = new WebServiceDO();

            CopyPropertiesHelper.CopyProperties(webServiceDo, clone, true);

            webServiceDo = clone;

            lock (_webServices)
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var existingWebService = uow.WebServiceRepository.FindOne(x => x.Name == webServiceDo.Name);

                    if (existingWebService == null)
                    {
                        webServiceDo.Id = 0;
                        uow.WebServiceRepository.Add(existingWebService = webServiceDo);
                    }
                    else
                    {
                        // this is for updating webService
                        CopyPropertiesHelper.CopyProperties(webServiceDo, existingWebService, false, x => x.Name != "Id");
                    }

                    uow.SaveChanges();

                    var webService = Clone(existingWebService);
                    _webServices[existingWebService.Id] = webService;

                    return webService;
                }
            }
        }
    }
}
