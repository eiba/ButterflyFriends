using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;
using ButterflyFriends.Controllers;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json;
using PagedList;
using WebGrease.Css.Extensions;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    [Authorize(Roles = "Eier, Admin")]
    public class HRManagementController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();
        public int pageSize = 10;   //entities per page

        // GET: Admin/HRManagement
        /// <summary>
        /// index page, show initial list elements
        /// </summary>
        /// <returns>model with list elements of all entities</returns>
        public ActionResult Index()
        {
            var users = from s in _context.Users
                orderby s.Lname
                select s;

            var children = from s in _context.Children
                orderby s.Lname
                select s;

            var model = new HRManagamentModel
            {
                EmployeeModel = new EmployeeModel
                {
                    Employees = users.Where(s => s.Employee != null).ToPagedList(1, pageSize)
                },
                ChildrenModel = new ChildrenModel {Children = children.ToPagedList(1, pageSize)},
                SponsorModel = new SponsorModel {Sponsors = users.Where(s => s.Employee == null).ToPagedList(1, pageSize)}
            };
            return View(model);
        }

        /// <summary>
        /// Upload profile picture
        /// </summary>
        /// <returns>returns and shows the new profile picture</returns>
        [HttpPost]
        public ActionResult ProfilePictureUpload()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    var store = new UserStore<ApplicationUser>(_context);
                    var manager = new UserManager<ApplicationUser>(store);
                    ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;  //the current user
                    var userId = Request.Form["userid"];
                    var ProfileImageWidth = 300;    //max profile image width
                    var ThumbnailImageWidth = 40;   //thumbnail width
                    var type = Request.Form["type"];    //type of entity. User or child.
                    var fileId = 0;
                    var employee = new ApplicationUser();
                    var child = new DbTables.Child();
                    IList<DbTables.File> pictures = new List<DbTables.File>();
                    if (type == "employee")
                    {
                        employee = _context.Users.Find(userId);
                        if (currentUser.RoleNr >= employee.RoleNr)   //user not the current user or rolenumber to high, not allowed to change profile picture
                        {
                            if (currentUser.Id != userId)
                            {
                                return Json(new { error= "Du har ikke lov til å endre dette profilbildet" ,success="false"});

                            }

                        }

                        pictures = employee.Pictures;
                    }
                    else if (type == "child")
                    {
                        child = _context.Children.Find(Int32.Parse(userId));    //get child id
                        pictures = child.Pictures;
                    }

                    var profPic = new DbTables.File();
                    var thumbNail = new DbTables.ThumbNail();
                    if (pictures.Any()) //check if picture is already uploaded
                    {
                        foreach (var pic in pictures)
                        {
                            if (pic.FileType == DbTables.FileType.Profile)
                            {
                                profPic = pic;
                                thumbNail = pic.ThumbNail;
                            }
                        }
                    }
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];

                        if (profPic.Content == null)
                        {
                            var picture = new DbTables.File();
                            //save profile picture
                            if (employee.Email != null) //check if entity is employee
                            {
                                picture = new DbTables.File
                                {
                                    FileName = Path.GetFileName(file.FileName),
                                    FileType = DbTables.FileType.Profile,
                                    ContentType = file.ContentType,
                                    User = new List<ApplicationUser> {employee}
                                };
                            }
                            else
                            {
                                picture = new DbTables.File //entity is a child
                                {
                                    FileName = Path.GetFileName(file.FileName),
                                    FileType = DbTables.FileType.Profile,
                                    ContentType = file.ContentType,
                                    Children = new List<DbTables.Child> {child}
                                };
                            }
                            byte[] content;
                            using (var reader = new BinaryReader(file.InputStream))
                            {

                                content = reader.ReadBytes(file.ContentLength);
                            }

                            Bitmap bmp;
                            using (var ms = new MemoryStream(content))  //get bitmap of image
                            {
                                bmp = new Bitmap(ms);
                            }
                            double ratio = (double) ((double) bmp.Width/(double) bmp.Height);   //get height width ratio
                            int height = (int) ((double) ProfileImageWidth/ratio);
                            var newImg = ResizeImage(bmp, ProfileImageWidth, height);   //resize image so that it cannot exceed 300 pixels

                            byte[] cntnt;
                            using (var stream = new MemoryStream())
                            {
                                newImg.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                cntnt = stream.ToArray();
                            }
                            picture.Content = cntnt;

                            _context.Files.Add(picture);

                            //save thumbnail
                            var thumbnail = new DbTables.ThumbNail
                            {
                                ThumbNailName = Path.GetFileName(file.FileName),
                                ContentType = file.ContentType,
                                FileType = DbTables.FileType.Thumbnail,
                                File = picture
                            };

                            int Theight = (int) ((double) ThumbnailImageWidth/ratio);   
                            var newThumb = ResizeImage(bmp, ThumbnailImageWidth, Theight);  //resize profile image to 40 pixels to create a thumbnail
                            byte[] Tcntnt;
                            using (var stream = new MemoryStream())
                            {
                                newThumb.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                Tcntnt = stream.ToArray();
                            }
                            thumbnail.Content = Tcntnt;
                            _context.ThumbNails.Add(thumbnail);

                            if (employee.Email != null)
                            {
                                employee.Thumbnail = thumbnail;
                                _context.Entry(employee).State = EntityState.Modified;
                            }
                            else
                            {
                                child.Thumbnail = thumbnail;
                                _context.Entry(child).State = EntityState.Modified;
                            }
                            _context.SaveChanges();
                            fileId = picture.FileId;
                        }
                        else
                        {   //there already exist a profile image in database
                            //Profile picture
                            profPic.FileName = Path.GetFileName(file.FileName);
                            profPic.ContentType = file.ContentType;

                            byte[] content;
                            using (var reader = new BinaryReader(file.InputStream))
                            {

                                content = reader.ReadBytes(file.ContentLength);
                            }

                            Bitmap bmp;
                            using (var ms = new MemoryStream(content))
                            {
                                bmp = new Bitmap(ms);
                            }
                            double ratio = (double) ((double) bmp.Width/(double) bmp.Height);   //ratio of width and height
                            int height = (int) ((double) ProfileImageWidth/ratio);
                            var newImg = ResizeImage(bmp, ProfileImageWidth, height);       //resize image

                            byte[] cntnt;
                            using (var stream = new MemoryStream())
                            {
                                newImg.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                cntnt = stream.ToArray();
                            }
                            profPic.Content = cntnt;

                            _context.Entry(profPic).State = EntityState.Modified;

                            //thumbnail
                            thumbNail.ThumbNailName = Path.GetFileName(file.FileName);
                            thumbNail.ContentType = file.ContentType;

                            int Theight = (int) ((double) ThumbnailImageWidth/ratio);
                            var newThumb = ResizeImage(bmp, ThumbnailImageWidth, Theight);  //resize image to thubnail
                            byte[] Tcntnt;
                            using (var stream = new MemoryStream())
                            {
                                newThumb.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                Tcntnt = stream.ToArray();
                            }
                            thumbNail.Content = Tcntnt;
                            _context.Entry(thumbNail).State = EntityState.Modified;

                            _context.SaveChanges();
                            fileId = profPic.FileId;
                        }

                    }
                    // Returns message that successfully uploaded
                    return PartialView("UploadImagePartials/_ProfileImagePartial", new FileModel {FileId = fileId});
                    //return Json("Filopplastning var en suksess!");
                }
                catch (Exception ex)
                {
                    return Json("Ooops, det skjedde en feil: " + ex.Message);
                }
            }

                return Json("Ingen filer valgt");

        }

        /// <summary>
        /// Rezises image so that it will be be compressed with smaller dimensions
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics gfx = Graphics.FromImage(resizedImage))
            {
                gfx.DrawImage(image, new Rectangle(0, 0, width, height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            return resizedImage;
        }

        /// <summary>
        /// returns employee list based on search criteria
        /// </summary>
        /// <param name="employeePage"></param>
        /// <returns></returns>
        public ActionResult EmployeeList(int? employeePage)
        {

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var employeePhone = (Request.Form["employeePhone"]);
            var EmployeeStreetadress = Request.Form["EmployeeStreetadress"];
            var EmployeeZip = (Request.Form["EmployeeZip"]);
            var EmployeeCity = Request.Form["EmployeeCity"];
            var EmployeeCounty = Request.Form["EmployeeCounty"];
            var EmployeePosition = Request.Form["EmployeePosition"];
            var employeeAccountNumber = (Request.Form["employeeAccountNumber"]);

            int pageNumber = (employeePage ?? 1);
            ViewBag.employeePage = (employeePage ?? 1);

            return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

        }

        public ActionResult ChildrenList(int? childrenPage)
        {

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var ChildDoB = (Request.Form["ChildDoB"]);
            var ChildSponsor = Request.Form["ChildSponsor"];
            int pageNumber = (childrenPage ?? 1);
            ViewBag.childrenPage = (childrenPage ?? 1);

            return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);

        }

        public ActionResult SponsorList(int? sponsorPage)
        {

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var SponsorPhone = (Request.Form["SponsorPhone"]);
            var SponsorStreetadress = Request.Form["SponsorStreetadress"];
            var SponsorZip = (Request.Form["SponsorZip"]);
            var SponsorCity = Request.Form["SponsorCity"];
            var SponsorCounty = Request.Form["SponsorCounty"];

            int pageNumber = (sponsorPage ?? 1);
            ViewBag.sponsorPage = (sponsorPage ?? 1);

            return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

        }
        /****************Detail functions**************/

            /// <summary>
            /// gets and returns details modal partial view
            /// </summary>
            /// <param name="id">id of entity</param>
            /// <param name="type">type of entity</param>
            /// <param name="employeePage">employee list page</param>
            /// <returns></returns>
        [HttpGet]
        public ActionResult showEmployeeDetails(string id, string type, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var pictures = user.Pictures;

            if (pictures.Any())
            {
                foreach (var pic in pictures)
                {
                    if (pic.FileType == DbTables.FileType.Profile)
                    {
                        return PartialView("DetailPartials/_EmployeeDetailsPartial",new EmployeeDetailsModel { File = new FileModel { FileId = pic.FileId, Type = "employee" },User = user});
                    }
                }
            }
            return PartialView("DetailPartials/_EmployeeDetailsPartial", new EmployeeDetailsModel { File = new FileModel { Type = "employee" }, User = user });


        }
        /// <summary>
        /// show sponsor modal page
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult showSponsorDetails(string id, string type, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var pictures = user.Pictures;

            if (pictures.Any())
            {
                foreach (var pic in pictures)
                {
                    if (pic.FileType == DbTables.FileType.Profile)
                    {
                        return PartialView("DetailPartials/_SponsorDetailsPartial", new EmployeeDetailsModel { File = new FileModel { FileId = pic.FileId, Type = "employee" }, User = user });
                    }
                }
            }
            return PartialView("DetailPartials/_SponsorDetailsPartial", new EmployeeDetailsModel { File = new FileModel { Type = "employee" }, User = user });


        }
        /// <summary>
        /// show child 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="childrenPage"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult showChildDetails(int id, string type, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var pictures = child.Pictures;

            if (pictures.Any())
            {
                foreach (var pic in pictures)
                {
                    if (pic.FileType == DbTables.FileType.Profile)
                    {
                        return PartialView("DetailPartials/_ChildDetailsPartial", new ChildDetailsModel { File = new FileModel { FileId = pic.FileId, Type = "child" }, Child = child });
                    }
                }
            }
            return PartialView("DetailPartials/_ChildDetailsPartial", new ChildDetailsModel { File = new FileModel { Type = "child" },Child = child});
        }

        /************************************************/
        /// <summary>
        /// show edit modal for employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employeePage"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult showEmployeeEdit(string id, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new changeUserInfo
            {
                Fname = user.Fname,
                Lname = user.Lname,
                Id = user.Id,
                Phone = user.Phone,
                PostCode = user.Adress.PostCode,
                City = user.Adress.City,
                StreetAdress = user.Adress.StreetAdress,
                State = user.Adress.County,
                Email = user.Email,
                Position = user.Employee.Position,
                AccountNumber = user.Employee.AccountNumber,
                RoleNr = user.RoleNr

            };

            return PartialView("EditPartials/_EmployeeEditPartial", model);
        }

        /// <summary>
        /// show sponsor edit modal
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult showSponsorEdit(string id, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new changeUserInfo
            {
                Fname = user.Fname,
                Lname = user.Lname,
                Id = user.Id,
                Phone = user.Phone,
                PostCode = user.Adress.PostCode,
                City = user.Adress.City,
                StreetAdress = user.Adress.StreetAdress,
                State = user.Adress.County,
                Email = user.Email

            };

            return PartialView("EditPartials/_SponsorEditPartial", model);
        }

        /// <summary>
        /// show child edit modal
        /// </summary>
        /// <param name="id"></param>
        /// <param name="childrenPage"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult showChildEdit(int? id, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                return HttpNotFound();
            }
            ChildCreateModel model;
            if (child.SponsorId == null)
            {
                model = new ChildCreateModel
                {
                    Child = child
                };
            }
            else
            {
                model = new ChildCreateModel
                {
                    Child = child,
                    SponsorName = child.User.Fname + " " + child.User.Lname
                };
            }
            return PartialView("EditPartials/_ChildEditPartial", model);

        }

        /// <summary>
        /// edit child entity
        /// </summary>
        /// <param name="model">child create model</param>
        /// <param name="childrenPage">page of children list</param>
        /// <returns>updated childrens list</returns>
        [HttpPost]
        public ActionResult ChildEdit(ChildCreateModel model, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var ChildDoB = (Request.Form["ChildDoB"]);
            var ChildSponsor = Request.Form["ChildSponsor"];

            int pageNumber = (childrenPage ?? 1);

            if (ModelState.IsValid)
            {
                var sponsormsg = "";
                if (model.Child.Id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var child = _context.Children.Find(model.Child.Id);
                if (child == null)
                {
                    return HttpNotFound();
                }
                if (model.Child.DoB > DateTime.Now)
                {
                    ViewBag.Error = "Barnet kan ikke være født i fremtiden!";

                    return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
                }

                child.DoB = model.Child.DoB;
                child.Fname = model.Child.Fname;
                child.Lname = model.Child.Lname;
                if (model.Child.SponsorId != null && model.SponsorName != null) //try to add sponsor, if it exists
                {
                    var user = _context.Users.Find(model.Child.SponsorId);
                    if (user != null)
                    {
                        if (user.Fname + " " + user.Lname != model.SponsorName)
                        {
                            sponsormsg = ", men fant ikke bruker " + model.SponsorName;
                        }
                        else
                        {
                            child.SponsorId = model.Child.SponsorId;
                        }
                    }
                }
                else if (model.Child.SponsorId == null && model.SponsorName != null)
                {
                    sponsormsg = ", men fant ikke bruker " + model.SponsorName;
                }
                else if (model.SponsorName == null)
                {
                    child.SponsorId = null;
                }
                try
                {
                    _context.Entry(child).State = EntityState.Modified;
                    _context.SaveChanges();

                    ViewBag.Success = "Fadderbarnet " + model.Child.Fname + " " + model.Child.Lname + " ble oppdatert" +sponsormsg;
                    ViewBag.Id = model.Child.Id;
                    return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
                }
                catch (EntityException ex)
                {

                    ViewBag.Error = "Error: "+ex.Message;
                    ViewBag.Id = model.Child.Id;

                    return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
                }

            }

            ViewBag.Error = "Oops! Ugyldige verdier";
            ViewBag.Id = model.Child.Id;

            return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
        }
        /// <summary>
        /// create employee modal
        /// </summary>
        /// <param name="employeePage"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult showEmployeeCreate(int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);

            return PartialView("CreatePartials/_EmployeeCreatePartial", new RegisterViewModel());
        }
        /// <summary>
        /// sponsor create modal
        /// </summary>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult showSponsorCreate(int? sponsorPage)
        {
            ViewBag.employeePage = (sponsorPage ?? 1);

            return PartialView("CreatePartials/_SponsorCreatePartial", new RegisterViewModel());
        }
        /// <summary>
        /// children create modal
        /// </summary>
        /// <param name="childrenPage"></param>
        /// <returns></returns>
        public ActionResult showChildCreate(int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);

            return PartialView("CreatePartials/_ChildCreatePartial", new ChildCreateModel());
        }

        /************************deactivate functions**********************/
        
        /// <summary>
        /// deactivate employee
        /// </summary>
        /// <param name="id">id to deactivate</param>
        /// <param name="employeePage">page of employee list</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> employeeDeactivate(string id, int? employeePage)
        {
            //filter values
            ViewBag.employeePage = (employeePage ?? 1); 
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var employeePhone = (Request.Form["employeePhone"]);
            var EmployeeStreetadress = Request.Form["EmployeeStreetadress"];
            var EmployeeZip = (Request.Form["EmployeeZip"]);
            var EmployeeCity = Request.Form["EmployeeCity"];
            var EmployeeCounty = Request.Form["EmployeeCounty"];
            var EmployeePosition = Request.Form["EmployeePosition"];
            var employeeAccountNumber = (Request.Form["employeeAccountNumber"]);

            int pageNumber = (employeePage ?? 1);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);
            ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            if (currentUser.RoleNr >= user.RoleNr)  //rolenumber is too low, return error message
            {
                ViewBag.Error = "Du kan ikke deaktivere en bruker på samme eller høyere brukernivå";
                ViewBag.Id = user.Id;
                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            user.LockoutEnabled = true; //lock out user
            user.LockoutEndDateUtc = new DateTime(9999, 12, 30); //lock out until year 9999, which is basically forever
            user.InactiveSince = DateTime.Now;  //deactivated since now
            user.IsEnabeled = false;

            var result = await manager.UpdateAsync(user);
            _context.SaveChanges();

            if (result.Succeeded)
            {
                ViewBag.Success = "Brukeren " + user.Email + " har blitt deaktivert.";
                ViewBag.Id = user.Id;
            }
            else
            {

                ViewBag.Error = "Noe gikk galt - " + result.Errors;
                ViewBag.Id = user.Id;
                ViewBag.Worked = false;
            }
            return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

        }

        /// <summary>
        /// deactivate sponsor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> sponsorDeactivate(string id, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var SponsorPhone = (Request.Form["SponsorPhone"]);
            var SponsorStreetadress = Request.Form["SponsorStreetadress"];
            var SponsorZip = (Request.Form["SponsorZip"]);
            var SponsorCity = Request.Form["SponsorCity"];
            var SponsorCounty = Request.Form["SponsorCounty"];  //filter values

            int pageNumber = (sponsorPage ?? 1);
            if (id == null)
            {
                ViewBag.Error = "Id ikke funnet";
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                ViewBag.Error = "Bruker ikke funet";
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);

            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = new DateTime(9999, 12, 30);
            user.InactiveSince = DateTime.Now;
            user.IsEnabeled = false;

            var result = await manager.UpdateAsync(user);
            _context.SaveChanges();

            if (result.Succeeded)
            {
                ViewBag.Success = "Brukeren " + user.Email + " har blitt deaktivert.";
                ViewBag.Id = user.Id;
            }
            else
            {

                ViewBag.Error = "Noe gikk galt - " + result.Errors;
                ViewBag.Id = user.Id;
                ViewBag.Worked = false;
            }
            return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

        }

        /// <summary>
        /// deactivate child
        /// </summary>
        /// <param name="id"></param>
        /// <param name="childrenPage"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult childDeactivate(int id, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var ChildDoB = (Request.Form["ChildDoB"]);
            var ChildSponsor = Request.Form["ChildSponsor"];

            int pageNumber = (childrenPage ?? 1);

            if (id == 0)
            {
                ViewBag.Error = "Ingen id";
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                ViewBag.Error = "Ingen bruker funnet";
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }
            child.isActive = false;
            child.InactiveSince = DateTime.Now;
            try
            {
            _context.Entry(child).State = EntityState.Modified;
            _context.SaveChanges();

                ViewBag.Success = "Barnet " + child.Fname +" "+child.Lname+ " har blitt deaktivert.";
                ViewBag.Id = child.Id;
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }
            catch (EntityException ex)
            {

                ViewBag.Error = "Noe gikk galt "+ex.Message;
                ViewBag.Id = child.Id;
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }
            
        }

        /***************************************/

        /************************activate functions**********************/

            /// <summary>
            /// activate employee
            /// </summary>
            /// <param name="id">id to activate</param>
            /// <param name="employeePage"></param>
            /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> employeeActivate(string id, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var employeePhone = (Request.Form["employeePhone"]);
            var EmployeeStreetadress = Request.Form["EmployeeStreetadress"];
            var EmployeeZip = (Request.Form["EmployeeZip"]);
            var EmployeeCity = Request.Form["EmployeeCity"];
            var EmployeeCounty = Request.Form["EmployeeCounty"];
            var EmployeePosition = Request.Form["EmployeePosition"];
            var employeeAccountNumber = (Request.Form["employeeAccountNumber"]);

            int pageNumber = (employeePage ?? 1);
            if (id == null)
            {
                ViewBag.Error = "Id ikke funnet";
                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null) { 

                ViewBag.Error = "Bruker ble ikke funnet";
            return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);
            ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            if (currentUser.RoleNr >= user.RoleNr)  //lower or same in hiearchy. not allowed to perform this task
            {
                ViewBag.Error = "Kan ikke aktivere en bruker på samme nivå eller lavere enn deg";
                ViewBag.Id = user.Id;

                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            user.LockoutEnabled = false;    //no longer locked out
            user.IsEnabeled = true; //user is enabeled
            user.InactiveSince = null;  //no longer inactive

            var result = await manager.UpdateAsync(user);
            _context.SaveChanges();

            if (result.Succeeded)
            {
                ViewBag.Success = "Brukeren " + user.Email + " har blitt aktivert.";
                ViewBag.Id = user.Id;
            }
            else
            {

                ViewBag.Error = "Noe gikk galt - " + result.Errors;
                ViewBag.Id = user.Id;
            }
            return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

        }
        /// <summary>
        /// activate sponsor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> sponsorActivate(string id, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var SponsorPhone = (Request.Form["SponsorPhone"]);
            var SponsorStreetadress = Request.Form["SponsorStreetadress"];
            var SponsorZip = (Request.Form["SponsorZip"]);
            var SponsorCity = Request.Form["SponsorCity"];
            var SponsorCounty = Request.Form["SponsorCounty"];

            int pageNumber = (sponsorPage ?? 1);
            if (id == null)
            {
                ViewBag.Error = "Id ikke funnet";
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                ViewBag.Error = "Bruker ikke funnet";
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);

            user.LockoutEnabled = false;
            user.IsEnabeled = true;
            user.InactiveSince = null;

            var result = await manager.UpdateAsync(user);
            _context.SaveChanges();

            if (result.Succeeded)
            {
                ViewBag.Success = "Brukeren " + user.Email + " har blitt aktivert.";
                ViewBag.Id = user.Id;
            }
            else
            {

                ViewBag.Error = "Noe gikk galt - " + result.Errors;
                ViewBag.Id = user.Id;
            }

            return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);
        }
        /// <summary>
        /// activate child entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="childrenPage"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> childActivate(int id, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var ChildDoB = (Request.Form["ChildDoB"]);
            var ChildSponsor = Request.Form["ChildSponsor"];

            int pageNumber = (childrenPage ?? 1);

            if (id == 0)
            {
                ViewBag.Error = "Ingen id";
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                ViewBag.Error = "Ingen barn funnet";
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }
            child.isActive = true;
            child.InactiveSince = null;
            try
            {
                _context.Entry(child).State = EntityState.Modified;
                _context.SaveChanges();

                ViewBag.Success = "Barnet " + child.Fname + " " + child.Lname + " har blitt aktivert.";
                ViewBag.Id = child.Id;
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);

            }
            catch (EntityException ex)
            {

                ViewBag.Error = "Noe gikk galt " + ex.Message;
                ViewBag.Id = child.Id;
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);

            }
        }
        
        /// <summary>
        /// edit employee
        /// </summary>
        /// <param name="model">model with profile change info</param>
        /// <param name="employeePage">employee page</param>
        /// <returns>updated employee list partial view</returns>
        [HttpPost]
        public async Task<ActionResult> EditEmployee(changeUserInfo model, int? employeePage)
        {
            //filter values
            ViewBag.employeePage = (employeePage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var employeePhone = (Request.Form["employeePhone"]);
            var EmployeeStreetadress = Request.Form["EmployeeStreetadress"];
            var EmployeeZip = (Request.Form["EmployeeZip"]);
            var EmployeeCity = Request.Form["EmployeeCity"];
            var EmployeeCounty = Request.Form["EmployeeCounty"];
            var EmployeePosition = Request.Form["EmployeePosition"];
            var employeeAccountNumber = (Request.Form["employeeAccountNumber"]);

            int pageNumber = (employeePage ?? 1);

            if (ModelState.IsValid)
            {
                

                var store = new UserStore<ApplicationUser>(_context);
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = manager.FindById(model.Id); //the user to be edited
                ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;  //the current user

                if (user.Id == null)
                {
                    return HttpNotFound();
                }
                if ((currentUser.RoleNr >= user.RoleNr)) //rolenumber too low
                {
                    if (currentUser.Id != user.Id)
                    {
                        ViewBag.Id = user.Id;
                        ViewBag.Error =
                            "Kan ikke endre informasjonen til en bruker som er på samme brukernivå eller høyere";

                        return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress,
                            EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber,
                            pageNumber);
                    }

                }
                if (model.RoleNr != null)
                { //there is a role number sent in

                    if (currentUser.Id == user.Id && user.RoleNr != model.RoleNr)  //the user wants to change their own role, not allowed
                    {
                        ViewBag.Id = user.Id;
                        ViewBag.Error = "Du kan ikke endre din egen rolle";

                        return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);
                    }

                    var rolesOfUser = await manager.GetRolesAsync(model.Id);    //the user can change the role

                    if (rolesOfUser.Any())
                    {
                        foreach (var item in rolesOfUser.ToList())
                        {
                            await manager.RemoveFromRoleAsync(model.Id, item);
                        }
                    }
                    user.RoleNr = model.RoleNr.Value;
                    manager.AddToRole(model.Id, ResolveUserRole(model.RoleNr));

                }
                var results = (from s in _context.Users
                              where
                                  s.Email.Contains(model.Email)
                              select s).ToList();

                if (results.Any()) {    //check if email exists
                foreach (var appUser in results)
                {
                    if (appUser.Id != model.Id) //if the one using the email is not the user to edit we cannot edit
                    {
                        ViewBag.Id = user.Id;
                        ViewBag.Error = "Emailen er allerede i bruk";

                            return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);
                        }
                }
                }
                //so far so good, change the details of the user
                user.Fname = model.Fname;
                user.Lname = model.Lname;
                user.Phone = model.Phone;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.Employee.AccountNumber = model.AccountNumber;
                user.Employee.Position = model.Position;

                var newAdress = new DbTables.Adresses
                {
                    City = model.City,
                    StreetAdress = model.StreetAdress,
                    County = model.State,
                    PostCode = model.PostCode
                };
                var adress = AdressExist(newAdress);

                if (adress == null)
                {
                    user.Adress = newAdress;
                    _context.Adresses.Add(newAdress);
                    _context.SaveChanges();

                }
                else if (user.Adress == adress)
                {
                    //do nothing
                }
                else
                {
                    user.Adress = adress;
                }

                var result = await manager.UpdateAsync(user); //update the user in the databse
                _context.SaveChanges();

                if (result.Succeeded) //if update succeeds
                {

                    if (model.Password != null)
                    {
                        var provider = new DpapiDataProtectionProvider("ButterflyFriends");
                        manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("Passwordresetting"));
                        string resetToken = await manager.GeneratePasswordResetTokenAsync(model.Id);
                        var Presult = await manager.ResetPasswordAsync(model.Id, resetToken, model.Password);   //reset passwor with token
                        if (!Presult.Succeeded)
                        {
                            ViewBag.Error = "Passordet må matche kriteriene";
                            ViewBag.Id = user.Id;

                            return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);
                        }
                    }

                    ViewBag.Success = "Brukeren "+ user.Email+" ble oppdatert.";
                    ViewBag.Id = user.Id;

                    return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

                }
               

                ViewBag.Id = user.Id;
                    ViewBag.Error = "Noe gikk galt "+result.Errors;
                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            var errormsg = "";
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errormsg +=error.ErrorMessage +" ";
                }
            }
            ViewBag.Id = model.Id;
            ViewBag.Error = "Noe gikk galt: "+errormsg;
            return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);



        }

        /// <summary>
        /// edit sponsors, same as employee
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditSponsor(changeUserInfo model, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var SponsorPhone = (Request.Form["SponsorPhone"]);
            var SponsorStreetadress = Request.Form["SponsorStreetadress"];
            var SponsorZip = (Request.Form["SponsorZip"]);
            var SponsorCity = Request.Form["SponsorCity"];
            var SponsorCounty = Request.Form["SponsorCounty"];

            int pageNumber = (sponsorPage ?? 1);
            if (ModelState.IsValid)
            {


                var store = new UserStore<ApplicationUser>(_context);
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = manager.FindById(model.Id); //the user to be edited
                if (user == null)
                {
                    return HttpNotFound();
                }

                var results = (from s in _context.Users
                               where
                                   s.Email.Contains(model.Email)
                               select s).ToList();

                if (results.Any())
                {
                    foreach (var appUser in results)
                    {
                        if (appUser.Id != model.Id)
                        {
                            ViewBag.Id = user.Id;
                            ViewBag.Error = "Emailen er allerede i bruk";
                            return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

                        }
                    }
                }
                //so far so good, change the details of the user
                user.Fname = model.Fname;
                user.Lname = model.Lname;
                user.Phone = model.Phone;
                user.Email = model.Email;
                user.UserName = model.Email;

                var newAdress = new DbTables.Adresses
                {
                    City = model.City,
                    StreetAdress = model.StreetAdress,
                    County = model.State,
                    PostCode = model.PostCode
                };
                var adress = AdressExist(newAdress);

                if (adress == null)
                {
                    user.Adress = newAdress;
                    _context.Adresses.Add(newAdress);
                    _context.SaveChanges();

                }
                else if (user.Adress == adress)
                {
                    //do nothing
                }
                else
                {
                    user.Adress = adress;
                }

                var result = await manager.UpdateAsync(user); //update the user in the databse
                _context.SaveChanges();

                if (result.Succeeded) //if update succeeds
                {
                    if (model.Password != null)
                    {
                        var provider = new DpapiDataProtectionProvider("ButterflyFriends");
                        manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("Passwordresetting"));
                        string resetToken = await manager.GeneratePasswordResetTokenAsync(model.Id);
                        var Presult = await manager.ResetPasswordAsync(model.Id, resetToken, model.Password);
                        if (!Presult.Succeeded)
                        {
                            ViewBag.Error = "Passordet må matche kriteriene";
                            ViewBag.Id = user.Id;
                            return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);
                        }
                    }

                    ViewBag.Success = "Brukeren " + user.Email + " ble oppdatert.";
                    ViewBag.Id = user.Id;
                    return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

                }


                ViewBag.Id = user.Id;
                ViewBag.Error = "Noe gikk galt " + result.Errors;
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);
            }

            var errormsg = "";
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errormsg += error.ErrorMessage + " ";
                }
            }

            ViewBag.Id = model.Id;
            ViewBag.Error = "Noe gikk galt: "+errormsg;
            return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);



        }

        /// <summary>
        /// creates and employee
        /// </summary>
        /// <param name="model">values to edit</param>
        /// <param name="employeePage">employee page</param>
        /// <returns>returns updated partial view</returns>
        [HttpPost]
        public async Task<ActionResult> CreateEmployee(RegisterViewModel model, int? employeePage)
        {
            //filter values
            ViewBag.employeePage = (employeePage ?? 1);

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var employeePhone = (Request.Form["employeePhone"]);
            var EmployeeStreetadress = Request.Form["EmployeeStreetadress"];
            var EmployeeZip = (Request.Form["EmployeeZip"]);
            var EmployeeCity = Request.Form["EmployeeCity"];
            var EmployeeCounty = Request.Form["EmployeeCounty"];
            var EmployeePosition = Request.Form["EmployeePosition"];
            var employeeAccountNumber = (Request.Form["employeeAccountNumber"]);

            int pageNumber = (employeePage ?? 1);

            if (ModelState.IsValid)
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
                ApplicationUser currentUser = userManager.FindByIdAsync(User.Identity.GetUserId()).Result;  //the current user
                if (currentUser.RoleNr >= model.RoleNr) //cannot create user with same or higher user level
                {
                    ViewBag.Error = "Du kan ikke legge til en bruker på samme eller høyere brukernivå";

                    return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

                }
                var results = (from s in _context.Users
                              where
                                  s.Email.Contains(model.Email)
                              select s).ToList();
                
                if (results.Any())  //email is already in use, return errormessage
                {

                            
                            ViewBag.Error = "Emailen er allerede i bruk.";

                    return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);



                }
                //so far so good, create user object
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Fname = model.Fname,
                    Lname = model.Lname,
                    Phone = model.Phone,
                    IsEnabeled = true,
                    RoleNr = model.RoleNr,
                    Employee = new DbTables.Employees {AccountNumber = model.AccountNumber,Position = model.Position}
                };

                var Adress = new DbTables.Adresses
                {
                    StreetAdress = model.StreetAdress,
                    City = model.City,
                    County = model.State,
                    PostCode = model.PostCode
                };
                
                var UserAdress = AdressExist(Adress);   //check if adress already exist in database
                if (UserAdress == null)
                {
                    UserAdress = Adress;
                    try
                    {
                        _context.Adresses.Add(UserAdress);
                        _context.SaveChanges();
                    }
                    catch (EntityException ex)
                    {
                        ViewBag.Error = "Noe gikk galt " + ex.Message;
                        return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);


                    }

                }
                user.Adress = UserAdress;

                var result = await userManager.CreateAsync(user, model.Password);   //create user

                if (result.Succeeded)   //creatinon succseeds
                {

                    userManager.AddToRole(user.Id, ResolveUserRole(model.RoleNr));  //add to appropriate role
                    ViewBag.Success = "Brukeren "+user.Email+" ble lagt til i databasen";
                    ViewBag.Id = user.Id;   //return updated view
                    return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

                }
                ViewBag.Error = "Noe gikk galt "+ result.Errors;
                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            var errormsg = "";
            foreach (ModelState modelState in ViewData.ModelState.Values)   //modelstate errors. get the errors and return to view
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errormsg += error.ErrorMessage + " ";
                }
            }
            ViewBag.Error = "Noe gikk galt: "+errormsg;
            return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

        }

        /// <summary>
        /// create sponsor, same procedure as employee creation
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateSponsor(RegisterViewModel model, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var SponsorPhone = (Request.Form["SponsorPhone"]);
            var SponsorStreetadress = Request.Form["SponsorStreetadress"];
            var SponsorZip = (Request.Form["SponsorZip"]);
            var SponsorCity = Request.Form["SponsorCity"];
            var SponsorCounty = Request.Form["SponsorCounty"];

            int pageNumber = (sponsorPage ?? 1);

            if (ModelState.IsValid)
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
                var results = (from s in _context.Users
                               where
                                   s.Email.Contains(model.Email)
                               select s).ToList();

                if (results.Any())
                {

                            ViewBag.Error = "Emailen er allerede i bruk.";

                    return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);


                }
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Fname = model.Fname,
                    Lname = model.Lname,
                    Phone = model.Phone,
                    IsEnabeled = true,
                    RoleNr = 3
                };
                var Adress = new DbTables.Adresses
                {
                    StreetAdress = model.StreetAdress,
                    City = model.City,
                    County = model.State,
                    PostCode = model.PostCode
                };

                var UserAdress = AdressExist(Adress);
                if (UserAdress == null)
                {
                    UserAdress = Adress;
                    try
                    {
                        _context.Adresses.Add(UserAdress);
                        _context.SaveChanges();
                    }
                    catch (EntityException ex)
                    {

                        ViewBag.Error = "Noe gikk galt " + ex.Message;
                        return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

                    }

                }
                user.Adress = UserAdress;

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {

                    userManager.AddToRole(user.Id, ResolveUserRole(user.RoleNr));
                    ViewBag.Success = "Brukeren " + user.Email + " ble lagt til i databasen";
                    ViewBag.Id = user.Id;
                    return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

                }
                ViewBag.Error = "Noe gikk galt " + result.Errors;
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

            }

            var errormsg = "";
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errormsg += error.ErrorMessage + " ";
                }
            }

            ViewBag.Error = "Noe gikk galt: "+errormsg;
            return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);
        }

        /// <summary>
        /// create child
        /// </summary>
        /// <param name="model"></param>
        /// <param name="childrenPage"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateChild(ChildCreateModel model, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var ChildDoB = (Request.Form["ChildDoB"]);
            var ChildSponsor = Request.Form["ChildSponsor"];

            int pageNumber = (childrenPage ?? 1);

            if (ModelState.IsValid) //check if model values are valid
            {
                var sponsormsg = "";
                if (model.Child.SponsorId != null) { 
                var user = _context.Users.Find(model.Child.SponsorId);
                if (user != null)
                {
                    if (user.Fname +" "+user.Lname != model.SponsorName)    
                    {
                        model.Child.SponsorId = null;
                            sponsormsg = ", men fant ikke bruker " + model.SponsorName; //could not find sponsor
                        }
                }
                }
                else if (model.Child.SponsorId == null && model.SponsorName != null)
                {
                    sponsormsg = ", men fant ikke bruker " + model.SponsorName;
                }
                model.Child.isActive = true;
                if (model.Child.DoB > DateTime.Now) 
                {
                    ViewBag.Error = "Barnet kan ikke være født i fremtiden!";   //date of birth cannot be in the future

                    return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
                }
                try
                {
                    _context.Children.Add(model.Child);
                    _context.SaveChanges();

                    ViewBag.Success = "Fadderbarnet " + model.Child.Fname + " " + model.Child.Lname + " ble lagt til i databasen"+sponsormsg;
                    ViewBag.Id = model.Child.Id;
                    return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
                }
                catch (EntityException ex)
                {
                    ViewBag.Error = "Error: "+ex.Message;

                    return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
                }
            }

            var errormsg = "";
            foreach (ModelState modelState in ViewData.ModelState.Values)   //add modelstate errors, return them to view
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errormsg += error.ErrorMessage + " ";
                }
            }

            ViewBag.Error = "Noe gikk galt: " + errormsg;

            return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
        }

        /// <summary>
        /// shows employee delete partial modal
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employeePage"></param>
        /// <returns></returns>
        public ActionResult showEmployeeDelete(string id, int? employeePage)
        {
            ViewBag.employeePage = (employeePage ?? 1);

            if (id == null)
            {
                var error = from s in _context.Users
                            where s.Employee != null
                            orderby s.Lname
                            select s;
                ViewBag.Error = "Ingen id funnet";
                return PartialView("ListPartials/_EmployeePartial",
                           new EmployeeModel { Employees = error.ToPagedList((employeePage ?? 1), pageSize) });
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                var error = from s in _context.Users
                            where s.Employee != null
                            orderby s.Lname
                            select s;
                ViewBag.Error = "Fant ikke bruker";
                return PartialView("ListPartials/_EmployeePartial",
                           new EmployeeModel { Employees = error.ToPagedList((employeePage ?? 1), pageSize) });
            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);
            ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;

            return PartialView("DeletePartials/_DeleteEmployeePartial", user);
        }

        /// <summary>
        /// delete employee entity
        /// </summary>
        /// <param name="id">id of employee to delete</param>
        /// <param name="employeePage">page og employee list</param>
        /// <returns>filtered list result</returns>
        public ActionResult EmployeeDelete(string id, int? employeePage)
        {
            //filter parameters
            ViewBag.employeePage = (employeePage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var employeePhone = (Request.Form["employeePhone"]);
            var EmployeeStreetadress = Request.Form["EmployeeStreetadress"];
            var EmployeeZip = (Request.Form["EmployeeZip"]);
            var EmployeeCity = Request.Form["EmployeeCity"];
            var EmployeeCounty = Request.Form["EmployeeCounty"];
            var EmployeePosition = Request.Form["EmployeePosition"];
            var employeeAccountNumber = (Request.Form["employeeAccountNumber"]);

            int pageNumber = (employeePage ?? 1);
            if (id == null)
            {
                
                ViewBag.Error = "Ingen id funnet";
                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
               
                ViewBag.Error = "Bruker ble ikke funnet";
                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);
            ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            if (currentUser.RoleNr >= user.RoleNr)
            {
                
                ViewBag.Error = "Du kan ikke slette noen som er på et høyere eller samme brukernivå";   //user level too low
                ViewBag.id = id;
                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }
            try
            {
                DbTables.File profile = new DbTables.File();    
                DbTables.ThumbNail thumbnail = new DbTables.ThumbNail();
                var email = user.Email;


                var images = (from s in _context.Files      //check if the user has profile image. if so delete this too
                    where s.FileType == DbTables.FileType.Profile
                    select s).ToList();

                foreach (var image in images)
                {
                    if (image.User.First().Id == user.Id)
                    {
                        profile = image;
                        thumbnail = image.ThumbNail;
                    }
                }

                if (profile.Content != null) { 

                _context.Entry(profile).State = EntityState.Deleted;
                _context.Entry(thumbnail).State = EntityState.Deleted;
                }

                _context.Entry(user.Employee).State = EntityState.Deleted;  //delete related employee entity
                _context.Entry(user).State = EntityState.Deleted;

                _context.TagBoxs.RemoveRange(_context.TagBoxs.Where(x => x.Id == user.Id)); //remove tagboxes of user from database

                _context.SaveChanges();

                

                ViewBag.Success = "Brukeren "+email+" ble suksessfult slettet";

                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);


            }
            catch (EntityException ex)
            {
                
                ViewBag.Error = "Error: "+ex.Message;
                ViewBag.id = id;
                return FilterResultEmployees(search, active, order, filter, employeePhone, EmployeeStreetadress, EmployeeZip, EmployeeCity, EmployeeCounty, EmployeePosition, employeeAccountNumber, pageNumber);

            }


        }

        /// <summary>
        /// show sponsor delete
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        public ActionResult showSponsorDelete(string id, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);
            if (id == null)
            {
                var error = from s in _context.Users
                                where s.Employee == null
                                orderby s.Lname
                                select s;
                ViewBag.Error = "Ingen id funnet";
                return PartialView("ListPartials/_SponsorPartial",
                           new SponsorModel { Sponsors = error.ToPagedList((sponsorPage ?? 1), pageSize) });
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                var error = from s in _context.Users
                                where s.Employee == null
                                orderby s.Lname
                                select s;
                ViewBag.Error = "Bruker ble ikke funnet";
                return PartialView("ListPartials/_SponsorPartial",
                           new SponsorModel { Sponsors = error.ToPagedList((sponsorPage ?? 1), pageSize) });
            }

            return PartialView("DeletePartials/_DeleteSponsorPartial", user);
        }

        /// <summary>
        /// delete sponsor. same proceduare as with employees
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sponsorPage"></param>
        /// <returns></returns>
        public ActionResult SponsorDelete(string id, int? sponsorPage)
        {
            ViewBag.sponsorPage = (sponsorPage ?? 1);

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var SponsorPhone = (Request.Form["SponsorPhone"]);
            var SponsorStreetadress = Request.Form["SponsorStreetadress"];
            var SponsorZip = (Request.Form["SponsorZip"]);
            var SponsorCity = Request.Form["SponsorCity"];
            var SponsorCounty = Request.Form["SponsorCounty"];

            int pageNumber = (sponsorPage ?? 1);

            if (id == null)
            {

                ViewBag.Error = "Ingen id funnet";
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {

                ViewBag.Error = "Fant ikke bruker";
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);

            }
            var store = new UserStore<ApplicationUser>(_context);
            var manager = new UserManager<ApplicationUser>(store);
            ApplicationUser currentUser = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            if (currentUser.RoleNr >= user.RoleNr)
            {

                ViewBag.Error = "Du kan ikke slette noen som er på et høyere eller samme brukernivå";
                ViewBag.id = id;
                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);
            }
            try
            {
                DbTables.File profile = new DbTables.File();
                DbTables.ThumbNail thumbnail = new DbTables.ThumbNail();
                var email = user.Email;


                var images = (from s in _context.Files
                              where s.FileType == DbTables.FileType.Profile
                              select s).ToList();

                foreach (var image in images)
                {
                    if (image.User.First().Id == user.Id)
                    {
                        profile = image;
                        thumbnail = image.ThumbNail;
                    }
                }

                if (profile.Content != null)
                {
                    _context.Entry(profile).State = EntityState.Deleted;
                    _context.Entry(thumbnail).State = EntityState.Deleted;
                }
                _context.Entry(user).State = EntityState.Deleted;

                _context.TagBoxs.RemoveRange(_context.TagBoxs.Where(x => x.Id == user.Id));

                _context.SaveChanges();

                ViewBag.Success = "Brukeren " + email + " ble suksessfult slettet";

                return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);


            }
            catch (EntityException ex)
            {
                var error = from s in _context.Users
                            where s.Employee == null
                            orderby s.Lname
                            select s;
                ViewBag.Error = "Error: " + ex.Message;
                ViewBag.id = id;
                return PartialView("ListPartials/_SponsorPartial",
                          new SponsorModel { Sponsors = error.ToPagedList((sponsorPage ?? 1), pageSize) });
            }


        }

        /// <summary>
        /// show child delete modal
        /// </summary>
        /// <param name="id"></param>
        /// <param name="childrenPage"></param>
        /// <returns></returns>
        public ActionResult showChildDelete(int id, int? childrenPage)
        {
            ViewBag.childrenPage = (childrenPage ?? 1);
            if (id == 0)
            {
                var error = from s in _context.Children
                            orderby s.Lname
                            select s;
                ViewBag.Error = "Ingen id funnet";
                return PartialView("ListPartials/_ChildrenPartial",
                           new ChildrenModel { Children = error.ToPagedList((childrenPage ?? 1), pageSize) });
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                var error = from s in _context.Children
                            orderby s.Lname
                            select s;
                ViewBag.Error = "Barnet ble ikke funnet";
                return PartialView("ListPartials/_ChildrenPartial",
                           new ChildrenModel { Children = error.ToPagedList((childrenPage ?? 1), pageSize) });
            }

            return PartialView("DeletePartials/_DeleteChildPartial", child);
        }

        /// <summary>
        /// delete child, same procedure as with employees and sponsors
        /// </summary>
        /// <param name="id"></param>
        /// <param name="childrenPage"></param>
        /// <returns></returns>
        public ActionResult ChildDelete(int id, int? childrenPage)
        {
            //filter values
            ViewBag.childrenPage = (childrenPage ?? 1);
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var ChildDoB = (Request.Form["ChildDoB"]);
            var ChildSponsor = Request.Form["ChildSponsor"];

            int pageNumber = (childrenPage ?? 1);

            if (id == 0)
            {
                ViewBag.Error = "Ingen id funnet";
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }
            DbTables.Child child = _context.Children.Find(id);
            if (child == null)
            {
                ViewBag.Error = "Barnet ble ikke funnet";
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }

            try
            {
                DbTables.File profile = new DbTables.File();
                DbTables.ThumbNail thumbnail = new DbTables.ThumbNail();
                var name = child.Fname+" "+child.Lname;


                var images = (from s in _context.Files
                              where s.FileType == DbTables.FileType.Profile
                              select s).ToList();

                foreach (var image in images)
                {
                    if (image.Children.First().Id == child.Id)
                    {
                        profile = image;
                        thumbnail = image.ThumbNail;
                    }
                }

                if (profile.Content != null)
                {
                    _context.Entry(profile).State = EntityState.Deleted;
                    _context.Entry(thumbnail).State = EntityState.Deleted;
                }
                _context.TagBoxs.RemoveRange(_context.TagBoxs.Where(x => x.Id == child.Id.ToString()));

                _context.Entry(child).State = EntityState.Deleted;


                _context.SaveChanges();


                ViewBag.Success = "Brukeren " + name + " ble suksessfult slettet";

                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);

            }
            catch (EntityException ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                ViewBag.id = id;
                return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);
            }
        }
        /// <summary>
        /// filter employees
        /// </summary>
        /// <returns>filter list resulst</returns>
        public ActionResult FilterEmployees()
        {
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var employeePhone = (Request.Form["employeePhone"]);
            var EmployeeStreetadress = Request.Form["EmployeeStreetadress"];
            var EmployeeZip = (Request.Form["EmployeeZip"]);
            var EmployeeCity = Request.Form["EmployeeCity"];
            var EmployeeCounty = Request.Form["EmployeeCounty"];
            var EmployeePosition = Request.Form["EmployeePosition"];
            var employeeAccountNumber = (Request.Form["employeeAccountNumber"]);

            int pageNumber = 1;
            ViewBag.employeePage = 1;


            return FilterResultEmployees(search,active,order,filter,employeePhone,EmployeeStreetadress,EmployeeZip,EmployeeCity,EmployeeCounty,EmployeePosition,employeeAccountNumber,pageNumber);
        }
        /// <summary>
        /// filter sponsors
        /// </summary>
        /// <returns>filter list resulst</returns>
        public ActionResult FilterSponsors()
        {
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var SponsorPhone = (Request.Form["SponsorPhone"]);
            var SponsorStreetadress = Request.Form["SponsorStreetadress"];
            var SponsorZip = (Request.Form["SponsorZip"]);
            var SponsorCity = Request.Form["SponsorCity"];
            var SponsorCounty = Request.Form["SponsorCounty"];

            int pageNumber = 1;
            ViewBag.sponsorPage = 1;


            return FilterResultSponsors(search, active, order, filter, SponsorPhone, SponsorStreetadress, SponsorZip, SponsorCity, SponsorCounty, pageNumber);
        }
        /// <summary>
        /// filter children
        /// </summary>
        /// <returns>filter list resulst</returns>
        public ActionResult FilterChildren()
        {
            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var ChildDoB = (Request.Form["ChildDoB"]);
            var ChildSponsor = Request.Form["ChildSponsor"];


            int pageNumber = 1;
            ViewBag.childPage = 1;


            return FilterResultChildren(search, active, order, filter, ChildDoB, ChildSponsor,pageNumber);
        }
        /// <summary>
        /// filter result based on parameters
        /// </summary>
        /// <param name="search"></param>
        /// <param name="active"></param>
        /// <param name="order"></param>
        /// <param name="filter"></param>
        /// <param name="dob"></param>
        /// <param name="sponsor"></param>
        /// <param name="pageNumber"></param>
        /// <returns>returns a partial view list with entities corresponding to search criteria</returns>
        public PartialViewResult FilterResultChildren(string search, string active, string order, string filter, string dob, string sponsor,int pageNumber)
        {
            DateTime? DoB = new DateTime();
            if (!string.IsNullOrEmpty(dob))
            {
                DoB = DateTime.Parse(dob);
            }
            else
            {
                DoB = null;
            }
            var children = from s in _context.Children
                            where 
                            (s.Fname +" "+ s.Lname).Contains(search) &&
                            (s.User.Fname+" "+s.User.Lname).Contains(sponsor)
                            select s;
            if (DoB.HasValue)
            {
                children = children.Where(s => s.DoB.Equals(DoB.Value));
            }
            if (active == "yes")
            {
                children = children.Where(s => s.isActive);

            }
            else if (active == "no")
            {
                children = children.Where(s => !s.isActive);

            }

            if (order == "descending")
            {
                switch (filter)
                {
                    case "1":
                        children =
                            children.OrderByDescending(s => s.Fname);
                        break;
                    default:
                        children =
                            children.OrderByDescending(s => s.Lname);
                        break;
                }
            }
            else
            {
                switch (filter)
                {
                    case "1":
                        children =
                            children.OrderBy(s => s.Fname);
                        break;
                    default:
                        children =
                            children.OrderBy(s => s.Lname);
                        break;
                }
            }
            if (pageNumber > 1 && children.Any())
            {
                if (children.Count() <= pageSize * (pageNumber))    //chech that page is not empty. if so show highest available page
                {

                    pageNumber = (int)Math.Ceiling((double)children.Count() / (double)pageSize);
                    ViewBag.employeePage = pageNumber;
                    return PartialView("ListPartials/_ChildrenPartial",
                          new ChildrenModel { Children = children.ToPagedList(pageNumber, pageSize) });
                }
            }
            else if (!children.Any())   //no elements to show. show page 1.
            {
                ViewBag.employeePage = 1;
                return PartialView("ListPartials/_ChildrenPartial",
                           new ChildrenModel { Children = children.ToPagedList(pageNumber, pageSize) });
            }
            return PartialView("ListPartials/_ChildrenPartial",
                          new ChildrenModel { Children = children.ToPagedList(pageNumber, pageSize) });
        }

        /// <summary>
        /// filter result for employees
        /// </summary>
        /// <param name="search"></param>
        /// <param name="active"></param>
        /// <param name="order"></param>
        /// <param name="filter"></param>
        /// <param name="phone"></param>
        /// <param name="streetadress"></param>
        /// <param name="zipcode"></param>
        /// <param name="city"></param>
        /// <param name="county"></param>
        /// <param name="position"></param>
        /// <param name="accountnumber"></param>
        /// <param name="pageNumber"></param>
        /// <returns>returns a partial view result list with filtered employees</returns>
        public PartialViewResult FilterResultEmployees(string search, string active, string order, string filter, string phone, string streetadress, string zipcode, string city, string county, string position, string accountnumber, int pageNumber)
        {
            var employees = from s in _context.Users
                            where s.Employee != null &&
                            ((s.Fname + s.Lname).Contains(search) ||
                            s.Email.Contains(search)) &&
                            s.Phone.Contains(phone) &&
                            s.Employee.Position.Contains(position) &&
                            s.Employee.AccountNumber.ToString().Contains(accountnumber) &&
                            s.Adress.City.Contains(city) &&
                            s.Adress.County.Contains(county) &&
                            s.Adress.PostCode.ToString().Contains(zipcode) &&
                            s.Adress.StreetAdress.Contains(streetadress)
                            select s;
            if (active == "yes")
            {
                employees = employees.Where(s => s.IsEnabeled);

            }
            else if (active == "no")
            {
                employees = employees.Where(s => !s.IsEnabeled);

            }

            if (order == "descending")
            {
                switch (filter)
                {
                    case "1":
                        employees =
                            employees.OrderByDescending(s => s.Fname);
                        break;
                    case "2":
                        employees = employees.OrderByDescending(s => s.Email);
                        break;
                    default:
                        employees =
                            employees.OrderByDescending(s => s.Lname);
                        break;
                }
            }
            else
            {
                switch (filter)
                {
                    case "1":
                        employees =
                            employees.OrderBy(s => s.Fname);
                        break;
                    case "2":
                        employees = employees.OrderBy(s => s.Email);
                        break;
                    default:
                        employees =
                            employees.OrderBy(s => s.Lname);
                        break;
                }
            }
            if (pageNumber > 1 && employees.Any())
            {
                if (employees.Count() <= pageSize * (pageNumber))
                {

                    pageNumber = (int)Math.Ceiling((double)employees.Count() / (double)pageSize);
                    ViewBag.employeePage = pageNumber;
                    return PartialView("ListPartials/_EmployeePartial",
                          new EmployeeModel { Employees = employees.ToPagedList(pageNumber, pageSize) });
                }
            }
            else if (!employees.Any())
            {
                ViewBag.employeePage = 1;
                return PartialView("ListPartials/_EmployeePartial",
                      new EmployeeModel { Employees = employees.ToPagedList(1, pageSize) });
            }
            return PartialView("ListPartials/_EmployeePartial",
                                          new EmployeeModel { Employees = employees.ToPagedList(pageNumber, pageSize) });
        }

        /// <summary>
        /// filter result for sponsors
        /// </summary>
        /// <param name="search"></param>
        /// <param name="active"></param>
        /// <param name="order"></param>
        /// <param name="filter"></param>
        /// <param name="phone"></param>
        /// <param name="streetadress"></param>
        /// <param name="zipcode"></param>
        /// <param name="city"></param>
        /// <param name="county"></param>
        /// <param name="pageNumber"></param>
        /// <returns>returns filtered result based on parameters</returns>
        public PartialViewResult FilterResultSponsors(string search, string active, string order,string filter,string phone,string streetadress,string zipcode,string city,string county,int pageNumber)
        {
            var sponsors = from s in _context.Users
                            where s.Employee == null &&
                            ((s.Fname+s.Lname).Contains(search)||
                            s.Email.Contains(search)) &&
                            s.Phone.Contains(phone) && 
                            s.Adress.City.Contains(city) &&
                            s.Adress.County.Contains(county) &&
                            s.Adress.PostCode.ToString().Contains(zipcode) &&
                            s.Adress.StreetAdress.Contains(streetadress)
                        select s;
            if (active == "yes")
            {
                sponsors = sponsors.Where(s => s.IsEnabeled);

            }
            else if (active == "no")
            {
                sponsors = sponsors.Where(s => !s.IsEnabeled);

            }

            if (order == "descending")
            {
                switch (filter)
                {
                    case "1":
                        sponsors =
                            sponsors.OrderByDescending(s => s.Fname);
                        break;
                    case "2":
                        sponsors = sponsors.OrderByDescending(s => s.Email);
                        break;
                    default:
                        sponsors =
                            sponsors.OrderByDescending(s => s.Lname);
                        break;
                }
            }
            else
            {
                switch (filter)
                {
                    case "1":
                        sponsors =
                            sponsors.OrderBy(s => s.Fname);
                        break;
                    case "2":
                        sponsors = sponsors.OrderBy(s => s.Email);
                        break;
                    default:
                        sponsors =
                            sponsors.OrderBy(s => s.Lname);
                        break;
                }
            }
            if (pageNumber > 1 && sponsors.Any())   //make sure page is not empty
            {
                if (sponsors.Count() <= pageSize * (pageNumber))
                {
                    
                    pageNumber = (int)Math.Ceiling((double)sponsors.Count() / (double)pageSize);
                    ViewBag.sponsorPage = pageNumber;
                    return PartialView("ListPartials/_SponsorPartial",
                          new SponsorModel { Sponsors = sponsors.ToPagedList(pageNumber, pageSize) });
                }
            }else if (!sponsors.Any())
            {
                ViewBag.sponsorPage = 1;
                return PartialView("ListPartials/_SponsorPartial",
                          new SponsorModel { Sponsors = sponsors.ToPagedList(pageNumber, pageSize) });
            }
            return PartialView("ListPartials/_SponsorPartial",
                         new SponsorModel { Sponsors = sponsors.ToPagedList(pageNumber, pageSize) });
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

        /// <summary>
        /// get users from database
        /// </summary>
        /// <returns>returns list of users to add as sponsors</returns>
        [HttpPost]
        public ActionResult GetUsers()
        {
            var body = Request.InputStream;
            var encoding = Request.ContentEncoding;
            var reader = new StreamReader(body, encoding);
            var json = reader.ReadToEnd();


            var enteties = new List<object>();

            var users = (from s in _context.Users
                         where
                         s.Fname.StartsWith(json) ||
                         s.Lname.StartsWith(json)
                         orderby s.Lname
                         select s).ToList();

            foreach (var user in users)
            {
                var imgId = 1;  //default profile image
                if (user.Thumbnail != null)
                {
                    imgId = user.Thumbnail.ThumbNailId;
                }
                enteties.Add(new { Name = user.Fname + " " + user.Lname, Id = user.Id, imgId = imgId,email = user.Email });
            }
            return Json(enteties);
        }

        /// <summary>
        /// resolves role based on role number
        /// </summary>
        /// <param name="roleNr">role number in the hierarchy</param>
        /// <returns>returns identity role</returns>
        public string ResolveUserRole(int? roleNr)
        {
            if (roleNr == 3)
            {
                return "Fadder";
            }
             if (roleNr == 2)
            {
                return "Ansatt";
            }
             if (roleNr == 1)
            {
                return "Admin";
            }

                return "Eier";
        }
    }
}