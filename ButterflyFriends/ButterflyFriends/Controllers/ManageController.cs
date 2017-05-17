using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.DataProtection;

namespace ButterflyFriends.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
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
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ditt passord har blitt endred."
                : message == ManageMessageId.SetPasswordSuccess ? "Ditt passord har blitt sett."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "En feil oppstod."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var indexModel = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            var user =_context.Users.Find(userId);
            var ProfileImage = new DbTables.File();
            var pictures = user.Pictures.Where(s => s.FileType == DbTables.FileType.Profile);
            if (pictures.Any())
            {
                ProfileImage = pictures.First();
            }
            var profileModel = new changeProfileModel 
            {
                Fname = user.Fname,
                Lname = user.Lname,
                City = user.Adress.City,
                StreetAdress = user.Adress.StreetAdress,
                PostCode = user.Adress.PostCode,
                State = user.Adress.County,
                Phone = user.Phone,
                Id = user.Id,
                File = ProfileImage,
                BirthNumber = user.BirthNumber
            };
            var model = new ProfileViewModel
            {
                Index = indexModel,
                Profile = profileModel
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {

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
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

                if (user != null)
                {

                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
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
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
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
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "En feil oppstod."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        public ActionResult ShowUserEdit(string id)
        {

            if (id == null)                                             //Id is null, return bad request
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);              //get the requested user

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }
            var changeProfileModel = new changeProfileModel
            {
                Fname = user.Fname,
                Lname = user.Lname,
                City = user.Adress.City,
                StreetAdress = user.Adress.StreetAdress,
                PostCode = user.Adress.PostCode,
                State = user.Adress.County,
                Phone = user.Phone,
                Id = user.Id,
                BirthNumber = user.BirthNumber
            };

            return PartialView("_EditProfilePartial", changeProfileModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser(changeProfileModel model)
        {

            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                if (userId == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                }

                var store = new UserStore<ApplicationUser>(_context);
                var manager = new UserManager<ApplicationUser>(store);
                ApplicationUser user = manager.FindById(userId); //the current user



                //so far so good, change the details of the user
                user.Fname = model.Fname;
                user.Lname = model.Lname;
                user.Phone = model.Phone;
                user.BirthNumber = model.BirthNumber;

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
                    adress = newAdress;
                    _context.Adresses.Add(newAdress);
                    _context.SaveChanges();
                    
                }else if (user.Adress == adress)
                {
                    //do nothing
                }
                else
                {
                    user.Adress = adress;
                }

                IdentityResult result = await manager.UpdateAsync(user); //update the user in the databse
                store.Context.SaveChanges();

                if (result.Succeeded) //if update succeeds
                {

                    if (Request.IsAjaxRequest()) //it succeeds, show success status message
                    {
                        ViewBag.Success = "Profilinformasjon oppdatert.";
                        var ProfileModel = new changeProfileModel
                        {
                            Id = user.Id,
                            Fname = user.Fname,
                            Lname = user.Lname,
                            Phone = user.Phone,
                            City = adress.City,
                            State = adress.County,
                            StreetAdress = adress.StreetAdress,
                            PostCode = adress.PostCode,
                            BirthNumber = user.BirthNumber
                        };
                        return PartialView("_UserInfoPartial", ProfileModel);
                    }
                }
                else
                { 
                    var ProfileModel = new changeProfileModel
                    {
                        Id = user.Id,
                        Fname = user.Fname,
                        Lname = user.Lname,
                        Phone = user.Phone,
                        City = adress.City,
                        State = adress.County,
                        StreetAdress = adress.StreetAdress,
                        PostCode = adress.PostCode
                    };
                    ViewBag.Error = "Noe gikk galt.";
                    return PartialView("_UserInfoPartial", ProfileModel);
                }
            }
            else
            {
                ApplicationUser user = _context.Users.Find(User.Identity.GetUserId());
                var adress = _context.Adresses.Find(user.AdressId);
                var ProfileModel = new changeProfileModel
                {
                    Id = user.Id,
                    Fname = user.Fname,
                    Lname = user.Lname,
                    Phone = user.Phone,
                    City = adress.City,
                    State = adress.County,
                    StreetAdress = adress.StreetAdress,
                    PostCode = adress.PostCode
                };
                string messages = string.Join("\n", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));

                ViewBag.Error = "Ugyldige verdier: " + messages;

                return PartialView("_UserInfoPartial", ProfileModel);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        [Authorize(Roles = "Eier, Admin, Fadder, Ansatt")]
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
                    var ProfileImageWidth = 300;
                    var ThumbnailImageWidth = 40;
                    var type = Request.Form["type"];
                    //ApplicationUser user = _context.Users.Find(userId);
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
                                return Json(new { error = "Du har ikke lov til å endre dette profilbildet", success = "false" });

                            }

                        }

                        pictures = employee.Pictures;
                    }
                    else if (type == "child")
                    {
                        child = _context.Children.Find(Int32.Parse(userId));
                        pictures = child.Pictures;
                    }

                    //var pictures = user.Pictures;
                    var profPic = new DbTables.File();
                    var thumbNail = new DbTables.ThumbNail();
                    if (pictures.Any())
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
                            if (employee.Email != null)
                            {
                                picture = new DbTables.File
                                {
                                    FileName = Path.GetFileName(file.FileName),
                                    FileType = DbTables.FileType.Profile,
                                    ContentType = file.ContentType,
                                    User = new List<ApplicationUser> { employee }
                                };
                            }
                            else
                            {
                                picture = new DbTables.File
                                {
                                    FileName = Path.GetFileName(file.FileName),
                                    FileType = DbTables.FileType.Profile,
                                    ContentType = file.ContentType,
                                    Children = new List<DbTables.Child> { child }
                                };
                            }
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
                            double ratio = (double)((double)bmp.Width / (double)bmp.Height);
                            int height = (int)((double)ProfileImageWidth / ratio);
                            var newImg = ResizeImage(bmp, ProfileImageWidth, height);

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
                                //User = new List<ApplicationUser> {user}
                            };

                            int Theight = (int)((double)ThumbnailImageWidth / ratio);
                            var newThumb = ResizeImage(bmp, ThumbnailImageWidth, Theight);
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
                            //fileId = picture.FileId;
                        }
                        else
                        {
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
                            double ratio = (double)((double)bmp.Width / (double)bmp.Height);
                            int height = (int)((double)ProfileImageWidth / ratio);
                            var newImg = ResizeImage(bmp, ProfileImageWidth, height);

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

                            int Theight = (int)((double)ThumbnailImageWidth / ratio);
                            var newThumb = ResizeImage(bmp, ThumbnailImageWidth, Theight);
                            byte[] Tcntnt;
                            using (var stream = new MemoryStream())
                            {
                                newThumb.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                Tcntnt = stream.ToArray();
                            }
                            thumbNail.Content = Tcntnt;
                            _context.Entry(thumbNail).State = EntityState.Modified;

                            _context.SaveChanges();
                            //fileId = profPic.FileId;
                        }

                    }
                    var user = _context.Users.Find(userId);
                    var ProfileImage = new DbTables.File();
                    var Picture = user.Pictures.Where(s => s.FileType == DbTables.FileType.Profile);
                    if (Picture.Any())
                    {
                        ProfileImage = Picture.First();
                    }
                    // Returns message that successfully uploaded
                    return PartialView("_ProfileImagePartial", ProfileImage);
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

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}