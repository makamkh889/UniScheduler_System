using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GraduationProject.core.Filters
{
    public class HandelErrorAttribute : Attribute, IExceptionFilter
    {
        void IExceptionFilter.OnException(ExceptionContext context)
        {
            ContentResult contentResult = new ContentResult();
            contentResult.StatusCode = 500;
            contentResult.Content= context.Exception.Message;
            context.Result = contentResult;
        }
    }
}
