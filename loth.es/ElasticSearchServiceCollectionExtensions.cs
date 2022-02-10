using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace loth.es
{
    public static class ElasticSearchServiceCollectionExtensions
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, Action<ElasticSearchOptions> setupAction)
        {
            if (setupAction == null)
            {
                return services;
            }

            var options = new ElasticSearchOptions();

            setupAction(options);

            var uris = options.Uris.Split(",").Select(x => new Uri(x));

            var logger = services.BuildServiceProvider().GetService<ILogger>();

            var pool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(pool);
            settings
                .RequestTimeout(TimeSpan.FromSeconds(options.RequestTimeout))
                .PingTimeout(TimeSpan.FromSeconds(options.PingTimeout))
                .OnRequestCompleted(apiCallDetails =>
                {
                    var list = new List<string>();
                    // log out the request and the request body, if one exists for the type of request
                    if (apiCallDetails.RequestBodyInBytes != null)
                    {
                        list.Add(
                            $"{apiCallDetails.HttpMethod} {apiCallDetails.Uri} " +
                            $"{Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes)}");
                    }
                    else
                    {
                        list.Add($"{apiCallDetails.HttpMethod} {apiCallDetails.Uri}");
                    }
                    logger.LogDebug(string.Join(";", list));
                });

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);

            return services;
        }

    }
}
