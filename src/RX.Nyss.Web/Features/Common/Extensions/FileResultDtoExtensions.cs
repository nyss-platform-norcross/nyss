using System.Threading.Tasks;
using RX.Nyss.Web.Features.DataCollectors.Dto;

namespace Microsoft.AspNetCore.Mvc
{
    public static class FileResultDtoExtensions
    {
        public static async Task<FileResult> AsFileResult(this Task<FileResultDto> fileResultTask)
        {
            var result = await fileResultTask;

            return new FileContentResult(result.Data, result.MimeType);
        }
    }
}
