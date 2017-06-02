using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    [Authorize(Roles = "Eier, Admin, Ansatt")]
    public class PRController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        public int pageSize = 4; //how many articles to show per page

        /// <summary>
        ///     Get index method of PR
        /// </summary>
        /// <param name="message">message to display if an article has been deleten and user has been returned to index view</param>
        /// <returns>returns index view</returns>
        // GET: Admin/PR
        public ActionResult Index(string message)
        {
            if (message != null)
                ViewBag.Message = message;
            var articles = from s in _context.Articles
                orderby s.LastSavedDateTime descending
                orderby s.Id descending
                select s;
            ViewBag.page = 1;
            return View(articles.ToPagedList(1, pageSize)); //return articles as pages list
        }

        public ActionResult Email()
        {
            return View();
        }

        /// <summary>
        ///     Sends email
        /// </summary>
        /// <returns>error or success message</returns>
        public ActionResult SendEmail()
        {
            var emailHTML = Request.Unvalidated.Form["html"]; //email text
            var emailSubject = Request.Form["subject"];
            var recipients = Request.Form["recipients"].Split(',');
            var allsponsors = Request.Form["allsponsors"];
            var allemployees = Request.Form["allemployees"];

            if (!string.IsNullOrEmpty(emailHTML) && !string.IsNullOrEmpty(emailSubject))
            {
                if ((recipients[0] == "") && (recipients.Length == 1) && (allsponsors == null) && (allemployees == null))
                    return Json(new {error = true, message = "Error: Ingen mottakere", success = false});
                //no recievers sent in

                try
                {
                    var mailMsg = new MailMessage(); //mailmessage object

                    if (Request.Files.Count > 0)
                    {
                        //  Get all files from Request object  
                        var files = Request.Files;
                        for (var i = 0; i < files.Count; i++)
                        {
                            var file = files[i];

                            mailMsg.Attachments.Add(new Attachment(file.InputStream, Path.GetFileName(file.FileName),
                                file.ContentType));
                        }
                    }

                    // To
                    if ((allsponsors != null) && (allemployees != null)) //add everyone in the database
                    {
                        var results = _context.Users.ToList();
                        foreach (var result in results)
                            mailMsg.To.Add(new MailAddress(result.Email, result.Fname + " " + result.Lname));
                    }
                    else if (allsponsors != null) //add all sponsors
                    {
                        var sponsors = (from s in _context.Users
                            where
                            s.Employee == null
                            select s).ToList();
                        foreach (var sponsor in sponsors)
                            mailMsg.To.Add(new MailAddress(sponsor.Email, sponsor.Fname + " " + sponsor.Lname));
                    }
                    else if (allemployees != null) //add all employees
                    {
                        var employees = (from s in _context.Users
                            where
                            s.Employee != null
                            select s).ToList();
                        foreach (var employee in employees)
                            mailMsg.To.Add(new MailAddress(employee.Email, employee.Fname + " " + employee.Lname));
                    }

                    if (!((recipients[0] == "") && (recipients.Length == 1))) //there's recipients in recipient list
                        foreach (var recipient in recipients)
                            mailMsg.To.Add(new MailAddress(recipient));

                    // From
                    mailMsg.From = new MailAddress("noreply@butterflyfriends.com", "Butterfly Friends");

                    mailMsg.Subject = "Email";

                    var html = emailHTML;
                    mailMsg.Body = html;
                    mailMsg.IsBodyHtml = true;


                    // Init SmtpClient and send
                    var smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                    var SendGridAPIList = _context.SendGridAPI.ToList();
                    var SendGridAPI = new DbTables.SendGridAPI();
                    if (SendGridAPIList.Any())
                        SendGridAPI = SendGridAPIList.First();
                    else
                        return Json(new {error = true, message = "SendGrid er ikke konfigurert", success = false});
                    if (!SendGridAPI.Enabeled) //sendgrid is disabeled
                        return Json(new {error = true, message = "SendGrid er slått av", success = false});

                    var credentials =
                        new NetworkCredential(SendGridAPI.UserName,
                            SendGridAPI.PassWord);
                    smtpClient.Credentials = credentials;

                    smtpClient.Send(mailMsg);

                    return Json(new {error = false, message = "Emailen ble sendt", success = true});
                }
                catch (Exception ex)
                {
                    return Json(new {error = true, message = "Error: " + ex.Message, success = false});
                }
            }
            return Json(new {error = true, message = "Kan ikke sende en tom email", success = false});
        }

        /// <summary>
        ///     Get article
        /// </summary>
        /// <param name="id">id of article</param>
        /// <returns>return view with article</returns>
        public ActionResult Article(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var article = _context.Articles.Find(id);
            if (article == null)
                return HttpNotFound();

            return View(article);
        }

        /// <summary>
        ///     publishes article for display at front page
        /// </summary>
        /// <returns>error or success message</returns>
        [HttpPost]
        public ActionResult Publish()
        {
            var id = Request.Form["articleid"];
            if (id == "")
                return Json(new {error = true, message = "Artikkelen må lagres først", success = false});
            //article is not saved yet
            var article = _context.Articles.Find(int.Parse(id));
            if (article == null)
                return HttpNotFound();
            try
            {
                if (article.FirstPublisheDateTime == null)
                {
                    article.FirstPublisheDateTime = DateTime.Now;
                    ;
                }

                article.Published = !article.Published;
                _context.Entry(article).State = EntityState.Modified;
                _context.SaveChanges();

                if (article.Published)
                    return
                        Json(
                            new
                            {
                                error = false,
                                message = "Siste lagrede versjon av " + article.Name + " ble publisert",
                                success = true,
                                published = true
                            }); //publishing was succsessful

                return
                    Json(
                        new
                        {
                            error = false,
                            message = "Siste lagrede versjon av " + article.Name + " ble endret til ikke publisert",
                            success = true,
                            published = false
                        });
            }
            catch (EntityException ex)
            {
                return
                    Json(
                        new
                        {
                            error = true,
                            message = "Error, artikkelens status ble ikke endret: " + ex.Message,
                            success = false
                        });
            }
        }

        /// <summary>
        ///     Delete article
        /// </summary>
        /// <returns>return succsses or error message</returns>
        [HttpPost]
        public ActionResult Delete()
        {
            var id = Request.Form["articleid"];
            if (id == "")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var article = _context.Articles.Find(int.Parse(id));
            if (article == null)
                return HttpNotFound();
            try
            {
                var message = "Artikkelen " + article.Name + " ble slettet";
                _context.Files.RemoveRange(article.Images);
                _context.Articles.Remove(article);
                _context.SaveChanges();


                return
                    Json(
                        new
                        {
                            success = true,
                            error = false,
                            reload = Url.Action("New", "PR", new {message}),
                            url = Url.Action("Index", "PR", new {message})
                        });
            }
            catch (EntityException ex)
            {
                return
                    Json(
                        new
                        {
                            error = true,
                            message = "Error, artikkelen ble ikke slettet: " + ex.Message,
                            success = false
                        });
            }
        }

        /// <summary>
        ///     Deletes image from article and database
        /// </summary>
        [HttpPost]
        public void DeleteImage()
        {
            var id = Request.Form["imageid"];
            if (id == "")
                return;
            var image = _context.Files.Find(int.Parse(id));
            if (image == null)
                return;

            _context.Files.Remove(image);
            _context.SaveChanges();
        }

        /// <summary>
        ///     Show new article view
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ActionResult New(string message)
        {
            if (message != null)
                ViewBag.Message = message;

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var currentUser = manager.FindById(User.Identity.GetUserId());

            return View(new ArticleModel {Date = DateTime.Now, Name = currentUser.Fname + " " + currentUser.Lname});
        }

        /// <summary>
        ///     Uploads article
        /// </summary>
        /// <returns>error or succsess</returns>
        [HttpPost]
        public ActionResult UploadArticle()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            //article parts from form
            var content = Request.Unvalidated.Form["article"];
            var title = Request.Unvalidated.Form["article-title"];
            var preamble = Request.Unvalidated.Form["article-preamble"];
            var preambleNoHTML = Request.Unvalidated.Form["preamble"];
            var titleNoHTML = Request.Unvalidated.Form["title"];
            var id = Request.Form["articleid"];
            var articleName = Request.Unvalidated.Form["articlenName"];
            var TitleInner = Request.Unvalidated.Form["title-inner"];
            var PreambleInner = Request.Unvalidated.Form["preamble-inner"];
            var imagesList = new List<DbTables.File>();
            if (Request.Form["images"] != "none") //get image ids from form. Images that are currently in the article
            {
                var images = Array.ConvertAll(Request.Form["images"].Split(','), s => int.Parse(s));
                imagesList = _context.Files.Where(s => images.Contains(s.FileId)).ToList();
            }

            if (title == null)
            {
                title = "";
                titleNoHTML = "";
                TitleInner = "";
            }
            if (preamble == null)
            {
                preamble = "";
                preambleNoHTML = "";
                PreambleInner = "";
            }
            if (content == null)
                content = "";
            if (currentUser != null)
            {
                if (currentUser.Employee != null)
                {
                    if (id == "") //save new article
                        try
                        {
                            IList<DbTables.Employees> autorList = new List<DbTables.Employees>();
                            autorList.Add(currentUser.Employee);
                            var article = new DbTables.Article //create article object
                            {
                                Content = content,
                                Employees = autorList,
                                Header = title,
                                Published = false,
                                Title = titleNoHTML,
                                Name = articleName,
                                LastSavedDateTime = DateTime.Now,
                                Images = imagesList,
                                Preamble = preamble,
                                PreambleNoHTML = preambleNoHTML,
                                TitleInner = TitleInner,
                                PreambleInner = PreambleInner
                            };
                            foreach (var image in imagesList) //get images and change their temporary status to false
                                if (image != null)
                                {
                                    image.Temporary = false;
                                    _context.Entry(image).State = EntityState.Modified;
                                }
                            _context.Articles.Add(article);
                            _context.SaveChanges();

                            return
                                Json(
                                    new
                                    {
                                        error = false,
                                        message = "Artikkelen " + article.Name + " ble suksessfult lastet opp og lagret",
                                        success = true,
                                        articleid = article.Id
                                    });
                        }
                        catch (EntityException ex)
                        {
                            return Json(new {error = true, message = "Error: " + ex.Message, success = false});
                        }

                    try
                    {
                        var article = _context.Articles.Find(int.Parse(id));
                        if (article == null)
                            return
                                Json(
                                    new
                                    {
                                        error = true,
                                        message = "Error, fant ikke artikkelen som skulle lagres",
                                        success = false
                                    });
                        IList<DbTables.File> imagesToRemove = new List<DbTables.File>();
                        foreach (var image in article.Images)
                            //check if there are any images related to the article which are not in the article anymore and delete them
                        {
                            if (imagesList.Contains(image))
                                continue;
                            imagesToRemove.Add(image);
                        }
                        foreach (var image in imagesList)
                            if ((image != null) && image.Temporary)
                            {
                                image.Temporary = false;
                                _context.Entry(image).State = EntityState.Modified;
                            }
                        article.Images.Clear();
                        _context.Files.RemoveRange(imagesToRemove);

                        var newAuthor = true;
                        foreach (var employee in article.Employees)
                            if (employee.User.Id == currentUser.Id)
                                newAuthor = false;
                        if (newAuthor) //new author, add to authors
                            article.Employees.Add(currentUser.Employee);
                        article.Header = title;
                        article.Title = titleNoHTML;
                        article.Content = content;
                        article.Name = articleName;
                        article.Images = imagesList;
                        article.Preamble = preamble;
                        article.PreambleNoHTML = preambleNoHTML;
                        article.TitleInner = TitleInner;
                        article.PreambleInner = PreambleInner;


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
                        return Json(new {error = true, message = "Error: " + ex.Message, success = false});
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

        /// <summary>
        ///     upload article image
        /// </summary>
        /// <returns>return image data, or error message</returns>
        [HttpPost]
        public ActionResult uploadImage()
        {
            var files = Request.Files;
            var file = files[0];
            var maxWidth = 800; //max image with allowed in article
            var picture = new DbTables.File //new picture object
            {
                FileName = Path.GetFileName(file.FileName),
                FileType = DbTables.FileType.ArticleImage,
                ContentType = file.ContentType,
                Temporary = true,
                UploadDate = DateTime.Now
            };

            byte[] content;
            using (var reader = new BinaryReader(file.InputStream))
            {
                content = reader.ReadBytes(file.ContentLength);
            }

            Bitmap bmp; //convert to bitmap
            using (var ms = new MemoryStream(content))
            {
                bmp = new Bitmap(ms);
            }

            if (bmp.Width > maxWidth)
            {
                //if bitmap with is greater than max allowed with, scale down image
                var ratio = bmp.Width/(double) bmp.Height; //get height to width ratio
                var height = (int) (maxWidth/ratio); //get new calculated height
                bmp = ResizeImage(bmp, maxWidth, height); //resize image with new height and maxwidth

                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, ImageFormat.Png);
                    content = stream.ToArray();
                }
            }
            picture.Content = content;

            try
            {
                _context.Files.Add(picture);
                _context.SaveChanges();

                return
                    Json(
                        new
                        {
                            size = bmp.Width + "," + bmp.Height,
                            url = "/File/ArticleImage?id=" + picture.FileId, //return image url, id and size
                            id = picture.FileId
                        });
            }
            catch (EntityException ex)
            {
                return Json(new {error = "Error: " + ex.Message});
            }
        }

        /// <summary>
        ///     rotate the image
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult rotateImage()
        {
            var id = int.Parse(Request.Form["id"]);
            var direction = Request.Form["direction"]; //direction to rotate, CW or CCW (counter clockwise)
            if ((id == 0) || (direction == null))
                return Json(new {error = "Error, ugyldige verdier"});
            var picture = _context.Files.Find(id);
            if (picture == null)
                return Json(new {error = "Error, fant ikke bilde"});
            Bitmap bmp;
            using (var ms = new MemoryStream(picture.Content)) //get bitmap of image
            {
                bmp = new Bitmap(ms);
            }
            if (direction == "CW")
            {
                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone); //rotate image 90 degrees

                byte[] content;
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, ImageFormat.Png);
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
                                url = "/File/ArticleImage?id=" + picture.FileId,
                                id = picture.FileId
                            });
                }
                catch (EntityException ex)
                {
                    return Json(new {error = "Error: " + ex.Message});
                }
            }
            else
            {
                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                //rotate image 270 degrees to simulate 90 degrees clockwise

                byte[] content;
                using (var stream = new MemoryStream())
                {
                    bmp.Save(stream, ImageFormat.Png);
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
                                url = "/File/ArticleImage?id=" + picture.FileId,
                                id = picture.FileId
                            }); //return new height and width of new image
                }
                catch (EntityException ex)
                {
                    return Json(new {error = "Error: " + ex.Message});
                }
            }
        }

        /// <summary>
        ///     Insert image into article
        /// </summary>
        /// <returns>error or image data</returns>
        [HttpPost]
        public ActionResult InsertImage()
        {
            var id = int.Parse(Request.Form["id"]);
            var crop = Request.Form["crop"];
            var cropList = new double[4];
            if (crop != "0,0,1,1") //check if image should be cropped
            {
                var cropArray = Request.Form["crop"].Split(','); //get the crop values
                var index = 0;
                foreach (var num in cropArray)
                {
                    var number = num.Replace(".", ",");
                    cropList[index] = double.Parse(number);
                    index += 1;
                }
            }

            var width = int.Parse(Request.Form["width"]); //get image width
            if ((id == 0) || (width == 0))
                return Json(new {error = "Error, ugyldige verdier"});
            var picture = _context.Files.Find(id);
            if (picture == null)
                return Json(new {error = "Error, fant ikke bilde"});
            Bitmap bmp;
            using (var ms = new MemoryStream(picture.Content))
            {
                bmp = new Bitmap(ms);
            }
            if (crop != "0,0,1,1") //crop image
            {
                //get new cropped x and y values
                var x1 = (int) (bmp.Width*cropList[1]);
                var y1 = (int) (bmp.Height*cropList[0]);

                var y2 = (int) (bmp.Height*cropList[2]);
                var x2 = (int) (bmp.Width*cropList[3]);

                //get new height and width
                var newWidth = x2 - x1;
                var newHeight = y2 - y1;
                var CroppedImage = cropImage(bmp, new Rectangle(x1, y1, newWidth, newHeight));
                //crop the image and display only new region

                byte[] content;
                using (var stream = new MemoryStream())
                {
                    CroppedImage.Save(stream, ImageFormat.Png);
                    content = stream.ToArray();
                }
                picture.Content = content;
                try
                {
                    _context.Entry(picture).State = EntityState.Modified;
                    _context.SaveChanges();
                    if (CroppedImage.Width < width)
                        width = CroppedImage.Width;
                    var ratio = CroppedImage.Width/(double) CroppedImage.Height;
                    var height = (int) (width/ratio); //get new height of cropped image

                    return
                        Json(
                            new
                            {
                                size = width + "," + height,
                                cemax = CroppedImage.Width,
                                url = "/File/ArticleImage?id=" + picture.FileId,
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
                width = bmp.Width;
            var Ratio = bmp.Width/(double) bmp.Height;
            var Height = (int) (width/Ratio);
            return
                Json(
                    new
                    {
                        size = width + "," + Height,
                        cemax = bmp.Width,
                        url = "/File/ArticleImage?id=" + picture.FileId,
                        id = picture.FileId,
                        alt = picture.FileName
                    });
        }

        /// <summary>
        ///     Function used to crop image
        /// </summary>
        /// <param name="bmp">bitmap of image to be cropped</param>
        /// <param name="cropArea">croparea representet by a rectangle</param>
        /// <returns>cropped image</returns>
        public Bitmap cropImage(Bitmap bmp, Rectangle cropArea)
        {
            var target = new Bitmap(cropArea.Width, cropArea.Height); //define the bitmap target 

            using (var g = Graphics.FromImage(target)) //draw image over on new cropped rectangle
            {
                g.DrawImage(bmp, new Rectangle(0, 0, target.Width, target.Height),
                    cropArea,
                    GraphicsUnit.Pixel);
            }
            return target;
        }

        /// <summary>
        ///     Get users based on query using database optimization
        /// </summary>
        /// <returns>list of user objects</returns>
        public ActionResult GetUsers()
        {
            var k = Request.Form["query"];
            var enteties = new List<object>();
            if (!string.IsNullOrEmpty(k))
            {
                //Get user and decrease row accsess
                var users =
                    _context.Users.Where(s => (s.Fname + " " + s.Lname).Contains(k) || s.Email.Contains(k))
                        .OrderBy(p => p.Lname)
                        .Select(x => new UserObj
                        {
                            name = x.Fname + " " + x.Lname,
                            thumbnail = x.Thumbnail,
                            email = x.Email,
                            employee = x.Employee
                        }).ToList();

                foreach (var user in users)
                {
                    var imgId = 1; //if user has no profile image, return id of default image
                    var isEmployee = new bool();
                    if (user.employee != null)
                        isEmployee = true;
                    if (user.thumbnail != null)
                        imgId = user.thumbnail.ThumbNailId;
                    enteties.Add(
                        new
                        {
                            name = user.name + " " + user.email,
                            fullname = user.name,
                            id = user.email,
                            imgid = imgId,
                            employee = isEmployee
                        });
                }
            }
            return Json(enteties);
        }

        /// <summary>
        ///     Return list of articles based on filter options
        /// </summary>
        /// <param name="page">page where the user is at the moment</param>
        /// <returns>returns a filter result</returns>
        public ActionResult ArticleList(int? page)
        {
            var published = Request.Form["published"];
            var filter = Request.Form["filter"];
            var search = Request.Form["search"];
            var author = Request.Form["author"];
            var content = Request.Form["content"];
            var order = Request.Form["order"];

            var pageNumber = page ?? 1;
            ViewBag.page = pageNumber;

            return FilterResult(published, search, content, filter, pageNumber, order, author);
        }

        /// <summary>
        ///     Filter articles
        /// </summary>
        /// <returns>returns filter result</returns>
        public ActionResult Filter()
        {
            var published = Request.Form["published"];
            var filter = Request.Form["filter"];
            var search = Request.Form["search"];
            var author = Request.Form["author"];
            var content = Request.Form["content"];
            var order = Request.Form["order"];

            var pageNumber = 1;
            ViewBag.page = 1;


            return FilterResult(published, search, content, filter, pageNumber, order, author);
        }

        /// <summary>
        ///     Filter out articles based on criteria
        /// </summary>
        /// <param name="published"></param>
        /// <param name="search"></param>
        /// <param name="content"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="order"></param>
        /// <param name="author"></param>
        /// <returns>partial view with resulting articles</returns>
        public PartialViewResult FilterResult(string published, string search, string content, string filter,
            int pageNumber, string order, string author)
        {
            var articles = from s in _context.Articles
                where (s.Name.Contains(search) ||
                       s.Title.Contains(search)) &&
                      (s.Content.Contains(content) ||
                       s.PreambleNoHTML.Contains(content))
                select s;

            if (published == "yes")
                articles = articles.Where(s => s.Published);
            else if (published == "no")
                articles = articles.Where(s => !s.Published);
            if (!string.IsNullOrEmpty(author)) //get the articles of author in query
            {
                IList<DbTables.Article> Articles = new List<DbTables.Article>();
                foreach (var article in articles.ToList())
                    foreach (var articleAuthor in article.Employees.ToList())
                    {
                        var user = articleAuthor.User;
                        var name = user.Fname + " " + user.Lname;
                        if (name.ToLower().Contains(author.ToLower()))
                        {
                            Articles.Add(article);
                            break;
                        }
                    }
                articles = Articles.AsQueryable();
            }
            if (order == "descending") //choose what to order by
                switch (filter)
                {
                    case "1":
                        articles =
                            articles.OrderByDescending(s => s.FirstPublisheDateTime.Value)
                                .ThenByDescending(s => s.Id);
                        break;
                    case "2":
                        articles = articles.OrderByDescending(s => s.Title);
                        break;
                    case "3":
                        articles = articles.OrderByDescending(s => s.Name);
                        break;
                    default:
                        articles =
                            articles.OrderByDescending(s => s.LastSavedDateTime)
                                .ThenByDescending(s => s.Id);
                        break;
                }
            else
                switch (filter) //choose what to filter by
                {
                    case "1":
                        articles =
                            articles.OrderBy(s => s.FirstPublisheDateTime.Value)
                                .ThenBy(s => s.Id);
                        break;
                    case "2":
                        articles = articles.OrderBy(s => s.Title);
                        break;
                    case "3":
                        articles = articles.OrderBy(s => s.Name);
                        break;
                    default:
                        articles =
                            articles.OrderBy(s => s.LastSavedDateTime)
                                .ThenBy(s => s.Id);
                        break;
                }

            return PartialView("_ArticleListPartial", articles.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        ///     Resized image
        /// </summary>
        /// <param name="image">Image to resize</param>
        /// <param name="width">with of image to be created</param>
        /// <param name="height">height of image to created</param>
        /// <returns>Returns a bitmap with resized image</returns>
        private static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            var resizedImage = new Bitmap(width, height);
            using (var gfx = Graphics.FromImage(resizedImage))
            {
                gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return resizedImage;
        }
    }
}