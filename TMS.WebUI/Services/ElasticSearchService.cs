using System;
using System.Collections.Generic;
using Nest;
using TMS.Domain.Entities;
using TMS.WebApp.Models;
using System.Linq;

namespace TMS.WebApp.Services
{
    public class ElasticSearchService : ISearchService<Worktask>
    {
        private readonly IElasticClient client;

        public ElasticSearchService()
        {
            client = ElasticConfig.GetClient();
        }

        public SearchResult<Worktask> Search(string username, string query, int page, int pageSize)
        {
            int[] boardIdList = KanbanService.GetInstance().UserBoardList(username).Select(b => b.Id).ToArray();
            var search = new SearchRequest
            {
                Query = new MultiMatchQuery
                {
                    Query = query,
                    Fields = Infer.Fields("attachments.attachment_content", "task_title", "task_description", "task_identify")
                },

                Highlight = new Highlight
                {
                    PreTags = new[] { "<sr>" },
                    PostTags = new[] { "</sr>" },
                    Fields = new Dictionary<Field, IHighlightField>
                    {
                        { "*", new HighlightField {} }
                    }
                }
            };

            var result = client.Search<Worktask>(search);
            var highlights = result.Hits.Select(h => new SearchResultItem<Worktask>( h.Highlights, h.Source)).ToList();

            return new SearchResult<Worktask>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents.Where(d => boardIdList.Any(i => i == d.BoardID)).ToList(),
                Highlights = highlights.Where(d => boardIdList.Any(i => i == d.Source.BoardID)),
                ElapsedMilliseconds = result.Took,
                IsValid = result.IsValid
            };
        }

        public Worktask Get(string id)
        {
            var result = client.Get<Worktask>(new DocumentPath<Worktask>(id));
            return result.Source;
        }

        public IEnumerable<string> Autocomplete(string query)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Suggest(string query)
        {
            throw new NotImplementedException();
        }

        public SearchResult<Worktask> FindMoreLikeThis(string query, int pageSize)
        {
            throw new NotImplementedException();
        }

        public SearchResult<Worktask> SearchByCategory(string query, IEnumerable<string> tags, int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}