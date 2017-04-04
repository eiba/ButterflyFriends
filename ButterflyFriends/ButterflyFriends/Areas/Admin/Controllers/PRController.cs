using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class PRController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        // GET: Admin/PR
        public ActionResult Index(string message)
        {
            if (message != null)
            {
                ViewBag.Message = message;
            }
            return View(_context.Articles.ToList());
        }

        public ActionResult Article(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var article = _context.Articles.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }

            return View(article);
        }

        [HttpPost]
        public ActionResult Publish()
        {
            var id = Request.Form["articleid"];
            if (id == "")
            {
                return Json(new { error = true, message = "Artikkelen må lagres først", success = false });
            }
            var article = _context.Articles.Find(int.Parse(id));
            if (article == null)
            {
                return HttpNotFound();
            }
            try
            {

                    article.Published = !article.Published;
                    _context.Entry(article).State = EntityState.Modified;
                    _context.SaveChanges();

                if (article.Published)
                {
                    return Json(new { error = false, message = "Siste lagrede versjon av " + article.Name + " ble publisert", success = true ,published =true});

                }

                    return Json(new { error = false, message = "Siste lagrede versjon av " + article.Name + " ble endret til ikke publisert", success = true, published=false});

            }
            catch (EntityException ex)
            {
                return Json(new { error = true, message = "Error, artikkelens status ble ikke endret: " + ex.Message, success = false });

            }

        }
        [HttpPost]
        public ActionResult Delete()
        {
            var id = Request.Form["articleid"];
            if (id == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }
            var article = _context.Articles.Find(int.Parse(id));
            if (article == null)
            {
                return HttpNotFound();
            }
            try
            {
                var message = "Artikkelen " + article.Name + " ble slettet";
                _context.Articles.Remove(article);
                _context.SaveChanges();



                return Json(new { success=true,error=false, reload= Url.Action("New", "PR", new { message }), url = Url.Action("Index", "PR",new {message}) });
            }
            catch (EntityException ex)
            {
                return Json(new { error = true, message = "Error, artikkelen ble ikke slettet: " + ex.Message, success = false });

            }

        }
        [HttpPost]
        public void DeleteImage()
        {
            var id = Request.Form["imageid"];
            if (id == "")
            {
                return;

            }
            var image = _context.Files.Find(int.Parse(id));
            if (image == null)
            {
                return;
            }

                _context.Files.Remove(image);
                _context.SaveChanges();

        }
        public ActionResult New(string message)
        {
            if (message != null)
            {
                ViewBag.Message = message;
            }

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser currentUser = manager.FindById(User.Identity.GetUserId());

            return View(new ArticleModel { Date = DateTime.Now, Name = currentUser.Fname + " " + currentUser.Lname });
        }

        [HttpPost]
        public ActionResult UploadArticle()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            ApplicationUser currentUser = manager.FindById(User.Identity.GetUserId());
            var content = Request.Unvalidated.Form["article"];
            var title = Request.Unvalidated.Form["article-title"];
            var titleNoHTML = Request.Unvalidated.Form["title"];
            var id = Request.Form["articleid"];
            var articleName = Request.Form["articlenName"];

            if (title==null)
            {
                title = "";
                titleNoHTML = "";
            }
            if (content == null)
            {
                content = "";
            }
            if (currentUser != null)
                {
                    if (currentUser.Employee != null)
                    {
                        if (id == "")
                        {
                            try
                            {
                                DbTables.Article article = new DbTables.Article
                                {
                                    Content = content,
                                    Employee = currentUser.Employee,
                                    Header = title,
                                    Published = false,
                                    Title = titleNoHTML,
                                    Name = articleName,
                                    LastSavedDateTime = DateTime.Now
                                };

                                _context.Articles.Add(article);
                                _context.SaveChanges();

                                return
                                    Json(
                                        new
                                        {
                                            error = false,
                                            message = "Artikkelen "+article.Name+" ble suksessfult lastet opp og lagret",
                                            success = true,
                                            articleid = article.Id
                                        });
                            }
                            catch (EntityException ex)
                            {


                                return Json(new {error = true, message = "Error: " + ex.Message, success = false});
                            }
                        }
                      
                            try
                            {
                                var article = _context.Articles.Find(int.Parse(id));
                                if (article == null)
                                {
                                    return Json(new { error = true, message = "Error, fant ikke artikkelen som skulle lagres", success = false });

                                }

                                article.Header = title;
                                article.Title = titleNoHTML;
                                article.Content = content;
                                article.Name = articleName;


                                article.LastSavedDateTime = DateTime.Now;
                                _context.Entry(article).State = EntityState.Modified;
                                _context.SaveChanges();

                                return
                                    Json(
                                        new
                                        {
                                            error = false,
                                            message = "Artikkelen " + article.Name + " ble lagret",
                                            success = true,
                                            articleid = article.Id
                                        });
                            }
                            catch (EntityException ex)
                            {


                                return Json(new { error = true, message = "Error: " + ex.Message, success = false });
                            }
                        
                    }


                        return
                            Json(
                                new
                                {
                                    error = true,
                                    message = "Du må være ansatt for å legge til artikler",
                                    success = false
                                });
                    
                }

                    return
                        Json(
                            new
                            {
                                error = true,
                                message = "En bruker må være logget inn for å legge til artikkel",
                                success = false
                            });
                


        }

        [HttpPost]
        public ActionResult uploadImage()
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];

            var picture = new DbTables.File
            {
                FileName = Path.GetFileName(file.FileName),
                FileType = DbTables.FileType.ArticleImage,
                ContentType = file.ContentType,
                Temporary = true,
                UploadDate = DateTime.Now

            };

            using (var reader = new BinaryReader(file.InputStream))
            {

                picture.Content = reader.ReadBytes(file.ContentLength);
            }

            Bitmap bmp;
            using (var ms = new MemoryStream(picture.Content))
            {
                bmp = new Bitmap(ms);
            }

            try
            {
                _context.Files.Add(picture);
                _context.SaveChanges();

                return
                    Json(
                        new
                        {
                            size = bmp.Width + "," + bmp.Height,
                            url = "/File?id=" + picture.FileId,
                            id = picture.FileId
                        });
            }
            catch (EntityException ex)
            {

                return Json(new {error = "Error: " + ex.Message});
            }



        }

        [HttpPost]
        public ActionResult rotateImage()
        {
            var id = int.Parse(Request.Form["id"]);
            var direction = Request.Form["direction"];
            if (id == 0 || direction == null)
            {
                return Json(new {error = "Error, ugyldige verdier"});
            }
            var picture = _context.Files.Find(id);
            if (picture == null)
            {
                return Json(new {error = "Error, fant ikke bilde"});
            }
            Bitmap bmp;
            using (var ms = new MemoryStream(picture.Content))
            {
                bmp = new Bitmap(ms);
            }
            if (direction == "CW")
            {
                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

                byte[] content;
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    content = stream.ToArray();
                }
                picture.Content = content;
                try
                {
                    _context.Entry(picture).State = EntityState.Modified;
                    _context.SaveChanges();

                    return
                        Json(
                            new
                            {
                                size = bmp.Width + "," + bmp.Height,
                                url = "/File?id=" + picture.FileId,
                                id = picture.FileId
                            });
                }
                catch (EntityException ex)
                {

                    return Json(new { error = "Error: " + ex.Message });
                }
                
            }
            else
            {
                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                byte[] content;
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    content = stream.ToArray();
                }
                picture.Content = content;
                try
                {
                    _context.Entry(picture).State = EntityState.Modified;
                    _context.SaveChanges();
                    return Json(new { size = bmp.Width + "," + bmp.Height, url = "/File?id=" + picture.FileId, id = picture.FileId });

                }
                catch (EntityException ex)
                {

                    return Json(new {error="Error: "+ex.Message });
                }


            }

        }

        [HttpPost]
        public ActionResult InsertImage()
        {
            var id = int.Parse(Request.Form["id"]);
            var crop = Request.Form["crop"];
            double[] cropList = new double[4];
            if (crop != "0,0,1,1")
            {
                string[] cropArray = Request.Form["crop"].Split(',');
                var index = 0;
                foreach (var num in cropArray)
                {
                    var number = num.Replace(".", ",");
                    cropList[index]=double.Parse(number);
                    index += 1;
                }

            }
            
            var width = int.Parse(Request.Form["width"]);
            if (id == 0 || width == 0)
            {
                return Json(new { error = "Error, ugyldige verdier" });
            }
            var picture = _context.Files.Find(id);
            if (picture == null)
            {
                return Json(new { error = "Error, fant ikke bilde" });
            }
            Bitmap bmp;
            using (var ms = new MemoryStream(picture.Content))
            {
                bmp = new Bitmap(ms);
            }
            if (crop != "0,0,1,1")
            {

                    var x1 = (int) (bmp.Width*cropList[1]);
                    var y1 = (int) (bmp.Height*cropList[0]);

                    var y2 = (int) (bmp.Height*cropList[2]);
                    var x2 = (int) (bmp.Width*cropList[3]);
                    var newWidth = x2 - x1;
                    var newHeight = y2 - y1;
                    Bitmap CroppedImage = cropImage(bmp, new Rectangle(x1, y1, newWidth, newHeight));
                
                byte[] content;
                using (var stream = new MemoryStream())
                {
                    CroppedImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    content = stream.ToArray();
                }
                picture.Content = content;
                try
                {
                    _context.Entry(picture).State = EntityState.Modified;
                    _context.SaveChanges();
                    if (CroppedImage.Width < width)
                    {
                        width = CroppedImage.Width;
                    }
                    double ratio = (double)((double)CroppedImage.Width / (double)CroppedImage.Height);
                    int height = (int)((double)width / ratio);

                    return
                        Json(
                            new
                            {
                                size = width + "," + height,
                                cemax = CroppedImage.Width,
                                url = "/File?id=" + picture.FileId,
                                id = picture.FileId,
                                alt = picture.FileName
                            });

                }
                catch (EntityException ex)
                {

                    return Json(new {error = "Error: " + ex.Message});
                }

            }
            //Image is not to be cropped
            if (bmp.Width < width)
            {
                width = bmp.Width;
            }
            double Ratio = (double)((double)bmp.Width / (double)bmp.Height);
            int Height = (int)((double)width / Ratio);
            return
                       Json(
                           new
                           {
                               size = width + "," + Height,
                               cemax = bmp.Width,
                               url = "/File/?id=" + picture.FileId,
                               id = picture.FileId,
                               alt = picture.FileName
                           });

        }

        public Bitmap cropImage(Bitmap bmp, Rectangle cropArea)
        {
            Bitmap target = new Bitmap(cropArea.Width, cropArea.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(bmp, new Rectangle(0, 0, target.Width, target.Height),
                                 cropArea,
                                 GraphicsUnit.Pixel);
            }
            return target;
        }
        /*
        private static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(resizedImage))
            {
                gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return resizedImage;
        }*/
    }
}