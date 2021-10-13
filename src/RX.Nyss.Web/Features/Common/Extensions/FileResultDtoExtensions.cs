using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Common.Extensions
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
