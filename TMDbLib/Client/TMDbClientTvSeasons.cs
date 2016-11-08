﻿using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RestSharp;
using TMDbLib.Objects.Authentication;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.TvShows;
using TMDbLib.Utilities;
using Credits = TMDbLib.Objects.TvShows.Credits;

namespace TMDbLib.Client
{
    public partial class TMDbClient
    {
        /// <summary>
        /// Retrieve a season for a specifc tv Show by id.
        /// </summary>
        /// <param name="tvShowId">TMDb id of the tv show the desired season belongs to.</param>
        /// <param name="seasonNumber">The season number of the season you want to retrieve. Note use 0 for specials.</param>
        /// <param name="extraMethods">Enum flags indicating any additional data that should be fetched in the same request.</param>
        /// <param name="language">If specified the api will attempt to return a localized result. ex: en,it,es </param>
        /// <returns>The requested season for the specified tv show</returns>
        public TvSeason GetTvSeason(int tvShowId, int seasonNumber, TvSeasonMethods extraMethods = TvSeasonMethods.Undefined, string language = null)
        {
            if (extraMethods.HasFlag(TvSeasonMethods.AccountStates))
                RequireSessionId(SessionType.UserSession);

            RestRequest request = new RestRequest("tv/{id}/season/{season_number}");
            request.AddUrlSegment("id", tvShowId.ToString(CultureInfo.InvariantCulture));
            request.AddUrlSegment("season_number", seasonNumber.ToString(CultureInfo.InvariantCulture));

            if (extraMethods.HasFlag(TvSeasonMethods.AccountStates))
                request.AddParameter("session_id", SessionId);

            language = language ?? DefaultLanguage;
            if (!String.IsNullOrWhiteSpace(language))
                request.AddParameter("language", language);

            string appends = string.Join(",",
                                         Enum.GetValues(typeof(TvSeasonMethods))
                                             .OfType<TvSeasonMethods>()
                                             .Except(new[] { TvSeasonMethods.Undefined })
                                             .Where(s => extraMethods.HasFlag(s))
                                             .Select(s => s.GetDescription()));

            if (appends != string.Empty)
                request.AddParameter("append_to_response", appends);

            IRestResponse<TvSeason> response = _client.Get<TvSeason>(request);

            // Nothing to patch up
            if (response.Data == null)
                return null;

            if (response.Data.Episodes != null)
                response.Data.EpisodeCount = response.Data.Episodes.Count;

            if (response.Data.Credits != null)
                response.Data.Credits.Id = response.Data.Id ?? 0;

            if (response.Data.ExternalIds != null)
                response.Data.ExternalIds.Id = response.Data.Id ?? 0;

            if (response.Data.AccountStates != null)
            {
                response.Data.AccountStates.Id = response.Data.Id ?? 0;
                // Do some custom deserialization, since TMDb uses a property that changes type we can't use automatic deserialization
                CustomDeserialization.DeserializeAccountStatesRating(response.Data.AccountStates, response.Content);
            }

            return response.Data;
        }

        /// <summary>
        /// Returns a credits object for the season of the tv show associated with the provided TMDb id.
        /// </summary>
        /// <param name="tvShowId">The TMDb id of the target tv show.</param>
        /// <param name="seasonNumber">The season number of the season you want to retrieve information for. Note use 0 for specials.</param>
        /// <param name="language">If specified the api will attempt to return a localized result. ex: en,it,es </param>
        public Credits GetTvSeasonCredits(int tvShowId, int seasonNumber, string language = null)
        {
            return GetTvSeasonMethod<Credits>(tvShowId, seasonNumber, TvSeasonMethods.Credits, dateFormat: "yyyy-MM-dd", language: language);
        }

        /// <summary>
        /// Retrieves all images all related to the season of specified tv show.
        /// </summary>
        /// <param name="tvShowId">The TMDb id of the target tv show.</param>
        /// <param name="seasonNumber">The season number of the season you want to retrieve information for. Note use 0 for specials.</param>
        /// <param name="language">
        /// If specified the api will attempt to return a localized result. ex: en,it,es.
        /// For images this means that the image might contain language specifc text
        /// </param>
        public PosterImages GetTvSeasonImages(int tvShowId, int seasonNumber, string language = null)
        {
            return GetTvSeasonMethod<PosterImages>(tvShowId, seasonNumber, TvSeasonMethods.Images, language: language);
        }

        public ResultContainer<Video> GetTvSeasonVideos(int tvShowId, int seasonNumber, string language = null)
        {
            return GetTvSeasonMethod<ResultContainer<Video>>(tvShowId, seasonNumber, TvSeasonMethods.Videos, language: language);
        }

        /// <summary>
        /// Returns an object that contains all known exteral id's for the season of the tv show related to the specified TMDB id.
        /// </summary>
        /// <param name="tvShowId">The TMDb id of the target tv show.</param>
        /// <param name="seasonNumber">The season number of the season you want to retrieve information for. Note use 0 for specials.</param>
        public ExternalIds GetTvSeasonExternalIds(int tvShowId, int seasonNumber)
        {
            return GetTvSeasonMethod<ExternalIds>(tvShowId, seasonNumber, TvSeasonMethods.ExternalIds);
        }

        public ResultContainer<TvEpisodeAccountState> GetTvSeasonAccountState(int tvShowId, int seasonNumber)
        {
            RequireSessionId(SessionType.UserSession);

            RestRequest req = new RestRequest("tv/{id}/season/{season_number}/account_states");
            req.AddUrlSegment("id", tvShowId.ToString(CultureInfo.InvariantCulture));
            req.AddUrlSegment("season_number", seasonNumber.ToString(CultureInfo.InvariantCulture));
            req.AddUrlSegment("method", TvEpisodeMethods.AccountStates.GetDescription());
            req.AddParameter("session_id", SessionId);

            IRestResponse<ResultContainer<TvEpisodeAccountState>> response = _client.Get<ResultContainer<TvEpisodeAccountState>>(req);

            // Do some custom deserialization, since TMDb uses a property that changes type we can't use automatic deserialization
            if (response.Data != null)
            {
                CustomDeserialization.DeserializeAccountStatesRating(response.Data, response.Content);
            }

            return response.Data;
        }

        public ChangesContainer GetTvSeasonChanges(int seasonId)
        {
            RestRequest req = new RestRequest("tv/season/{id}/changes");
            req.AddUrlSegment("id", seasonId.ToString(CultureInfo.InvariantCulture));

            IRestResponse<ChangesContainer> response = _client.Get<ChangesContainer>(req);

            return response.Data;
        }

        private T GetTvSeasonMethod<T>(int tvShowId, int seasonNumber, TvSeasonMethods tvShowMethod, string dateFormat = null, string language = null) where T : new()
        {
            RestRequest req = new RestRequest("tv/{id}/season/{season_number}/{method}");
            req.AddUrlSegment("id", tvShowId.ToString(CultureInfo.InvariantCulture));
            req.AddUrlSegment("season_number", seasonNumber.ToString(CultureInfo.InvariantCulture));
            req.AddUrlSegment("method", tvShowMethod.GetDescription());

            if (dateFormat != null)
                req.DateFormat = dateFormat;

            language = language ?? DefaultLanguage;
            if (!String.IsNullOrWhiteSpace(language))
                req.AddParameter("language", language);

            IRestResponse<T> resp = _client.Get<T>(req);

            return resp.Data;
        }
    }
}
