using back_end.Services;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/image")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        //[HttpPost]
        //[Consumes("multipart/form-data")]
        //[RequestSizeLimit(6 * 1024 * 1024)]
        //public async Task<IActionResult> UploadImageAsync([FromForm] IFormFile? file)
        //{
        //    var response = await _fileService.UploadImageAsync(file);
        //    return Ok(response);
        //}

        //[HttpDelete]
        //public async Task<IActionResult> DeleteImageAsync([FromQuery] string objectKey)
        //{
        //    await _fileService.DeleteImageAsync(objectKey);
        //    return NoContent();
        //}
    }
}
