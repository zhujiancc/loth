using Furion.ConfigurableOptions;
using System.ComponentModel.DataAnnotations;

namespace loth.fur
{
    public class AppInfoOptions : IConfigurableOptions
    {
        [StringLength(maximumLength: 10, MinimumLength = 5)]
        public string Name { get; set; }
        public string Version { get; set; }
        public string Company { get; set; }
    }
}