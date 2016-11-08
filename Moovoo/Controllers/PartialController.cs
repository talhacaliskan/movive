using Moovoo.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace Moovoo.Controllers
{
    public class PartialController : Controller
    {
        private readonly TMDbClient _client = new TMDbClient("a4e41166de745c41026cd38600d63901");
        private readonly UserConcrete _uc = new UserConcrete();

        // GET: Partial
        [ChildActionOnly]
        public ActionResult TopRated()
        {
            var top = _client.GetTopRatedMovie(1, "en-US").Results.Take(8).ToList();
            return PartialView(top);
        }
        [ChildActionOnly]
        public ActionResult Upcoming()
        {

            var upcoming = _client.GetMovieList(MovieListType.Upcoming, "en-US", 1).Results.Where(p=>p.ReleaseDate>=DateTime.Now.ToLocalTime()).OrderBy(p=>p.ReleaseDate).ToList();
            
            return PartialView(upcoming);
        }
        [HttpGet]
        [ChildActionOnly]
        public ActionResult Recommended(int id)
        {
            var rec = _client.GetMovieRecommended(id, 1).Results.Where(p=>p.ReleaseDate<DateTime.Now).ToList();
            return PartialView(rec);
        }

        [ChildActionOnly]
        public ActionResult User()
        {
            if (Session["Email"] != null)
            {
                var user =_uc.GetUser(Session["Email"].ToString());
                return PartialView(user);
            }
            else
            {
                return null;
            }
            
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");

        }
    }
}