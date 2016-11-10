using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;

namespace Moovoo.Controllers
{
    public class MovieController : Controller
    {
        private readonly TMDbClient _client = new TMDbClient("a4e41166de745c41026cd38600d63901");

        // GET: Movie
        public ActionResult Movie(int id)
        {
            var movie = _client.GetMovie(id, MovieMethods.Images | MovieMethods.Credits | MovieMethods.Videos | MovieMethods.Similar);

            return View(movie);
        }
        public ActionResult Discover()
        {
            var top = _client.GetTopRatedMovie(1, null).Results;
            var pop = _client.GetPopularMovie(1, null).Results;
            return View(top);

        }
        [HttpGet]
        public ActionResult MoreRecommended(int id)
        {
            var rec = _client.GetMovie(id).Genres;
            var movie = rec.SelectMany(item => _client.GetGenreMovies(item.Id, 1).Results).ToList();
            var movieNoDuplicate = movie.GroupBy(p => p.Id).Select(p => p.FirstOrDefault()).OrderBy(p => p.Id).ToList();
            return View(movieNoDuplicate);
        }

        [HttpGet]
        public ActionResult CastMovies(int id)
        {
            var a = _client.GetPersonMovieCredits(id).Cast;
            return View(a);
        }
    }
}