using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Core.StructureMap;
using Data.Entities;
using Data.Infrastructure;

using Data.Interfaces;
using StructureMap;
using Utilities.Logging;

namespace Web.ExceptionHandling
{
    public class MvcHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if (filterContext.IsChildAction)
            {
                return;
            }

            // If custom errors are disabled, we need to let the normal ASP.NET exception handler
            // execute so that the user can see useful debugging information.
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            Exception exception = filterContext.Exception;
            
            if (!ExceptionType.IsInstanceOfType(exception))
            {
                return;
            }

            string controllerName = (string)filterContext.RouteData.Values["controller"];
            string actionName = (string)filterContext.RouteData.Values["action"];

            var httpException = filterContext.Exception as HttpException;
            int statusCode = 500;
            var view = View;
            if (httpException != null)
            {
                statusCode = httpException.GetHttpCode();
                var viewPath = String.Format("Views/Shared/{0}.cshtml", statusCode);
                var explicitPath = Path.Combine(Utilities.Server.ServerPhysicalPath, viewPath);
                if (File.Exists(explicitPath))
                {
                    view = statusCode.ToString();
                }
            }

            Logger.GetLogger().Error("Critical internal error occured.", exception);
           
            HandleErrorInfo model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
            filterContext.Result = new ViewResult
            {
                ViewName = view,
                MasterName = Master,
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                TempData = filterContext.Controller.TempData
            };
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = statusCode;

            // Certain versions of IIS will sometimes use their own error page when
            // they detect a server error. Setting this property indicates that we
            // want it to try to render ASP.NET MVC's error page instead.
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

            SaveUnhandledException(filterContext);


        }
        //Save unhandle exception to incident table
        private void SaveUnhandledException(ExceptionContext filterContext)
        {
            using (var _uow = ContainerObjectFactory.Container.GetInstance<IUnitOfWork>())
            {
                IncidentDO incidentDO = new IncidentDO();
                incidentDO.PrimaryCategory = "Error";
                if (filterContext.Exception.Message.Contains("Validation failed"))
                    incidentDO.SecondaryCategory = "ValidationException";
                else
                    incidentDO.SecondaryCategory = "ApplicationException";
                incidentDO.Data = filterContext.Exception.Message;
                _uow.IncidentRepository.Add(incidentDO);
                _uow.SaveChanges();
            }
        
        }
    }
}