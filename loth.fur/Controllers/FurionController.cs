using Furion;
using Furion.DynamicApiController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
            // ���Ƽ����ô˷�ʽ��ȡ���÷�ʽ���� ConfigureServices ����ʱʹ��
            var appInfo = App.GetConfig<AppInfoOptions>("AppInfo", true);
            return $@"���ƣ�{appInfo.Name}��
                      �汾��{appInfo.Version}��
                      ��˾��{appInfo.Company}";
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class FurController : ControllerBase
    {
        private readonly AppInfoOptions options1;
        private readonly AppInfoOptions options2;
        private readonly AppInfoOptions options3;

        public FurController(
            IOptions<AppInfoOptions> options
            , IOptionsSnapshot<AppInfoOptions> optionsSnapshot
            , IOptionsMonitor<AppInfoOptions> optionsMonitor)
        {
            options1 = options.Value;
            options2 = optionsSnapshot.Value;
            options3 = optionsMonitor.CurrentValue;
        }

        [HttpGet]
        public string Get()
        {
            var info1 = $@"���ƣ�{options1.Name}��
                      �汾��{options1.Version}��
                      ��˾��{options1.Company}";

            var info2 = $@"���ƣ�{options2.Name}��
                      �汾��{options2.Version}��
                      ��˾��{options2.Company}";

            var info3 = $@"���ƣ�{options3.Name}��
                      �汾��{options3.Version}��
                      ��˾��{options3.Company}";

            return $"{info1}-{info2}-{info3}";
        }
    }


    [DynamicApiController]
    public class FurionAppService
    {
        public string Get()
        {
            return $"Hello2 {nameof(Furion)}";
        }
    }
}