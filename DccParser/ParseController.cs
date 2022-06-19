using Microsoft.AspNetCore.Mvc;

namespace DccParser
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParseController : ControllerBase
    {
        [HttpPost]
        public ActionResult<ParseResponse> Parse([FromBody]ParseArgs args, [FromServices]CombinedParser parser)
        {
            if (string.IsNullOrWhiteSpace(args.Buffer))
                return new BadRequestObjectResult("Empty.");

            Span<byte> span = new(new byte[5 * 1024 * 1024]); //TODO setting
            if (!Convert.TryFromBase64String(args.Buffer, span, out var length))
                return new BadRequestObjectResult("Invalid base64.");

            var buffer = span.Slice(0, length).ToArray();

            var result = parser.Parse(buffer);
            
            if (result == null)
                return new BadRequestObjectResult("Could not parse pdf or image.");

            return new JsonResult(new ParseResponse() { Dcc = result });
        }
    }
}
