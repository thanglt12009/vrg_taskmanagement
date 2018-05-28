using System.Collections.Generic;
using TMS.WebApp.Models;
using TMS.Domain.Entities;

namespace TMS.WebApp.Services
{
    public interface ISearchService<T>
    {
        SearchResult<T> Search(string username, string query, int page = 1, int pageSize = 10);

        IEnumerable<string> Autocomplete(string query);

        IEnumerable<string> Suggest(string query);

        SearchResult<T> FindMoreLikeThis(string query, int pageSize);

        SearchResult<Worktask> SearchByCategory(string query, IEnumerable<string> tags, int page, int pageSize);

        T Get(string id);
    }
}