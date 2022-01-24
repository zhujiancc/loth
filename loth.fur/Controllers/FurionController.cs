using Furion;
using Microsoft.AspNetCore.Mvc;

namespace loth.fur.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FurionController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<FurionController> _logger;

        public FurionController(ILogger<FurionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            // 不推荐采用此方式读取，该方式仅在 ConfigureServices 启动时使用
            var appInfo = App.GetConfig<AppInfoOptions>("AppInfo", true);
            return $@"名称：{appInfo.Name}，
                      版本：{appInfo.Version}，
                      公司：{appInfo.Company}";
        }
    }
}