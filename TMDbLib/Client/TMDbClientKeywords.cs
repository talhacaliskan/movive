﻿using System;
using RestSharp;
using TMDbLib.Objects.General;

namespace TMDbLib.Client
{
    public partial class TMDbClient
    {
        public Keyword GetKeyword(int keywordId)
        {
            RestRequest req = new RestRequest("keyword/{keywordId}");
            req.AddUrlSegment("keywordId", keywordId.ToString());

            IRestResponse<Keyword> resp = _client.Get<Keyword>(req);

            return resp.Data;
        }

        public SearchContainer<MovieResult> GetKeywordMovies(int keywordId, int page = 0)
        {
            return GetKeywordMovies(keywordId, DefaultLanguage, page);
        }

        public SearchContainer<MovieResult> GetKeywordMovies(int keywordId, string language, int page = 0)
        {
            RestRequest req = new RestRequest("keyword/{keywordId}/movies");
            req.AddUrlSegment("keywordId", keywordId.ToString());

            language = language ?? DefaultLanguage;
            if (!String.IsNullOrWhiteSpace(language))
                req.AddParameter("language", language);

            if (page >= 1)
                req.AddParameter("page", page);

            IRestResponse<SearchContainer<MovieResult>> resp = _client.Get<SearchContainer<MovieResult>>(req);

            return resp.Data;
        }
    }
}