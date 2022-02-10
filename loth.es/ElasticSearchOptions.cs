using System.ComponentModel.DataAnnotations;

namespace loth.es
{
    public class ElasticSearchOptions
    {
        /// <summary>
        /// node 地址
        /// </summary>
        [Required]
        public string Uris { get; set; }

        /// <summary>
        /// 请求超时时间/s
        /// </summary>
        public int RequestTimeout { get; set; } = 20;

        /// <summary>
        /// Ping超时时间/s
        /// </summary>
        public int PingTimeout { get; set; } = 20;
    }
}