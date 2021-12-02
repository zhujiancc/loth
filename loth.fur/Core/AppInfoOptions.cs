using Furion.ConfigurableOptions;

namespace loth.fur
{
    public class AppInfoOptions : IConfigurableOptions
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Company { get; set; }
    }
}