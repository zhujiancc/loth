using Furion.DynamicApiController;

namespace loth.fur.Controllers
{
    [DynamicApiController]
    public class FurionAppServiceV2
    {
        public string Get()
        {
            return $"Hello2 {nameof(Furion)}";
        }
    }
}