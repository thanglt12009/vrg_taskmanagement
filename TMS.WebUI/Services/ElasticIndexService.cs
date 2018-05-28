using System;
using TMS.Domain.Entities;
using Nest;
using System.Collections;
using System.Collections.Generic;

namespace TMS.WebApp.Services
{
    public class ElasticIndexService
    {
        private readonly IElasticClient client;

        public ElasticIndexService()
        {
            client = ElasticConfig.GetClient();
        }

        public void CreateIndex(List<Worktask> list)
        {
            if (!client.IndexExists(ElasticConfig.IndexName).Exists)
            {
                var indexDescriptor = new CreateIndexDescriptor(ElasticConfig.IndexName)
                    .Mappings(ms => ms
                    .Map<Worktask>((TypeMappingDescriptor<Worktask> m) => m.AutoMap())
                    .Map<Domain.Entities.Attachment>((TypeMappingDescriptor<Domain.Entities.Attachment> m) => m.AutoMap())
                    
                );

                client.CreateIndex(ElasticConfig.IndexName, i=> indexDescriptor);
            } else
            {
                var res = client.LowLevel.IndicesGetMapping<Worktask>(ElasticConfig.IndexName);
                var del = client.DeleteIndex(new DeleteIndexRequest(ElasticConfig.IndexName));
                var indexDescriptor = new CreateIndexDescriptor(ElasticConfig.IndexName)
                    .Mappings(ms => ms
                    .Map<Worktask>((TypeMappingDescriptor<Worktask> m) => m.AutoMap())
                    .Map<Domain.Entities.Attachment>((TypeMappingDescriptor<Domain.Entities.Attachment> m) => m.AutoMap())

                );

                client.CreateIndex(ElasticConfig.IndexName, i => indexDescriptor);
            }
           
            foreach (Worktask item in list)
            {
                try
                {
                    client.Index(item);
                }
                catch
                {
                    throw;
                }

            }
        }

        public bool CreateSingleIndex(Worktask worktask)
        {
            try
            {
                var result = client.Index(worktask);
                return result.IsValid;
            }
            catch
            {
                return false;
            }
        }


        public void DeleteIndex(string indexName)
        {
            if (client.IndexExists(ElasticConfig.IndexName).Exists)
            {
                client.DeleteIndex(ElasticConfig.IndexName);
            }
        }

        public void DeleteId(Worktask worktask)
        {
            var bulkResponse = client.Delete<Worktask>("AVegMX6OEI1lsaTKVC2v");
        }
    }
}