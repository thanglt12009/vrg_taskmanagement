using System;
using System.Configuration;
using Nest;
using Newtonsoft.Json;
using Elasticsearch.Net;

namespace TMS.WebApp.Services
{
    public static class ElasticConfig
    {
        public static string IndexName
        {
            get { return ConfigurationManager.AppSettings["indexName"]; }
        }

        public static string ElastisearchUrl
        {
            get { return ConfigurationManager.AppSettings["elastisearchUrl"]; }
        }

        public static IElasticClient GetClient()
        {
            var connectionPool = new SingleNodeConnectionPool(new Uri(ElastisearchUrl));
            var settings = new ConnectionSettings(connectionPool, connectionSettings => new MyJsonNetSerializer(connectionSettings))
                .DefaultIndex(IndexName)
                .DisableDirectStreaming()
                .PrettyJson();

            return new ElasticClient(settings);
        }
    }

    public class MyJsonNetSerializer : JsonNetSerializer
    {
        public MyJsonNetSerializer(IConnectionSettingsValues settings) : base(settings)
        {
        }
        /*
        protected override void ModifyJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings)
        {
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }
        */
    }
}