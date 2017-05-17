using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using PagedList;

namespace ButterflyFriends.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        public int imageNum = 3;
        public int articleNum = 3;
        public ActionResult Index()
        {
            var articles = filterArticles(0, articleNum);
            var carousel = _context.Carousel.ToList();
            IList<CarouselObject> carouselList = new List<CarouselObject>();
            if (carousel.Any() && carousel.First().Enabeled)
            {
                foreach (var file in carousel.First().CarouselItems)
                {
                    if (file.FileType == DbTables.FileType.CarouselImage)
                    {
                        carouselList.Add(new CarouselObject { id = file.FileId, type = "image" });
                    }
                    else
                    {
                        carouselList.Add(new CarouselObject { id = file.FileId, type = "video" });

                    }
                }
            }
            else
            {
                carouselList = null;
            }
            var About = new DbTables.Info();
            var AboutList = _context.About.ToList();
            if (AboutList.Any())
            {
                About = AboutList.First();
            }
            var background = new DbTables.BackgroundImage();
            var backgroundList = _context.BackgroundImage.ToList();
            if (backgroundList.Any())
            {
                background = backgroundList.First();
                if (background.Enabeled)
                {
                    ViewBag.Style = "background:url('/File/Background?id=" + @background.Image.FileId +
                                   "') no-repeat center center fixed;-webkit-background-size: cover;-moz-background-size: cover;-o-background-size: cover;background-size: cove;overflow-x: hidden;";
                    ViewBag.BackGround = "background-color:transparent;";
                }
            }
            var stripeList = _context.StripeAPI.ToList();
            if (stripeList.Any())
            {
                ViewBag.StripePublic = stripeList.First().Public;
            }

            var model = new FrontPageModel
            {
                Articles = articles,
                Carousel = carouselList,
                About = About
            };
            return View(model);

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
            var background = new DbTables.BackgroundImage();
            var backgroundList = _context.BackgroundImage.ToList();
            if (backgroundList.Any())
            {
                background = backgroundList.First();
                if (background.Enabeled)
                {
                    ViewBag.Style = "background:url('/File/Background?id=" + @background.Image.FileId +
                                   "') no-repeat center center fixed;-webkit-background-size: cover;-moz-background-size: cover;-o-background-size: cover;background-size: cove;overflow-x: hidden;";
                    ViewBag.BackGround = "background-color:transparent;";
                }
            }

            return View(article);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Om oss";

            var AboutList = _context.About.ToList();
            var About = new DbTables.Info();
            if (AboutList.Any())
            {
                About = AboutList.First();
            }
            var TwitterList = _context.Twitter.ToList();
            var Twitter = new DbTables.Twitter();
            if (TwitterList.Any())
            {
                Twitter = TwitterList.First();
            }
            var FacebookList = _context.Facebook.ToList();
            var Facebook = new DbTables.Facebook();
            if (FacebookList.Any())
            {
                Facebook = FacebookList.First();
            }
            //var style = "background:url(/File/Background?id="+") no-repeat center center fixed";

            var background = new DbTables.BackgroundImage();
            var backgroundList = _context.BackgroundImage.ToList();
            if (backgroundList.Any())
            {
                background = backgroundList.First();
                if (background.Enabeled)
                {
                    ViewBag.Style = "background:url('/File/Background?id=" + @background.Image.FileId +
                                    "') no-repeat center center fixed;-webkit-background-size: cover;-moz-background-size: cover;-o-background-size: cover;background-size: cove;overflow-x: hidden;";
                    ViewBag.BackGround = "background-color:transparent;";
                }
            }
            var AboutModel = new AboutModel
            {
                About = About,
                Facebook = Facebook,
                Twitter = Twitter
            };
            return View(AboutModel);
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
            
            var GoogleCaptcha = _context.GoogleCaptchaAPI.First();
            var model = new RequestModel
            {
                MembershipRequest = new DbTables.MembershipRequest(),
                SiteKey = GoogleCaptcha.SiteKey
            };
            var terms = _context.TermsOfUse.ToList();
            var Terms = new DbTables.TermsOfUse();
            if (terms.Any())
            {
                Terms = terms.First();
                if (Terms.Enabeled && Terms.Terms.FileId != 0) { 

                    model.TermsID = Terms.Terms.FileId;
                }
            }
            var background = new DbTables.BackgroundImage();
            var backgroundList = _context.BackgroundImage.ToList();
            if (backgroundList.Any())
            {
                background = backgroundList.First();
                if (background.Enabeled)
                {
                    ViewBag.Style = "background:url('/File/Background?id=" + @background.Image.FileId +
                                   "') no-repeat center center fixed;-webkit-background-size: cover;-moz-background-size: cover;-o-background-size: cover;background-size: cove;overflow-x: hidden;";
                    ViewBag.BackGround = "background-color:transparent;";
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult RequestMembership(RequestModel model)
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
                _context.MembershipRequests.Add(model.MembershipRequest);
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
            string messages = string.Join("\n", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));

            ViewBag.Error = "Ugyldige verdier: " + messages;
            ViewBag.Reset = "false";
            return PartialView("_statusPartial");

        }

        [Authorize(Roles = "Eier, Admin, Ansatt, Fadder")]
        public ActionResult MyImages()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            if (currentUser == null)
            {
                return HttpNotFound();
            }
            var startId = 0;                                //id of first image i client
            var images = filterImages(currentUser, startId,imageNum);
            var background = new DbTables.BackgroundImage();
            var backgroundList = _context.BackgroundImage.ToList();
            if (backgroundList.Any())
            {
                background = backgroundList.First();
                if (background.Enabeled)
                {
                    ViewBag.Style = "background:url('/File/Background?id=" + @background.Image.FileId +
                                   "') no-repeat center center fixed;-webkit-background-size: cover;-moz-background-size: cover;-o-background-size: cover;background-size: cove;overflow-x: hidden;";
                    ViewBag.BackGround = "background-color:transparent;";
                }
            }
            return View(new MyImagesModel
            {
                Images = images,
                StartId = 0
            });
        }

        public IList<DbTables.File> filterImages(ApplicationUser User,int startId, int imageNum)
        {
            List<DbTables.File> images = new List<DbTables.File>();
            IList<int> ids = new List<int>();
            if (User.Pictures.Any())
            {
                foreach (var picture in User.Pictures)
                {
                    if (picture.Published) { 
                    images.Add(picture);
                    ids.Add(picture.FileId);
                    }
                }
            }

            foreach (var child in User.Children)
            {
                foreach (var picture in child.Pictures)
                {
                    if (!ids.Contains(picture.FileId) && picture.Published)
                    {
                        images.Add(picture);
                        ids.Add(picture.FileId);
                    }
                }
            }
            images = images.OrderByDescending(s => s.UploadDate.Value).ThenByDescending(s => s.FileId).ToList();
            if (startId+imageNum <= images.Count)    //check if there's enough images to take from
            {
                images = images.GetRange(startId, imageNum);

            }
            else if (startId +1 > images.Count) //return nothing as startid is as great as there are elements in image list
            {
                images = null;
            }
            else if(imageNum >= images.Count)
            {
                //do nothing
            }
            else
            {
                images = images.GetRange(startId, images.Count -startId);
            }
            return images;
        }

        public IList<DbTables.Article> filterArticles(int startId, int articleNum)
        {
            var articles = _context.Articles.Where(s => s.Published).OrderByDescending(s => s.FirstPublisheDateTime).ThenByDescending(s => s.Id).ToList();
            if (startId + articleNum <= articles.Count)    //check if there's enough images to take from
            {
                articles = articles.GetRange(startId, articleNum);

            }
            else if (startId +1  > articles.Count) //return nothing as startid is as great as there are elements in image list
            {
                articles = null;
            }
            else if (articleNum >= articles.Count)  //return whole list as there are as many articles as there is articles on one page
            {
                //do nothing
            }
            else
            {
                articles = articles.GetRange(startId, articles.Count - startId);
            }
            return articles;
        }
        [HttpPost]
        public ActionResult GetImages()
        {
            var startId = int.Parse(Request.Form["startid"]);
            if(startId < imageNum) { 
            return null;
            }
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            var images = filterImages(currentUser, startId, imageNum);

            return PartialView("_MyImagesPartial", new MyImagesModel {Images = images,StartId = startId});
        }

        [HttpPost]
        public ActionResult GetArticles()
        {
            var startId = int.Parse(Request.Form["startid"]);
            if (startId < articleNum)
            {
                return null;
            }
            
            var articles = filterArticles(startId, articleNum);

            return PartialView("_ArticlesPartial", articles);
        }

        [HttpPost]
        public ActionResult HandlePayment()
        {
            
            var type = Request.Form["type"];
            var amount = int.Parse(Request.Form["amount"]);
            var anon = Request.Form["anon"];
            var user = Request.Form["user"];
            var token = Request.Form["token"];
            var email = Request.Form["email"];
            var phone = Request.Form["phone"];
            var city = Request.Form["city"];
            var streetadress = Request.Form["streetadress"];
            var postcode = Request.Form["postcode"];
            var birthnumber = Request.Form["birthnumber"];
            var name = Request.Form["name"];
            var description = Request.Form["description"];

            var recieptemail = "";
            var recieptname = "";
            var donation = new DbTables.Donations();

            if (anon == "true")
            {
                donation = new DbTables.Donations
                {
                    Amount = amount,
                    Description = description,
                    anonymous = true
                };
            }
            else if (user == "true" && User.Identity.GetUserId() != null)
            {
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
                ApplicationUser currentUser = manager.FindById(User.Identity.GetUserId());
                donation = new DbTables.Donations
                {
                    Amount = amount,
                    Description = description,
                    anonymous = false,
                    User = currentUser
                };
                recieptname = currentUser.Fname + " " + currentUser.Lname;
                recieptemail = currentUser.Email;
            }
            else
            {
                donation = new DbTables.Donations
                {
                    Amount = amount,
                    Email = email,
                    Phone = phone,
                    City = city,
                    StreetAdress = streetadress,
                    ZipCode = postcode,
                    BirthNumber = birthnumber,
                    Name = name,
                    Description = description,
                    anonymous = false
                };
                recieptemail = email;
                recieptname = name;
            }
            _context.Donations.Add(donation);
            _context.SaveChanges();

            // Process payment.
            var client = new WebClient();

            var data = new NameValueCollection();
            data["amount"] = (amount*100).ToString(CultureInfo.InvariantCulture); // Stripe charges are øre-based in NOK, so 100x the price.
            data["currency"] = "nok";
            data["source"] = token;
            data["description"] = "Donasjon "+donation.Id +": "+description;

            if (!string.IsNullOrEmpty(email)) { 
                data["receipt_email"] = email;
            }else if (user == "true")
            {
                data["receipt_email"]=donation.User.Email;
            }
            client.UseDefaultCredentials = true;

            var stripeList = _context.StripeAPI.ToList();
            if (!stripeList.Any())
            {
                return Json(new { Error = "Stripe er ikke konfigurert for applikasjonen.",Succsess="false", striperesponse ="false"});
            }

            client.Credentials = new NetworkCredential(_context.StripeAPI.ToList().First().Secret, "");

            try
            {
                client.UploadValues("https://api.stripe.com/v1/charges", "POST", data);

            }
            catch (WebException exception)
            {
                string responseString;
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    responseString = reader.ReadToEnd();
                }

                return Json(new { Error = responseString ,Success="false",striperesponse="true"});
            }

            // If we got this far, there were no errors, and we set the order to paid, and save.
            Response.StatusCode = 200;
            donation.isPaid = true;
            _context.SaveChanges();
            if (!string.IsNullOrEmpty(recieptemail))
            {
                var subject = "Kvitering på donasjon";
                var message = "Takk for din støtte! \n Du har donert "+amount+" kroner til Butterfly Friends. \n"+"Ditt referansenummer er "+donation.Id+". \n\n"+"Vennlig hilsen,\nButterfly Friends.";
                var messageHTML = "<p>Takk for din støtte! <br> Du har donert "+amount+" kroner til Butterfly Friends. <br>"+"Ditt referansenummer er "+donation.Id+". <br><br>"+"Vennlig hilsen,<br>Butterfly Friends.</p>";
                if(!SendEmail(message, messageHTML, subject, recieptemail, recieptname))
                {
                    ViewBag.Error = "Emailkviteringen kunne ikke sendes";
                }
            }

            ViewBag.Share = "https://www." + Request.Url.Host + "/Home/Index";
            ViewBag.ShareText = "Jeg har donert "+amount+" kr. til Butterfly Friends!";

            var TwitterList = _context.Twitter.ToList();
            var Twitter = new DbTables.Twitter();
            if (TwitterList.Any())
            {
                Twitter = TwitterList.First();
            }
            var FacebookList = _context.Facebook.ToList();
            var Facebook = new DbTables.Facebook();
            if (FacebookList.Any())
            {
                Facebook = FacebookList.First();
            }

            var model = new RecieptModel
            {
                Facebook = Facebook,
                Twitter = Twitter,
                Donation = donation
            };
            return PartialView("_RecieptPartial", model);
        }

        public bool SendEmail(string message, string messageHTML, string subject, string recieverEmail, string recieverName)
        {

            try
            {
                MailMessage mailMsg = new MailMessage();

                // To
                mailMsg.To.Add(new MailAddress(recieverEmail, recieverName));

                // From
                mailMsg.From = new MailAddress("noreply@butterflyfriends.com", "Butterfly Friends");

                // Subject and multipart/alternative Body

                    mailMsg.Subject = subject;
                    if (message != "")
                    {
                        string text = message;
                        string html = @messageHTML;
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null,
                            MediaTypeNames.Text.Plain));
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null,
                            MediaTypeNames.Text.Html));
                    }

                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                var SendGridAPIList = _context.SendGridAPI.ToList();
                var SendGridAPI = new DbTables.SendGridAPI();
                if (SendGridAPIList.Any())
                {
                    SendGridAPI = SendGridAPIList.First();
                }
                else
                {
                    return false;
                }


                System.Net.NetworkCredential credentials =
                        new System.Net.NetworkCredential(SendGridAPI.UserName,
                            SendGridAPI.PassWord);
                smtpClient.Credentials = credentials;

                smtpClient.Send(mailMsg);

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

    public class ReCaptcha
    {

        public bool Success { get; set; }
        public List<string> ErrorCodes { get; set; }
        
        public static bool Validate(string encodedResponse)
        {
            if (string.IsNullOrEmpty(encodedResponse)) return false;
            ApplicationDbContext _context = new ApplicationDbContext();
            var client = new System.Net.WebClient();
            var GoogleCaptchaList = _context.GoogleCaptchaAPI.ToList();
            var GoogleCaptcha = new DbTables.GoogleCaptchaAPI();
            if (GoogleCaptchaList.Any())
            {
                GoogleCaptcha = GoogleCaptchaList.First();
            }
            else
            {
                return false;
            }
            var secret = GoogleCaptcha.Secret;

            if (string.IsNullOrEmpty(secret)) return false;

            var googleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, encodedResponse));

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            var reCaptcha = serializer.Deserialize<ReCaptcha>(googleReply);

            return reCaptcha.Success;
        }
    }
}