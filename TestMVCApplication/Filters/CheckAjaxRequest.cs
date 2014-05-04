using System.Web.Mvc;

namespace TestMVCApplication.Filters
{
    public class CheckAjaxRequestAttribute : ActionFilterAttribute
    {
        private const string AjaxHeader = "X-Requested-With";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool isAjaxRequest = filterContext.HttpContext.Request.Headers[AjaxHeader] != null;
            if (!isAjaxRequest)
            {
                filterContext.Result = new ViewResult { ViewName = "Unauthorized" };
            }
        }
    }
}