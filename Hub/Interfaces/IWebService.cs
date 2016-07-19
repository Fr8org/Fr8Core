using Data.Entities;

namespace Hub.Interfaces
{
    public interface IWebService
    {
        WebServiceDO RegisterOrUpdate(WebServiceDO webServiceDo);
    }
}