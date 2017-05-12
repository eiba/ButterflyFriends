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
                GoogleCaptchaAPI = _context.GoogleCaptchaAPI.First(),
                SendGridAPI = _context.SendGridAPI.First(),
                File = pdf,
                Carousel = carouselObj
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
        [HttpPost]
        public ActionResult EditSendGrid(DbTables.SendGridAPI model)
        {
            try
            {
                var SendGridAPI = _context.SendGridAPI.First();
                SendGridAPI.PassWord = model.PassWord;
                SendGridAPI.UserName = model.UserName;
                _context.SaveChanges();
                ViewBag.Success = "Sendgrid API variabler ble sukessfult oppdatert";
                return PartialView("_SendGridPartial",_context.SendGridAPI.First());
            }
            catch (EntityException ex)
            {
                ViewBag.Error = ex.Message;
                return PartialView("_SendGridPartial", _context.SendGridAPI.First());
            }
        }

        public ActionResult EditRecaptcha(DbTables.GoogleCaptchaAPI model)
        {
            try
            {
                var GoogleRecaptcha = _context.GoogleCaptchaAPI.First();
                GoogleRecaptcha.Secret = model.Secret;
                GoogleRecaptcha.SiteKey = model.SiteKey;
                _context.SaveChanges();
                ViewBag.Success = "Google ReCaptcha variabler ble sukessfult oppdatert";
                return PartialView("_RecaptchaPartial", _context.GoogleCaptchaAPI.First());
            }
            catch (EntityException ex)
            {
                ViewBag.Error = ex.Message;
                return PartialView("_RecaptchaPartial", _context.GoogleCaptchaAPI.First());

            }
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
    }
}