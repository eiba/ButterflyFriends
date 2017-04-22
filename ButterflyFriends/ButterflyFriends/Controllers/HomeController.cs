using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Models;
using Newtonsoft.Json;

namespace ButterflyFriends.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View(new FrontPageModel { Articles = _context.Articles.Where(s => s.Published).ToList()});
        }

        [HttpGet]
        public ActionResult Article(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var article = _context.Articles.Find(id);
            if (article == null || !article.Published)
            {
                return HttpNotFound();
            }

            return View(article);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult RequestMembership()
        {
            ViewBag.Message = "Forespør Medlemskap.";

            return View(new DbTables.MembershipRequest());
        }

        [HttpPost]
        public ActionResult RequestMembership(DbTables.MembershipRequest model)
        {
            
            if (ModelState.IsValid) {
                var encodedResponse = Request.Form["g-Recaptcha-Response"];
                var isCaptchaValid = ReCaptcha.Validate(encodedResponse);

                if (!isCaptchaValid)
                {
                    
                    ViewBag.Error = "Recaptcha feilet";
                    ViewBag.Reset = "false";
                    return PartialView("_statusPartial");
                }
                try
            {
                _context.MembershipRequests.Add(model);
                _context.SaveChanges();
                ViewBag.Success = "Din forespørsel ble suksessfult motatt, vi kontakter deg så snart vi kan";
                ViewBag.Reset = "true";
                return PartialView("_statusPartial");
            }
            catch (EntityException ex)
            {

                ViewBag.Error = "Noe gikk galt" + ex.Message;
                    ViewBag.Reset = "false";
                    return PartialView("_statusPartial");
            }
            }
            string messages = string.Join(" ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));

            ViewBag.Error = "Ugyldige verdier: " + messages;
            ViewBag.Reset = "false";
            return PartialView("_statusPartial");
        }
    }
    public class ReCaptcha
    {
        public bool Success { get; set; }
        public List<string> ErrorCodes { get; set; }

        public static bool Validate(string encodedResponse)
        {
            if (string.IsNullOrEmpty(encodedResponse)) return false;

            var client = new System.Net.WebClient();
            var secret = "6LdechwUAAAAAMgUKGwZntwpCT8PksWPETIBvyCi";

            if (string.IsNullOrEmpty(secret)) return false;

            var googleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, encodedResponse));

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            var reCaptcha = serializer.Deserialize<ReCaptcha>(googleReply);

            return reCaptcha.Success;
        }
    }
}