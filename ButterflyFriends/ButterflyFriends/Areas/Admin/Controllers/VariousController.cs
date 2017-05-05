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
            var pdf = new DbTables.File();
            if (terms.Any())
            {
                pdf = terms.First();
            }
            var model = new VariousModel
            {
                GoogleCaptchaAPI = _context.GoogleCaptchaAPI.First(),
                SendGridAPI = _context.SendGridAPI.First(),
                File = pdf
            };
            return View(model);
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
                                ContentType = file.ContentType
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
    }
}