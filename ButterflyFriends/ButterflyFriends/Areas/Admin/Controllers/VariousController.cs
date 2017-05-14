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
        public ActionResult Index()
        {
            var terms = (from s in _context.Files
                where
                s.FileType == DbTables.FileType.PDF
                select s);
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
            var pdf = new DbTables.File();
            if (terms.Any())
            {
                pdf = terms.First();
            }
            var model = new VariousModel
            {
                GoogleCaptchaAPI = GoogleCap,
                SendGridAPI = SendG,
                File = pdf,
                Carousel = carouselObj,
                About = About,
                StripeAPI = Stripe,
                Twitter = Twitter,
                Facebook = Facebook
            };
            return View(model);
        }

        public ActionResult EnableCarousel()
        {
            var enable = Request.Form["enable"];
            try
            {
                var carousel = _context.Carousel.ToList().First();

                carousel.Enabeled = !carousel.Enabeled;
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

        public ActionResult EditFacebook(DbTables.Facebook model)
        {
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
        public ActionResult EditStripe(DbTables.StripeAPI model)
        {
            try
            {
                var Stripe = new DbTables.StripeAPI();
                var StripeList = _context.StripeAPI.ToList();
                if (StripeList.Any())
                {
                    Stripe = StripeList.First();
                    Stripe.Key = model.Key;
                    Stripe.Secret = model.Secret;
                }
                else
                {
                    Stripe.Key = model.Key;
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
        public ActionResult EditTwitter(DbTables.Twitter model)
        {
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

        public ActionResult EditAbout(DbTables.Info model)
        {
            if (ModelState.IsValid) { 
            try
            {
                var About = new DbTables.Info();
                var AboutList = _context.About.ToList();
                //var adress = new DbTables.Adresses();
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

            string messages = string.Join(" ", ModelState.Values
                                       .SelectMany(x => x.Errors)
                                       .Select(x => x.ErrorMessage));

            ViewBag.Error = "Ugyldige verdier: " + messages;

            return PartialView("_AboutPartial", ErrorAbout);
        }

        public ActionResult TermsUpload()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    var pdffiles = (from s in _context.Files
                        where
                        s.FileType == DbTables.FileType.PDF
                        select s);

                    var pdf = new DbTables.File();
                    if (pdffiles.Any())
                    {
                        pdf = pdffiles.First();
                    }
                    else
                    {
                        pdf = null;
                    }
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];
                        if (pdf == null)
                        {
                            pdf = new DbTables.File
                            {
                                FileName = Path.GetFileName(file.FileName),
                                FileType = DbTables.FileType.PDF,
                                ContentType = file.ContentType,
                                Temporary = false
                            };

                            using (var reader = new BinaryReader(file.InputStream))
                            {

                                pdf.Content = reader.ReadBytes(file.ContentLength);
                            }

                            _context.Files.Add(pdf);
                            _context.SaveChanges();

                        }
                        else
                        {
                            pdf.FileName = Path.GetFileName(file.FileName);
                            pdf.FileType = DbTables.FileType.PDF;
                            pdf.ContentType = file.ContentType;

                            using (var reader = new BinaryReader(file.InputStream))
                            {

                                pdf.Content = reader.ReadBytes(file.ContentLength);
                            }
                            _context.SaveChanges();
                        }

                    }
                    ViewBag.Success = "Filen ble sukksessfult lastet opp";
                    return PartialView("_TermsOfUserPartial",pdf);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error: "+ex.Message;
                    var pdf = (from s in _context.Files
                                    where
                                    s.FileType == DbTables.FileType.PDF
                                    select s);
                    var file = new DbTables.File();
                    if (pdf.Any())
                    {
                        file = pdf.First();
                    }
                    return PartialView("_TermsOfUserPartial", file);
                }
            }
            ViewBag.Error = "Ingen fil valgt";
            var pdfFile = (from s in _context.Files
                       where
                       s.FileType == DbTables.FileType.PDF
                       select s);
            var File = new DbTables.File();
            if (pdfFile.Any())
            {
                File = pdfFile.First();
            }
            return PartialView("_TermsOfUserPartial", File);
        }

        public ActionResult CarouselUpload()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    var carouselObj = new DbTables.Carousel();
                    var carousel = _context.Carousel.ToList();
                    if (carousel.Any())
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
                            _context.Files.RemoveRange(deleteCarousel);
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

                        if (file.ContentType.Contains("video"))
                        {
                            fileUpload.FileType = DbTables.FileType.CarouselVideo;
                        }
                        else
                        {
                            fileUpload.FileType = DbTables.FileType.CarouselImage;

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