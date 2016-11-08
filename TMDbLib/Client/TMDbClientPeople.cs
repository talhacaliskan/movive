﻿using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Utilities;

namespace TMDbLib.Client
{
    public partial class TMDbClient
    {
        public Person GetPerson(int personId, PersonMethods extraMethods = PersonMethods.Undefined)
        {
            RestRequest req = new RestRequest("person/{personId}");
            req.AddUrlSegment("personId", personId.ToString());

            string appends = string.Join(",",
                                         Enum.GetValues(typeof(PersonMethods))
                                             .OfType<PersonMethods>()
                                             .Except(new[] { PersonMethods.Undefined })
                                             .Where(s => extraMethods.HasFlag(s))
                                             .Select(s => s.GetDescription()));

            if (appends != string.Empty)
                req.AddParameter("append_to_response", appends);

            req.DateFormat = "yyyy-MM-dd";

            IRestResponse<Person> resp = _client.Get<Person>(req);

            // Patch up data, so that the end user won't notice that we share objects between request-types.
            if (resp.Data != null)
            {
                if (resp.Data.Images != null)
                    resp.Data.Images.Id = resp.Data.Id;

                if (resp.Data.Credits != null)
                    resp.Data.Credits.Id = resp.Data.Id;
            }

            return resp.Data;
        }

        private T GetPersonMethod<T>(int personId, PersonMethods personMethod, string dateFormat = null, string country = null, string language = null,
                                        int page = 0, DateTime? startDate = null, DateTime? endDate = null) where T : new()
        {
            RestRequest req = new RestRequest("person/{personId}/{method}");
            req.AddUrlSegment("personId", personId.ToString());
            req.AddUrlSegment("method", personMethod.GetDescription());

            if (dateFormat != null)
                req.DateFormat = dateFormat;

            if (country != null)
                req.AddParameter("country", country);
            language = language ?? DefaultLanguage;
            if (!String.IsNullOrWhiteSpace(language))
                req.AddParameter("language", language);

            if (page >= 1)
                req.AddParameter("page", page);
            if (startDate.HasValue)
                req.AddParameter("startDate", startDate.Value.ToString("yyyy-MM-dd"));
            if (endDate != null)
                req.AddParameter("endDate", endDate.Value.ToString("yyyy-MM-dd"));

            IRestResponse<T> resp = _client.Get<T>(req);

            return resp.Data;
        }

        public MovieCredits GetPersonMovieCredits(int personId)
        {
            return GetPersonMovieCredits(personId, DefaultLanguage);
        }

        public MovieCredits GetPersonMovieCredits(int personId, string language)
        {
            return GetPersonMethod<MovieCredits>(personId, PersonMethods.MovieCredits, language: language);
        }

        public TvCredits GetPersonTvCredits(int personId)
        {
            return GetPersonTvCredits(personId, DefaultLanguage);
        }

        public TvCredits GetPersonTvCredits(int personId, string language)
        {
            return GetPersonMethod<TvCredits>(personId, PersonMethods.TvCredits, language: language);
        }

        public ProfileImages GetPersonImages(int personId)
        {
            return GetPersonMethod<ProfileImages>(personId, PersonMethods.Images);
        }

        public SearchContainer<TaggedImage> GetPersonTaggedImages(int personId, int page)
        {
            return GetPersonTaggedImages(personId, DefaultLanguage, page);
        }

        public SearchContainer<TaggedImage> GetPersonTaggedImages(int personId, string language, int page)
        {
            return GetPersonMethod<SearchContainer<TaggedImage>>(personId, PersonMethods.TaggedImages, language: language, page: page);
        }

        public ExternalIds GetPersonExternalIds(int personId)
        {
            return GetPersonMethod<ExternalIds>(personId, PersonMethods.ExternalIds);
        }

        public List<Change> GetPersonChanges(int personId, DateTime? startDate = null, DateTime? endDate = null)
        {
            ChangesContainer changesContainer = GetPersonMethod<ChangesContainer>(personId, PersonMethods.Changes, startDate: startDate, endDate: endDate, dateFormat: "yyyy-MM-dd HH:mm:ss UTC");
            return changesContainer.Changes;
        }

        public SearchContainer<PersonResult> GetPersonList(PersonListType type, int page = 0)
        {
            RestRequest req;
            switch (type)
            {
                case PersonListType.Popular:
                    req = new RestRequest("person/popular");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            if (page >= 1)
                req.AddParameter("page", page.ToString());

            req.DateFormat = "yyyy-MM-dd";

            IRestResponse<SearchContainer<PersonResult>> resp = _client.Get<SearchContainer<PersonResult>>(req);

            return resp.Data;
        }

        public Person GetLatestPerson()
        {
            RestRequest req = new RestRequest("person/latest");

            req.DateFormat = "yyyy-MM-dd";

            IRestResponse<Person> resp = _client.Get<Person>(req);

            return resp.Data;
        }
    }
}