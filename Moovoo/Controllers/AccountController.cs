using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using System.Web.Security;
using Moovoo.Models;
using Moovoo.Concrete;
using System.Threading.Tasks;
using System.Threading;



namespace Moovoo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserConcrete _uc = new UserConcrete();
        // GET: Account
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return View("Login");
        }
        [HttpPost]
        public ActionResult SignUp(User user)
        {
            var response = _uc.SignUp(user);
            if (response == true)
            {
                Session["Email"] = user.Email;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

        }
        [HttpPost]
        public ActionResult SignIn(User user)
        {
            var response = _uc.SignIn(user);
            if (response == true)
            {
                Session["Email"] = user.Email;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
            

        }
        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url)
                {
                    Query = null,
                    Fragment = null,
                    Path = Url.Action("FacebookCallback")
                };
                return uriBuilder.Uri;
            }
        }
        [AllowAnonymous]
        public ActionResult Facebook()
        {
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = "1663499330647449",
                client_secret = "857f9bc2914d07a2b2c503b7df26e3a1",
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email"
            });
            return Redirect(loginUrl.AbsoluteUri);
        }
        public ActionResult FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = "1663499330647449",
                client_secret = "857f9bc2914d07a2b2c503b7df26e3a1",
                redirect_uri = RedirectUri.AbsoluteUri,
                code = code
            });
            var accessToken = result.access_token;
            Session["AccessToken"] = accessToken;
            fb.AccessToken = accessToken;
            dynamic me = fb.Get("me?fields=first_name,middle_name,last_name,id,email,picture");
            var usr = new User
            {
                FirstName = me.first_name,
                MiddleName = me.middle_name,
                LastName = me.last_name,
                Email = me.email,
                AccessToken = fb.AccessToken,
                SocialMediaAccountId = me.id,
            };
            var firstResponse = _uc.SignIn(usr);
            if (firstResponse == true)
            {
                Session["Email"] = usr.Email;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var response = _uc.SignUp(usr);
                if (response == true)
                {
                    Session["Email"] = usr.Email;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }



            //// Set the auth cookie

            //FormsAuthentication.SetAuthCookie(me.email, false);

        }

        }
    }
