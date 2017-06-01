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
using System.Web.Script.Serialization;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PagedList;

namespace ButterflyFriends.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        public int imageNum = 3;    //Images to get each time we get new ones
        public int articleNum = 3;  //articles to get each time we get new ones

        /// <summary>
        /// Gets the front page view with images and information
        /// </summary>
        /// <returns>Returns the view to the front page of the website</returns>
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
            var About = new DbTables.Info();    //get about model
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
                if (background.Enabeled)        //if background is in database and available, return 
                {
                    ViewBag.Style = "background:url('/File/Background?id=" + @background.Image.FileId +                                                                                                 //adds background to viewbag
                                   "') no-repeat center center fixed;-webkit-background-size: cover;-moz-background-size: cover;-o-background-size: cover;background-size: cove;overflow-x: hidden;";
                    ViewBag.BackGround = "background-color:transparent;";
                }
            }
            var stripeList = _context.StripeAPI.ToList();
            if (stripeList.Any())
            {
                if (stripeList.First().Enabeled) { 
                ViewBag.StripePublic = stripeList.First().Public;
                }
            }

            var subscriptions = _context.Subscriptions.Where(s => s.Enabeled).ToList();
            var donations = new Donations
            {
                Subscriptions = subscriptions,
            };
            if (!string.IsNullOrEmpty(About.DonateText))
            {
                donations.DonationText = About.DonateText;
            }
            var model = new FrontPageModel
            {
                Articles = articles,
                Carousel = carouselList,
                About = About,
                Donations = donations
            };
            return View(model);

        }

        /// <summary>
        /// Gets an article
        /// </summary>
        /// <param name="id">Return article</param>
        /// <returns>The article view</returns>
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
            var disqus = new DbTables.Disqus();
            var disqusList = _context.Disqus.ToList();
            if (disqusList.Any())
            {
                disqus = disqusList.First();
                if (disqus.Enabeled && !string.IsNullOrEmpty(disqus.DisqusUrl)) //check if we should load in disqus
                {
                    ViewBag.Disqus = disqus.DisqusUrl;
                }
            }
            return View(article);
        }
        /// <summary>
        /// Get about page with different variables
        /// </summary>
        /// <returns>Returns about page</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Returns the request membership view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RequestMembership()
        {
            ViewBag.Message = "Forespør Medlemskap.";

            var GoogleCaptcha = new DbTables.GoogleCaptchaAPI();
            var GoogleCaptchaList = _context.GoogleCaptchaAPI.ToList();
          

            var model = new RequestModel
            {
                MembershipRequest = new DbTables.MembershipRequest(),
                SiteKey = GoogleCaptcha.SiteKey
            };
            if (GoogleCaptchaList.Any())        //check if there are google recaptcha element in the database, add to model if true
            {
                if (GoogleCaptchaList.First().Enabeled) {   //check if enabeled, if not no recaptcha API needed
                model.SiteKey = GoogleCaptchaList.First().SiteKey;
                }
            }
            var terms = _context.TermsOfUse.ToList();
            var Terms = new DbTables.TermsOfUse();
            if (terms.Any())                //check if there are any terms of use in the database
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

        /// <summary>
        /// Post method for membershiprequest
        /// </summary>
        /// <param name="model">Request model with request info</param>
        /// <returns>Success or failure view</returns>
        [HttpPost]
        public ActionResult RequestMembership(RequestModel model)
        {
            
            if (ModelState.IsValid) {   //check if modelstate is valid
                var encodedResponse = Request.Form["g-Recaptcha-Response"]; //get the recaptcha token from the form
                var isCaptchaValid = ReCaptcha.Validate(encodedResponse);   //check if recaptcha token corresponds to a valid user or not

                if (!isCaptchaValid)    //recapthca failed. return view with errormessage
                {
                    
                    ViewBag.Error = "Recaptcha feilet";
                    ViewBag.Reset = "false";
                    return PartialView("_statusPartial");
                }
                try
            {
                _context.MembershipRequests.Add(model.MembershipRequest);   //add request to database, return succsess message
                _context.SaveChanges();
                ViewBag.Success = "Din forespørsel ble suksessfult motatt, vi kontakter deg så snart vi kan";
                ViewBag.Reset = "true";
                return PartialView("_statusPartial");
            }
            catch (EntityException ex)
            {

                ViewBag.Error = "Noe gikk galt" + ex.Message;   //something went wrong, display error message
                    ViewBag.Reset = "false";
                    return PartialView("_statusPartial");
            }
            }
            string messages = string.Join("\n", ModelState.Values   //return model error upon modelstate failours
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));

            ViewBag.Error = "Ugyldige verdier: " + messages;
            ViewBag.Reset = "false";
            return PartialView("_statusPartial");   //return error view

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

        /// <summary>
        /// Filter images based on the user and the startid of the images
        /// </summary>
        /// <param name="User">Current user</param>
        /// <param name="startId">Start id</param>
        /// <param name="imageNum">number of images to return</param>
        /// <returns>return list of images to display in view</returns>
        public IList<DbTables.File> filterImages(ApplicationUser User,int startId, int imageNum)
        {
            List<DbTables.File> images = new List<DbTables.File>();
            IList<int> ids = new List<int>();
            if (User.Pictures.Any())    //get all pictures of user
            {
                foreach (var picture in User.Pictures)
                {
                    if (picture.Published) {    //if picture is published
                    images.Add(picture);        //add picture
                    ids.Add(picture.FileId);    
                    }
                }
            }

            foreach (var child in User.Children)    //get pictures of user's children
            {
                foreach (var picture in child.Pictures)
                {
                    if (!ids.Contains(picture.FileId) && picture.Published)  //if is not already added
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

        /// <summary>
        /// Filter articles
        /// </summary>
        /// <param name="startId">Id of first article to get</param>
        /// <param name="articleNum">how many articles to return</param>
        /// <returns>List with articles to show in view</returns>
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
        /// <summary>
        /// Gets images based on how many images that are actually loaded
        /// </summary>
        /// <returns>Returns a partial view with the images</returns>
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

        /// <summary>
        /// We get articles based on how many articles are already loaded
        /// </summary>
        /// <returns>Returns partial view with the articles</returns>
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

        /// <summary>
        /// Handels the payments based on variables and the tokens sent in with the post request
        /// </summary>
        /// <returns>Returns a partial view with the reciept and information</returns>
        [HttpPost]
        public ActionResult HandlePayment()
        {
            
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

            if (anon == "true") //payment is anonymous
            {
                donation = new DbTables.Donations
                {
                    Amount = amount,
                    Description = description,
                    anonymous = true
                };
            }
            else if (user == "true" && User.Identity.GetUserId() != null)   //connet donation to user as long as there is actually a logged in user
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
                donation = new DbTables.Donations   //information has been given
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
            data["source"] = token;     //the payment token with user's credidentials
            data["description"] = "Donasjon "+donation.Id +": "+description;    //description of donation.

            if (!string.IsNullOrEmpty(email)) {     //reciept email is not null, add to request
                data["receipt_email"] = email;
            }else if (user == "true")
            {
                data["receipt_email"]=donation.User.Email;
            }
            client.UseDefaultCredentials = true;    //use default credidentials for API request

            var stripeList = _context.StripeAPI.ToList();   //check if stripe actually exists in the database
            byte[] response;
            if (!stripeList.Any())
            {
                return Json(new { Error = "Stripe er ikke konfigurert for applikasjonen.",Succsess="false", striperesponse ="false"});  //no stripe in database, return error
            }
            if(!stripeList.First().Enabeled)
            {
                return Json(new { Error = "Stripe er avslått for applikasjonen.", Succsess = "false", striperesponse = "false" });  //stripe disabeled

            }

            client.Credentials = new NetworkCredential(_context.StripeAPI.ToList().First().Secret, "");

            try
            {
               response = client.UploadValues("https://api.stripe.com/v1/charges", "POST", data);   // upload values and get response
            }
            catch (WebException exception)  //exepction happen when poisting to API
            {
                string responseString;
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))   //read the errorstring
                {
                    responseString = reader.ReadToEnd();
                }

                return Json(new { Error = responseString ,Success="false",striperesponse="true"});  // return responsestring as error message
            }
            /*var json_serializer = new JavaScriptSerializer(); 
            var JsonDict = (IDictionary<string, object>)json_serializer.DeserializeObject(client.Encoding.GetString(response));*/ //These lines parses the respone, which is at the moment not used for anything

            // If we got this far, there were no errors, and we set the order to paid, and save.
            Response.StatusCode = 200;
            donation.isPaid = true; //change element to paid
            _context.SaveChanges(); //save db
            if (!string.IsNullOrEmpty(recieptemail))   //send reciept email if reciept email is given
            {
                var subject = "Kvitering på donasjon";
                var message = "Takk for din støtte! \n Du har donert "+amount+" kroner til Butterfly Friends. \n"+"Ditt referansenummer er "+donation.Id+". \n\n"+"Vennlig hilsen,\nButterfly Friends.";
                var messageHTML = "<p>Takk for din støtte! <br> Du har donert "+amount+" kroner til Butterfly Friends. <br>"+"Ditt referansenummer er "+donation.Id+". <br><br>"+"Vennlig hilsen,<br>Butterfly Friends.</p>";
                if(!SendEmail(message, messageHTML, subject, recieptemail, recieptname))    //returns true if sending of email was succsessful
                {
                    ViewBag.Error = "Emailkviteringen kunne ikke sendes, Sendgrid er ikke konfigurert.";
                }
            }

            ViewBag.Share = "https://www." + Request.Url.Host + "/Home/Index";  //share link for twitter
            ViewBag.ShareText = "Jeg har donert "+amount+" kr. til Butterfly Friends!"; //share message

            var TwitterList = _context.Twitter.ToList();    //check if facebook and twitter exist
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
            return PartialView("_RecieptPartial", model);    //return reciept view
        }

        /// <summary>
        /// Handles subscription payments. Creates user and subscribes to plan
        /// </summary>
        /// <returns>reciept</returns>
        public ActionResult HandleSubPayment()
        {
            var subId =Request.Form["subId"];
            var anon = Request.Form["anon"];
            var user = Request.Form["user"];
            var token = Request.Form["token"];
            var email = Request.Form["email"];
            var phone = Request.Form["phone"];
            var birthnumber = Request.Form["birthnumber"];
            var name = Request.Form["name"];
            var description = Request.Form["description"];
            var recieptemail = "";  //email to send reciept
            var recieptname = "";   //name on reciept

            var client = new WebClient();

            var data = new NameValueCollection();
            data["source"] = token;     //the payment token with user's credidentials
            if (user == "true")
            {
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
                var currentUser = manager.FindById(User.Identity.GetUserId());
                recieptemail = currentUser.Email;
                recieptname = currentUser.Fname + " " + currentUser.Lname;
                data["email"] = currentUser.Email;
                data["description"] = "User "+email+" in the database: "+description;   //set description of donation
            }
            else if (anon == "true" && !string.IsNullOrEmpty(description))
            {
                data["description"] = description;
            }
            else
            {
                if (!string.IsNullOrEmpty(email))
                {
                    data["email"] = email;
                    recieptemail = email;
                }
                recieptname = name;
                data["description"] = "Telefon: " + phone + " - Navn: " + name + " - Fødselsnummer: "+birthnumber+" - Beskrivelse: " + description;
            }
            client.UseDefaultCredentials = true;    //use default credidentials for API request

            var stripeList = _context.StripeAPI.ToList();   //check if stripe actually exists in the database
            byte[] response;
            if (!stripeList.Any())
            {
                return Json(new { Error = "Stripe er ikke konfigurert for applikasjonen.", Succsess = "false", striperesponse = "false" });  //no stripe in database, return error
            }
            if (!stripeList.First().Enabeled)
            {
                return Json(new { Error = "Stripe er avslått for applikasjonen.", Succsess = "false", striperesponse = "false" });  //stripe disabeled

            }
            client.Credentials = new NetworkCredential(_context.StripeAPI.ToList().First().Secret, "");
                
            try
            {
                response = client.UploadValues("https://api.stripe.com/v1/customers", "POST", data);   // upload values and get response
            }
            catch (WebException exception)  //exepction happen when poisting to API
            {
                string responseString;
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))   //read the errorstring
                {
                    responseString = reader.ReadToEnd();
                }

                return Json(new { Error = responseString, Success = "false", striperesponse = "true" });  // return responsestring as error message
            }
            //if we get here customer was succsessfully created
            var json_serializer = new JavaScriptSerializer();
            var JsonDict = (IDictionary<string, object>)json_serializer.DeserializeObject(client.Encoding.GetString(response)); //deseroalize the response
            var customer = JsonDict["id"].ToString(); //get id of customer returned by the API

            data = new NameValueCollection();   //now create a collection for the plan and subscribe
            data["plan"] = subId;
            data["customer"] = customer;
            try
            {
                response = client.UploadValues("https://api.stripe.com/v1/subscriptions", "POST", data);   // upload values and get response
            }
            catch (WebException exception)  //exepction happen when poisting to API
            {
                string responseString;
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))   //read the errorstring
                {
                    responseString = reader.ReadToEnd();
                }

                return Json(new { Error = responseString, Success = "false", striperesponse = "true" });  // return responsestring as error message
            }
            //customer successfully subscribed to plan
            JsonDict = (IDictionary<string, object>)json_serializer.DeserializeObject(client.Encoding.GetString(response)); //deseroalize the response
            var subscriptionId = JsonDict["id"].ToString();
            Response.StatusCode = 200;

            var sub = _context.Subscriptions.Find(int.Parse(subId));
            if (!string.IsNullOrEmpty(recieptemail))   //send reciept email if reciept email is given
            {
                var subject = "Kvitering på donasjon";
                var message = "Takk for din støtte! \n Du har started et abonement med id " + subId + " for "+sub.Amount+" kroner i måneden til Butterfly Friends. \n" + "Ditt referansenummer er " + subscriptionId + ". \n\n" + "Vennlig hilsen,\nButterfly Friends.";
                var messageHTML = "<p>Takk for din støtte! <br> Du har startet et abonement med id " + subId + " for "+sub.Amount+" kroner i måneden til Butterfly Friends. <br>" + "Ditt referansenummer er: " + subscriptionId + ". <br><br>" + "Vennlig hilsen,<br>Butterfly Friends.</p>";
                if (!SendEmail(message, messageHTML, subject, recieptemail, recieptname))    //returns true if sending of email was succsessful
                {
                    ViewBag.Error = "Emailkviteringen kunne ikke sendes, Sendgrid er ikke konfigurert.";
                }
            }

            ViewBag.Share = "https://www." + Request.Url.Host + "/Home/Index";  //share link for twitter
            ViewBag.ShareText = "Jeg donerer " + sub.Amount + " kr i måneden til Butterfly Friends!"; //share message

            var TwitterList = _context.Twitter.ToList();    //check if facebook and twitter exist
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
            var subReciept = new SubReciept
            {
                Amount = sub.Amount,
                Id = subId,
                referenceId = subscriptionId
            };
            var model = new RecieptModel
            {
                Facebook = Facebook,
                Twitter = Twitter,
                SubReciept = subReciept
            };
            
            return PartialView("_RecieptPartial", model);    //return reciept view

        }
        /// <summary>
        /// Sends an email with reciept information
        /// </summary>
        /// <param name="message">Message in reciept</param>
        /// <param name="messageHTML">Message without html</param>
        /// <param name="subject">Subject of email</param>
        /// <param name="recieverEmail">Recipient email</param>
        /// <param name="recieverName">Name of recipient</param>
        /// <returns>Returns true or false based on wether email was sent or not</returns>
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
                if (!SendGridAPI.Enabeled)
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
        
        /// <summary>
        /// Validates whether the recapthca failed or not
        /// </summary>
        /// <param name="encodedResponse">Response token from the Recapthca API</param>
        /// <returns>Returns true or false based on the validation of user</returns>
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

            var googleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, encodedResponse)); // upload values and get response

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            var reCaptcha = serializer.Deserialize<ReCaptcha>(googleReply); //serialize the answer

            return reCaptcha.Success;
        }
    }
}