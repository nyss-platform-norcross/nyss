using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.Web.Utils.Filters
{
    public class SetCorrectResponseCode : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var resultObject = (context?.Result as ObjectResult)?.Value as Result;

            if (!resultObject?.IsSuccess ?? false)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            base.OnResultExecuting(context);
        }
    }
}