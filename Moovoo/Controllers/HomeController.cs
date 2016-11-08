using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.Search;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Discover;

namespace Moovoo.Controllers
{

    public class HomeController : Controller
    {
        readonly TMDbClient _client = new TMDbClient("a4e41166de745c41026cd38600d63901");

        // GET: Home
        
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string query)
        {
            var movie = _client.SearchMovie(query).Results;


            return View("Index", movie);
        }
        [HttpPost]
        public JsonResult Search(string term)
        {

            var movie = _client.SearchMovie(term).Results;
            var name = movie.Take(5).Select(r => new { id = r.Id, title = r.OriginalTitle });
            return Json(name, JsonRequestBehavior.AllowGet);
        }
        

        




    }
}