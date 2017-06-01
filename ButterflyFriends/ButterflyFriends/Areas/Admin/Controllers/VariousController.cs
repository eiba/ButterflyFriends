using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Models;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    [Authorize(Roles = "Eier, Admin")]
    public class VariousController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/Various
        /// <summary>
        /// Index view of various, check if various database elements are actually added and then add these to the model if they exist
        /// </summary>
        /// <returns>Various index view</returns>
        public ActionResult Index()
        {

            var carouselObj = new DbTables.Carousel();
            var GoogleCap = new DbTables.GoogleCaptchaAPI();
            var GoogleCapList = _context.GoogleCaptchaAPI.ToList();
            if (GoogleCapList.Any())
            {
                GoogleCap = GoogleCapList.First();
            }
            var SendG = new DbTables.SendGridAPI();
            var SendgridList = _context.SendGridAPI.ToList();
            if (SendgridList.Any())
            {
                SendG = SendgridList.First();
            }
            var Stripe = new DbTables.StripeAPI();
            var StripeList = _context.StripeAPI.ToList();
            if (StripeList.Any())
            {
                Stripe = StripeList.First();
            }
            var Facebook = new DbTables.Facebook();
            var FacebookList = _context.Facebook.ToList();
            if (FacebookList.Any())
            {
                Facebook = FacebookList.First();
            }
            var Twitter = new DbTables.Twitter();
            var TwitterList = _context.Twitter.ToList();
            if (TwitterList.Any())
            {
                Twitter = TwitterList.First();
            }
            var Disqus = new DbTables.Disqus();
            var DisqusList = _context.Disqus.ToList();
            if (DisqusList.Any())
            {
                Disqus = DisqusList.First();
            }
            var About = new DbTables.Info();
            var AboutList = _context.About.ToList();
            if (AboutList.Any())
            {
                About = AboutList.First();
            }
            var carousel = _context.Carousel.ToList();
            if (carousel.Any())
            {
                carouselObj = carousel.First();
            }
            var Terms = new DbTables.TermsOfUse();
            var TermsList = _context.TermsOfUse.ToList();
            if (TermsList.Any())
            {
                Terms = TermsList.First();
            }
            var Background = new DbTables.BackgroundImage();
            var BackgroundList = _context.BackgroundImage.ToList();
            if (BackgroundList.Any())
            {
                Background = BackgroundList.First();
            }

            var model = new VariousModel
            {
                GoogleCaptchaAPI = GoogleCap,
                SendGridAPI = SendG,
                Terms = Terms,
                Carousel = carouselObj,
                About = About,
                StripeAPI = Stripe,
                Twitter = Twitter,
                Facebook = Facebook,
                Background = Background,
                Disqus = Disqus
            };
            return View(model);
        }

        /// <summary>
        /// Enable the image carousel
        /// </summary>
        /// <returns>partial view for suksess or not</returns>
        public ActionResult EnableCarousel()
        {
            var enable = Request.Form["enable"];
            try
            {
                var carousel = _context.Carousel.ToList().First();

                carousel.Enabeled = !carousel.Enabeled; //change the enabeled value to the opposite
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_ImageCarouselPartial", _context.Carousel.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_ImageCarouselPartial", _context.Carousel.ToList().First());
            }

        }
        /// <summary>
        /// Enable background
        /// </summary>
        /// <returns>Succsess or failure view</returns>
        public ActionResult EnableBackground()
        {
            var enable = Request.Form["enable"];
            try
            {
                var background = _context.BackgroundImage.ToList().First();

                background.Enabeled = !background.Enabeled;
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_BackgroundImagePartial", _context.BackgroundImage.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_BackgroundImagePartial", _context.BackgroundImage.ToList().First());
            }

        }

        /// <summary>
        /// Enable terms of user function
        /// </summary>
        /// <returns>Succsess or failure view</returns>
        public ActionResult EnableTerms()
        {
            var enable = Request.Form["enable"];
            try
            {
                var Terms = _context.TermsOfUse.ToList().First();

                Terms.Enabeled = !Terms.Enabeled;
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_TermsOfUserPartial", _context.TermsOfUse.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_TermsOfUserPartial", _context.TermsOfUse.ToList().First());
            }

        }
        /// <summary>
        /// Enable Twitter
        /// </summary>
        /// <returns>Succsess or failure view</returns>
        public ActionResult EnableTwitter()
        {
            var enable = Request.Form["enable"];
            try
            {
                var twitter = _context.Twitter.ToList().First();

                twitter.Enabeled = !twitter.Enabeled;
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_TwitterPartial", _context.Twitter.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_TwitterPartial", _context.Twitter.ToList().First());
            }

        }
        /// <summary>
        /// Enable Facebook
        /// </summary>
        /// <returns>Succsess or failure view</returns>
        public ActionResult EnableFacebook()
        {
            var enable = Request.Form["enable"];
            try
            {
                var facebook = _context.Facebook.ToList().First();

                facebook.Enabeled = !facebook.Enabeled;
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_FacebookPartial", _context.Facebook.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_FacebookPartial", _context.Facebook.ToList().First());
            }

        }
        /// <summary>
        /// Enable stripe
        /// </summary>
        /// <returns></returns>
        public ActionResult EnableStripe()
        {
            var enable = Request.Form["enable"];
            try
            {
                var stripe = _context.StripeAPI.ToList().First();

                stripe.Enabeled = !stripe.Enabeled;
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_StripePartial", _context.StripeAPI.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_StripePartial", _context.StripeAPI.ToList().First());
            }

        }
        /// <summary>
        /// Enable sendgrid
        /// </summary>
        /// <returns></returns>
        public ActionResult EnableSendgrid()
        {
            var enable = Request.Form["enable"];
            try
            {
                var sendgrid = _context.SendGridAPI.ToList().First();

                sendgrid.Enabeled = !sendgrid.Enabeled;
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_SendGridPartial", _context.SendGridAPI.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_SendGridPartial", _context.SendGridAPI.ToList().First());
            }

        }
        /// <summary>
        /// Enable recaptcha
        /// </summary>
        /// <returns></returns>
        public ActionResult EnableRecaptcha()
        {
            var enable = Request.Form["enable"];
            try
            {
                var recaptcha = _context.GoogleCaptchaAPI.ToList().First();

                recaptcha.Enabeled = !recaptcha.Enabeled;
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_RecaptchaPartial", _context.GoogleCaptchaAPI.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_RecaptchaPartial", _context.GoogleCaptchaAPI.ToList().First());
            }

        }
        /// <summary>
        /// Enables or disables disqus
        /// </summary>
        /// <returns></returns>
        public ActionResult EnableDisqus()
        {
            var enable = Request.Form["enable"];
            try
            {
                var disqus = _context.Disqus.ToList().First();

                disqus.Enabeled = !disqus.Enabeled;
                _context.SaveChanges();
                if (enable == "true")
                {
                    ViewBag.Success = "Funksjonen ble slått på";
                }
                else
                {
                    ViewBag.Success = "Funksjonen ble slått av";

                }
                return PartialView("_DisqusPartial", _context.Disqus.ToList().First());
            }
            catch (Exception ex)
            {

                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_DisqusPartial", _context.Disqus.ToList().First());
            }

        }
        /// <summary>
        /// Edit sendgrid values
        /// </summary>
        /// <param name="model">Model values to change</param>
        /// <returns>view with updated info and success/failure message</returns>
        [HttpPost]
        public ActionResult EditSendGrid(DbTables.SendGridAPI model)
        {
            try
            {
                var SendGrid = new DbTables.SendGridAPI();
                var SendGridAPI = _context.SendGridAPI.ToList();
                if (SendGridAPI.Any())
                {
                    SendGrid = SendGridAPI.First();
                    SendGrid.PassWord = model.PassWord;
                    SendGrid.UserName = model.UserName;
                }
                else
                {
                    SendGrid.PassWord = model.PassWord;
                    SendGrid.UserName = model.UserName;
                    _context.SendGridAPI.Add(SendGrid);
                }
                
                _context.SaveChanges();
                ViewBag.Success = "Sendgrid API variabler ble sukessfult oppdatert";
                return PartialView("_SendGridPartial",_context.SendGridAPI.First());
            }
            catch (EntityException ex)
            {
                ViewBag.Error ="Error:"+ ex.Message;
                return PartialView("_SendGridPartial", _context.SendGridAPI.First());
            }
        }

        /// <summary>
        /// Edit values
        /// </summary>
        /// <param name="model">Model values to change</param>
        /// <returns>view with updated info and success/failure message</returns>
        public ActionResult EditFacebook(DbTables.Facebook model)
        {
            if (ModelState.IsValid) { 
            try
            {
                var Facebook = new DbTables.Facebook();
                var FacebookList = _context.Facebook.ToList();
                if (FacebookList.Any())
                {
                    Facebook = FacebookList.First();
                    Facebook.Url = model.Url;
                }
                else
                {
                    Facebook.Url = model.Url;
                    Facebook.Enabeled = true;
                    _context.Facebook.Add(Facebook);
                }

                _context.SaveChanges();
                ViewBag.Success = "Facebook variabler ble sukessfult oppdatert";
                return PartialView("_FacebookPartial", _context.Facebook.First());
            }
            catch (EntityException ex)
            {
                ViewBag.Error = "Error:" + ex.Message;
                return PartialView("_FacebookPartial", _context.Facebook.First());
            }
            }
            var FacebookError = new DbTables.Facebook();
            var FacebookListError = _context.Facebook.ToList();
            if (FacebookListError.Any())
            {
                FacebookError = FacebookListError.First();
            }
            string messages = string.Join("\r\n\r\n", ModelState.Values //validation failed, return errors
                                      .SelectMany(x => x.Errors)
                                      .Select(x => x.ErrorMessage));

            ViewBag.Error = "Ugyldige verdier: " + messages;

            return PartialView("_FacebookPartial", FacebookError);

        }

        /// <summary>
        /// Edit values
        /// </summary>
        /// <param name="model">Model values to change</param>
        /// <returns>view with updated info and success/failure message</returns>
        public ActionResult EditDisqus(DbTables.Disqus model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var Disqus = new DbTables.Disqus();
                    var DisqusList = _context.Disqus.ToList();
                    if (DisqusList.Any())
                    {
                        Disqus = DisqusList.First();
                        Disqus.DisqusUrl = model.DisqusUrl;
                    }
                    else
                    {
                        Disqus.DisqusUrl = model.DisqusUrl;
                        Disqus.Enabeled = true;
                        _context.Disqus.Add(Disqus);
                    }

                    _context.SaveChanges();
                    ViewBag.Success = "Disqus variabler ble sukessfult oppdatert";
                    return PartialView("_DisqusPartial", _context.Disqus.First());
                }
                catch (EntityException ex)
                {
                    ViewBag.Error = "Error:" + ex.Message;
                    return PartialView("_DisqusPartial", _context.Disqus.First());
                }
            }
            var DisqusError = new DbTables.Disqus();
            var DisqusListError = _context.Disqus.ToList();
            if (DisqusListError.Any())
            {
                DisqusError = DisqusListError.First();
            }
            string messages = string.Join("\r\n\r\n", ModelState.Values //validation failed
                                      .SelectMany(x => x.Errors)
                                      .Select(x => x.ErrorMessage));

            ViewBag.Error = "Ugyldige verdier: " + messages;

            return PartialView("_DisqusPartial", DisqusError);

        }
        /// <summary>
        /// Edit values
        /// </summary>
        /// <param name="model">Model values to change</param>
        /// <returns>view with updated info and success/failure message</returns>
        public ActionResult EditStripe(DbTables.StripeAPI model)
        {
            try
            {
                var Stripe = new DbTables.StripeAPI();
                var StripeList = _context.StripeAPI.ToList();
                if (StripeList.Any())
                {
                    Stripe = StripeList.First();
                    Stripe.Public = model.Public;
                    Stripe.Secret = model.Secret;
                }
                else
                {
                    Stripe.Public = model.Public;
                    Stripe.Secret = model.Secret;
                    _context.StripeAPI.Add(Stripe);
                }

                _context.SaveChanges();
                ViewBag.Success = "Stripe API variabler ble sukessfult oppdatert";
                return PartialView("_StripePartial", _context.StripeAPI.First());
            }
            catch (EntityException ex)
            {
                ViewBag.Error = "Error:" + ex.Message;
                return PartialView("_StripePartial", _context.StripeAPI.First());
            }
        }

        /// <summary>
        /// Edit values
        /// </summary>
        /// <param name="model">Model values to change</param>
        /// <returns>view with updated info and success/failure message</returns>
        public ActionResult EditTwitter(DbTables.Twitter model)
        {
            if (ModelState.IsValid) { 
            try
            {
                var Twitter = new DbTables.Twitter();
                var TwitterList = _context.Twitter.ToList();
                if (TwitterList.Any())
                {
                    Twitter = TwitterList.First();
                    Twitter.Url = model.Url;
                    Twitter.UserName = model.UserName;
                }
                else
                {
                    Twitter.Url = model.Url;
                    Twitter.UserName = model.UserName;
                    Twitter.Enabeled = true;
                    _context.Twitter.Add(Twitter);
                }

                _context.SaveChanges();
                ViewBag.Success = "Twitter variabler ble sukessfult oppdatert";
                return PartialView("_TwitterPartial", _context.Twitter.First());
            }
            catch (EntityException ex)
            {
                ViewBag.Error = "Error:" + ex.Message;
                return PartialView("_TwitterPartial", _context.Twitter.First());
            }
            }
            var TwitterError = new DbTables.Twitter();
            var TwitterListError = _context.Twitter.ToList();
            if (TwitterListError.Any())
            {
                TwitterError = TwitterListError.First();
            }
            string messages = string.Join("r\n\r\n", ModelState.Values
                                      .SelectMany(x => x.Errors)
                                      .Select(x => x.ErrorMessage));

            ViewBag.Error = "Ugyldige verdier: " + messages;

            return PartialView("_TwitterPartial", TwitterError);
        }
        /// <summary>
        /// Edit values
        /// </summary>
        /// <param name="model">Model values to change</param>
        /// <returns>view with updated info and success/failure message</returns>
        public ActionResult EditRecaptcha(DbTables.GoogleCaptchaAPI model)
        {
            try
            {
                var GoogleRe = new DbTables.GoogleCaptchaAPI();
                var GoogleRecaptcha = _context.GoogleCaptchaAPI.ToList();
                if (GoogleRecaptcha.Any())
                {
                    GoogleRe = GoogleRecaptcha.First();
                    GoogleRe.SiteKey = model.SiteKey;
                    GoogleRe.Secret = model.Secret;
                }
                else
                {
                    GoogleRe.SiteKey = model.SiteKey;
                    GoogleRe.Secret = model.Secret;
                    _context.GoogleCaptchaAPI.Add(GoogleRe);
                }
                _context.SaveChanges();
                ViewBag.Success = "Google ReCaptcha variabler ble sukessfult oppdatert";
                return PartialView("_RecaptchaPartial", _context.GoogleCaptchaAPI.First());
            }
            catch (EntityException ex)
            {
                ViewBag.Error ="Error: "+ ex.Message;
                return PartialView("_RecaptchaPartial", _context.GoogleCaptchaAPI.First());

            }
        }
        /// <summary>
        /// Edit values
        /// </summary>
        /// <param name="model">Model values to change</param>
        /// <returns>view with updated info and success/failure message</returns>
        public ActionResult EditAbout(DbTables.Info model)
        {
            if (ModelState.IsValid) { 
            try
            {
                var About = new DbTables.Info();
                var AboutList = _context.About.ToList();
                if (AboutList.Any())
                {
                    About = AboutList.First();
                    About.Phone = model.Phone;
                    About.About = model.About;
                    About.Email = model.Email;
                        About.DonateText = model.DonateText;
                        About.MembershipText = model.MembershipText;
                        var adress = new AboutAdress
                        {
                            StreetAdress = model.Adress.StreetAdress,
                            City = model.Adress.City,
                            PostCode = model.Adress.PostCode,
                            County = model.Adress.County
                        };

                    About.Adress = adress;
                }
                else
                {
                    About.Phone = model.Phone;  
                    About.About = model.About;
                    About.Email = model.Email;
                        About.DonateText = model.DonateText;
                        About.MembershipText = model.MembershipText;

                        var adress = new AboutAdress
                        {
                            StreetAdress = model.Adress.StreetAdress,
                            City = model.Adress.City,
                            PostCode = model.Adress.PostCode,
                            County = model.Adress.County
                        };
                    About.Adress = adress;
                    
                        _context.About.Add(About);
                }
                _context.SaveChanges();
                ViewBag.Success = "Sideinformasjonen ble oppdatert";
                return PartialView("_AboutPartial", _context.About.First());
            }
            catch (EntityException ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_AboutPartial", _context.About.First());

            }
            }
            var ErrorAbout = new DbTables.Info();
            var ErrorAboutList = _context.About.ToList();
            if (ErrorAboutList.Any())
            {
                ErrorAbout = ErrorAboutList.First();
            }

            string messages = string.Join("r\n\r\n", ModelState.Values
                                .SelectMany(x => x.Errors)
                                .Select(x => x.ErrorMessage));

            ViewBag.Error = "Ugyldige verdier: " + messages;

            return PartialView("_AboutPartial", ErrorAbout);
        }

        /// <summary>
        /// upload terms of use document
        /// </summary>
        /// <returns>Succsess or failure partial view</returns>
        public ActionResult TermsUpload()
        {
            if (Request.Files.Count > 0)    //check that there are actually files in the request
            {
                try
                {
                    var Terms = new DbTables.TermsOfUse();
                    var TermsList = _context.TermsOfUse.ToList();
                    if (TermsList.Any())    //if it exist already, get it
                    {
                        Terms = TermsList.First();
                    }
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)   //get the files object
                    {

                        HttpPostedFileBase file = files[i];
                        if (Terms.Id == 0)  //add the file, it does not exist
                        {
                            var TermsFile = new DbTables.File
                            {
                                FileName = Path.GetFileName(file.FileName),
                                FileType = DbTables.FileType.PDF,
                                ContentType = file.ContentType,
                                Temporary = false
                            };

                            using (var reader = new BinaryReader(file.InputStream))
                            {

                                TermsFile.Content = reader.ReadBytes(file.ContentLength);
                            }

                            _context.Files.Add(TermsFile);
                            Terms.Terms = TermsFile;
                            Terms.Enabeled = true;
                            _context.TermsOfUse.Add(Terms);
                            _context.SaveChanges();

                        }
                        else
                        {
                            var TermsFile = Terms.Terms;    //it exists, just change values
                            TermsFile.FileName = Path.GetFileName(file.FileName);
                            TermsFile.FileType = DbTables.FileType.PDF;
                            TermsFile.ContentType = file.ContentType;

                            using (var reader = new BinaryReader(file.InputStream))
                            {

                                TermsFile.Content = reader.ReadBytes(file.ContentLength);
                            }
                            _context.SaveChanges();
                        }

                    }
                    ViewBag.Success = "Filen ble sukksessfult lastet opp";
                    return PartialView("_TermsOfUserPartial",Terms);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error: "+ex.Message;
                    var Terms = new DbTables.TermsOfUse();
                    var TermsList = _context.TermsOfUse.ToList();
                    if (TermsList.Any())
                    {
                        Terms = TermsList.First();
                    }
                    return PartialView("_TermsOfUserPartial", Terms);
                }
            }
            ViewBag.Error = "Ingen fil valgt";
            var TermsError = new DbTables.TermsOfUse();
            var TermsListError = _context.TermsOfUse.ToList();
            if (TermsListError.Any())
            {
                TermsError = TermsListError.First();
            }
            return PartialView("_TermsOfUserPartial", TermsError);
        }

        /// <summary>
        /// upload carousel images and videos
        /// </summary>
        /// <returns></returns>
        public ActionResult CarouselUpload()
        {
            if (Request.Files.Count > 0)    //check that there are files to get
            {
                try
                {
                    var carouselObj = new DbTables.Carousel();
                    var carousel = _context.Carousel.ToList();
                    if (carousel.Any()) //if it already exists, clear the current files and delete them
                    {
                        carouselObj = carousel.First();
                        carouselObj.CarouselItems.Clear();
                        var deleteCarousel =
                            _context.Files.Where(
                                s =>
                                    s.FileType == DbTables.FileType.CarouselImage ||
                                    s.FileType == DbTables.FileType.CarouselVideo);
                        if (deleteCarousel.Any())
                        {
                            _context.Files.RemoveRange(deleteCarousel); //remove many at once.
                        }
                    }
                    else
                    {
                        carouselObj = new DbTables.Carousel
                        {
                            Enabeled = true,
                            CarouselItems = new List<DbTables.File>()
                        };
                        _context.Carousel.Add(carouselObj);

                    }


                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;   
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];

                        var fileUpload = new DbTables.File
                        {
                            FileName = Path.GetFileName(file.FileName),
                            ContentType = file.ContentType,
                            Temporary = false
                        };


                        using (var reader = new BinaryReader(file.InputStream)) 
                        {

                            fileUpload.Content = reader.ReadBytes(file.ContentLength);
                        }

                        if (file.ContentType.Contains("video")) //it's a video, add as video type
                        {
                            fileUpload.FileType = DbTables.FileType.CarouselVideo;
                        }
                        else
                        {
                            fileUpload.FileType = DbTables.FileType.CarouselImage;  //it's and image
                                
                        }
                        _context.Files.Add(fileUpload);
                        carouselObj.CarouselItems.Add(fileUpload);



                    }
                    _context.SaveChanges();

                    ViewBag.Success = "Filen(e) ble sukksessfult lastet opp";
                    return PartialView("_ImageCarouselPartial",carouselObj);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error: " + ex.Message;
                    var carouselErr = new DbTables.Carousel();
                    var carousell = _context.Carousel.ToList();
                    if (carousell.Any())
                    {
                        carouselErr = carousell.First();
                    }

                    return PartialView("_ImageCarouselPartial", carouselErr);

                }
            }
            ViewBag.Error = "Ingen fil valgt";
            var carouselError = new DbTables.Carousel();
            var carouselL = _context.Carousel.ToList();
            if (carouselL.Any())
            {
                carouselError = carouselL.First();
            }

            return PartialView("_ImageCarouselPartial", carouselError);

        }

        /// <summary>
        /// Upload background image
        /// </summary>
        /// <returns>Succsess or error view</returns>
        public ActionResult BackgroundUpload()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    var backgroundObj = new DbTables.BackgroundImage();
                    var background = _context.BackgroundImage.ToList();
                    if (background.Any())
                    {
                        backgroundObj = background.First();
                        
                    }
                    else
                    {
                        backgroundObj = new DbTables.BackgroundImage()
                        {
                            Enabeled = true,
                            Image = new DbTables.File()
                        };
                        _context.BackgroundImage.Add(backgroundObj);

                    }


                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)   
                    {

                        HttpPostedFileBase file = files[i];

                        var fileUpload = new DbTables.File
                        {
                            FileName = Path.GetFileName(file.FileName),
                            ContentType = file.ContentType,
                            Temporary = false,
                            FileType = DbTables.FileType.BackgroundImage
                        };

                        
                        using (var reader = new BinaryReader(file.InputStream))
                        {

                            fileUpload.Content = reader.ReadBytes(file.ContentLength);
                        }

                        _context.Files.Add(fileUpload);
                        backgroundObj.Image = fileUpload;



                    }
                    _context.SaveChanges();

                    ViewBag.Success = "Filen(e) ble sukksessfult lastet opp";
                    return PartialView("_BackgroundImagePartial", backgroundObj);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error: " + ex.Message;
                    var backgrounderr = new DbTables.BackgroundImage();
                    var backgroundl = _context.BackgroundImage.ToList();
                    if (backgroundl.Any())
                    {
                        backgrounderr = backgroundl.First();
                    }

                    return PartialView("_BackgroundImagePartial", backgrounderr);

                }
            }
            ViewBag.Error = "Ingen fil valgt";
            var backgroundError = new DbTables.BackgroundImage();
            var backgroundL = _context.BackgroundImage.ToList();
            if (backgroundL.Any())
            {
                backgroundError = backgroundL.First();
            }

            return PartialView("_BackgroundImagePartial", backgroundError);

        }
        
        /// <summary>
        /// Check if adress exist
        /// </summary>
        /// <param name="adress">adress to check</param>
        /// <returns>returns null if no adress found, otherwise returns adress</returns>
        public DbTables.Adresses AdressExist(DbTables.Adresses adress)
        {
                var adresses = _context.Set<DbTables.Adresses>();
            foreach (var Adress in adresses)
            {
                if (adress.StreetAdress == Adress.StreetAdress && adress.PostCode == Adress.PostCode & adress.City == Adress.City && adress.County == Adress.County)
                {
                    return Adress;
                }
            }
            return null;
        }
    }
}