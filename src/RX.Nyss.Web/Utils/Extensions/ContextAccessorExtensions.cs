using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RX.Nyss.Web.Utils.Extensions
{
    public static class ContextAccessorExtensions
    {
        public static async Task<int?> GetResourceParameter(this IHttpContextAccessor httpContextAccessor, string parameterName) =>
            await httpContextAccessor.HttpContext.Request.GetResourceParameter(parameterName);

        public static async Task<int?> GetResourceParameter(this HttpRequest request, string parameterName)
        {
            if (request.RouteValues.ContainsKey(parameterName) && int.TryParse(request.RouteValues[parameterName].ToString(), out var idFromRoute))
            {
                return idFromRoute;
            }

            if (request.Query.ContainsKey(parameterName) && int.TryParse(request.Query[parameterName].ToString(), out var idFromQuery))
            {
                return idFromQuery;
            }

            if (request.ContentType != "application/json")
            {
                return null;
            }

            using var reader = new StreamReader(request.Body);
            var json = await reader.ReadToEndAsync();

            var data = JsonConvert.DeserializeObject<JObject>(json);

            // After reading body recover it for reading again
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return data[parameterName]?.Value<int?>();
        }
    }
}
