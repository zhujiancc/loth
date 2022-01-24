using Furion.DynamicApiController;

namespace loth.fur.Controllers
{
    [DynamicApiController]
    public class FurionAppService
    {
        public string Get()
        {
            return $"Hello2 {nameof(Furion)}";
        }
    }
}