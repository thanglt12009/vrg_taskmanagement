using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Domain.Entities;

namespace TMS.WebApp.Models
{
    public class SearchResultItem<T>
    {
        public HighlightFieldDictionary Highlight { get; set; }
        public T Source { get; set; }
        public SearchResultItem(HighlightFieldDictionary highlights, T source)
        {
            this.Highlight = highlights;
            this.Source = source;
        }
    }
    public class SearchResult<T>
    {
        public int Total { get; set; }

        public int Page { get; set; }

        public bool IsValid { get; set; }

        public IEnumerable<T> Results { get; set; }

        public IEnumerable<SearchResultItem<T>> Highlights { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public Dictionary<string, long> AggregationsByTags { get; set; }
    }
}