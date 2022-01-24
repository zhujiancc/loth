﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace loth.fur.Controllers
{
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
            var info1 = $@"名称：{options1.Name}，
                      版本：{options1.Version}，
                      公司：{options1.Company}";

            var info2 = $@"名称：{options2.Name}，
                      版本：{options2.Version}，
                      公司：{options2.Company}";

            var info3 = $@"名称：{options3.Name}，
                      版本：{options3.Version}，
                      公司：{options3.Company}";

            return $"{info1}-{info2}-{info3}";
        }
    }
}