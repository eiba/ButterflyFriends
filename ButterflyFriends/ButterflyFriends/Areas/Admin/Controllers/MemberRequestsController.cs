using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Security.DataProtection;
using PagedList;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    [Authorize(Roles = "Eier, Admin, Ansatt")]
    public class MemberRequestsController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        public int pageSize = 10;   //requests per page

        // GET: Admin/MemberRequests
        /// <summary>
        /// get index page
        /// </summary>
        /// <returns>view with requests based to default last name</returns>
        public ActionResult Index()
        {
            var requests = from s in _context.MembershipRequests
                orderby s.Lname
                select s;
            ViewBag.page = 1;
            return View(requests.ToPagedList(1, pageSize));
        }

        /// <summary>
        /// Accept request
        /// </summary>
        /// <returns>returns partial view with requests</returns>
        [HttpPost]
        public async Task<ActionResult> RequestAccept()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var id = int.Parse(Request.Form["requestid"]);
            var message = Request.Form["message"];
            var req = _context.MembershipRequests.Find(id); //get the request

            ViewBag.page = Request.Form["page"];
            var pageNumber = int.Parse(Request.Form["page"]);
            var requests = from s in _context.MembershipRequests
                orderby s.Lname
                select s;
            if (req == null)
            {
                ViewBag.Error = "Fant ikke forespørselen.";
                return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));
            }
            var email = req.Email;  

            var results = (from s in _context.Users
                where
                s.Email.Contains(email)
                select s).ToList();

            if (results.Any())
            {
                foreach (var r in results)
                {
                    if (r.Email == email)   //email of request is already being used.
                    {

                        ViewBag.Error = "Emailen er allerede i bruk.";
                        return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));


                    }
                }

            }

            var newUser = new ApplicationUser   //validation passed so far, create new user object
            {
                Email = email,
                UserName = email,
                Fname = req.Fname,
                Lname = req.Lname,
                Phone = req.Phone,
                RoleNr = 3,
                IsEnabeled = true,
                BirthNumber = req.BirthNumber
            };
            var adress = new DbTables.Adresses
            {
                StreetAdress = req.StreetAdress,
                City = req.City,
                PostCode = req.PostCode,
                County = req.State
            };
            var userAdress = AdressExist(adress);   //check if adress already exist, create new if not
            newUser.Adress = userAdress;
            var result = await userManager.CreateAsync(newUser);
            if (result.Succeeded)
            {
                userManager.AddToRole(newUser.Id, ResolveUserRole(newUser.RoleNr)); //add to role based on role number (3, fadder)

                var provider = new DpapiDataProtectionProvider("ButterflyFriends");
                userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("Passwordresetting"));  
                string code = await userManager.GeneratePasswordResetTokenAsync(newUser.Id);    //create password reset token
                var callbackUrl = Url.Action("SetPassword", "Account", new { userId = newUser.Id, code = code ,area=""}, protocol: Request.Url.Scheme); //url for password setting
                var mailResult= SendEmail(req, callbackUrl, message);   //attempt to send email
                if (!mailResult)
                {
                    ViewBag.MailError = "Email ble ikke sendt";
                }
                var successRequests = from s in _context.MembershipRequests
                    orderby s.Lname
                    select s;

                try
                {

                    
                    _context.MembershipRequests.Remove(req);
                    _context.SaveChanges();

                    ViewBag.Success = "Brukeren " + newUser.Email + " ble lagt til i databasen";

                    return PartialView("_AccordionPartial", successRequests.ToPagedList(pageNumber, pageSize));
                }
                catch (EntityException ex)
                {
                    ViewBag.Error = "Error: " + ex.Message;
                    return PartialView("_AccordionPartial", successRequests.ToPagedList(pageNumber, pageSize));
                }

            }
            var errorstring = "";
            foreach (var error in result.Errors)
            {
                errorstring += " " + error;
            }
            ViewBag.Error = "Noe gikk galt " + errorstring;
            return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// Decline request, delete it and send email about the decline to applicant
        /// </summary>
        /// <returns>partial view with paged list</returns>
        [HttpPost]
        public ActionResult RequestDecline()
        {
            var id = int.Parse(Request.Form["requestid"]);
            var message = Request.Form["message"];

            ViewBag.page = Request.Form["page"];
            var pageNumber = int.Parse(Request.Form["page"]);


            try
            {
                var req = _context.MembershipRequests.Find(id);
                var name = req.Fname + " " + req.Lname;
                _context.MembershipRequests.Remove(req);
                _context.SaveChanges();

                var mailResult=SendEmail(req,null,message);
                if (!mailResult)
                {
                    ViewBag.MailError = "Email ble ikke sendt";
                }
                var requests = from s in _context.MembershipRequests
                    orderby s.Lname
                    select s;

                ViewBag.Success = "Forespørselen fra " + name + " ble fjernet";
                return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));

            }
            catch (EntityException ex)
            {
                var requests = from s in _context.MembershipRequests
                    orderby s.Lname
                    select s;

                ViewBag.Error = "Noe gikk galt: " + ex.Message;
                return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));
            }
        }

        /// <summary>
        /// get request list pased on page and search criteria
        /// </summary>
        /// <param name="page"></param>
        /// <returns>partial view with requests matchin search criteria</returns>
        public ActionResult RequestList(int? page)  
        {
            var search = Request.Form["search"];
            int pageNumber = (page ?? 1);
            ViewBag.page = (page ?? 1);
            if (!string.IsNullOrEmpty(search))
            {
                var requests = from s in _context.MembershipRequests
                               where s.Fname.Contains(search) ||
                                     s.Lname.Contains(search)||
                                     s.City.Contains(search) ||
                                     s.Description.Contains(search)||
                                     s.Email.Contains(search) ||
                                     s.Phone.Contains(search)||
                                     s.PostCode.ToString().Contains(search)||
                                     s.StreetAdress.Contains(search)||
                                     s.State.Contains(search)
                               orderby s.Lname
                               select s;

                return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));

            }

            var requestsNoSearch = from s in _context.MembershipRequests
                orderby s.Lname
                select s;

           

            return PartialView("_AccordionPartial", requestsNoSearch.ToPagedList(pageNumber, pageSize));

        }

        /// <summary>
        /// Filter pased on search criteria
        /// </summary>
        /// <returns>list and partial view based on search criteria</returns>
        public ActionResult Filter()
        {
            int pageNumber = 1;
            ViewBag.page = 1;
            var search = Request.Form["search"];
            if (!string.IsNullOrEmpty(search))
            {
                var requests = from s in _context.MembershipRequests
                               where s.Fname.Contains(search) ||
                                     s.Lname.Contains(search) ||
                                     s.City.Contains(search) ||
                                     s.Description.Contains(search) ||
                                     s.Email.Contains(search) ||
                                     s.Phone.Contains(search) ||
                                     s.PostCode.ToString().Contains(search) ||
                                     s.StreetAdress.Contains(search) ||
                                     s.State.Contains(search)
                               orderby s.Lname
                               select s;

                return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));
            }
            var requestsNoSearch = from s in _context.MembershipRequests
                                   orderby s.Lname
                                   select s;

            return PartialView("_AccordionPartial", requestsNoSearch.ToPagedList(pageNumber, pageSize));
        }
        /// <summary>
        /// check if adress exist
        /// </summary>
        /// <param name="adress">adress to check</param>
        /// <returns>adress if it exist. otherwise just return the initial adress parameter</returns>
        public DbTables.Adresses AdressExist(DbTables.Adresses adress)
        {
            var adresses = _context.Set<DbTables.Adresses>();
            foreach (var Adress in adresses)
            {
                if (adress.StreetAdress == Adress.StreetAdress &&
                    adress.PostCode == Adress.PostCode & adress.City == Adress.City && adress.County == Adress.County)
                {
                    return Adress;
                }
            }
            return adress;
        }

        /// <summary>
        /// send email
        /// </summary>
        /// <param name="request"></param>
        /// <param name="callbackUrl"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendEmail(DbTables.MembershipRequest request,string callbackUrl, string message)
        {
            
                try
                {
                    MailMessage mailMsg = new MailMessage();

                    // To
                    mailMsg.To.Add(new MailAddress(request.Email, request.Fname+" "+request.Lname));

                    // From
                    mailMsg.From = new MailAddress("noreply@butterflyfriends.com", "Butterfly Friends");

                // Subject and multipart/alternative Body
                    if (callbackUrl == null)    //no callbackurl, request is declined
                    {
                        mailMsg.Subject = "Medlemskap avist";
                        if (message != "")
                        {
                            string text = "Vi beklager å måtte melde at din forespørsel om medlamskap har blitt avist. \n\n"+message+"\n\nMvh, \nButterfly Friends";
                            string html = @"<p>Vi beklager å måtte melde at din forespørsel om medlamskap har blitt avist.<br><br>"+message+"<br><br>Mvh,<br>Butterfly Friends</p>";
                            mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null,
                                MediaTypeNames.Text.Plain));
                            mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null,
                                MediaTypeNames.Text.Html));
                        }
                        else
                        {
                        string text = "Vi beklager å måtte melde at din forespørsel om medlamskap har blitt avist. \n\nMvh, \nButterfly Friends";
                        string html = @"<p>Vi beklager å måtte melde at din forespørsel om medlamskap har blitt avist.<br><br>Mvh,<br>Butterfly Friends</p>";
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null,
                            MediaTypeNames.Text.Plain));
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null,
                            MediaTypeNames.Text.Html));
                    }
                }
                    else
                    {
                    mailMsg.Subject = "Medlemskap akseptert";   //Request accepted
                        if (message != "")
                        {
                        string text = "Ditt medlemskap har blitt akseptert og vi er veldig glad for å ha deg med på laget! \n\nDitt passord kan settes her: "+ callbackUrl + "\nDette kan også byttes på dine profilsider.\n\n"+ message + "\n\nMvh, \nButterfly Friends";
                        string html = @"<p>Ditt medlemskap har blitt akseptert og vi er veldig glad for å ha deg med på laget!<br><br>Ditt passord kan settes <a href=" + callbackUrl + ">her</a>.<br>Dette kan ogsp byttes på dine profilsider.<br><br>" + message + "<br><br>Mvh,<br>Butterfly Friends</p>";
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null,  //add both html and normal bodies to email
                            MediaTypeNames.Text.Plain));
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null,
                            MediaTypeNames.Text.Html));
                    }
                        else
                        {
                        string text = "Ditt medlemskap har blitt akseptert og vi er veldig glad for å ha deg med på laget! \n\nDitt passord kan settes her: " + callbackUrl + "\nDette kan også byttes på dine profilsider.\n\nMvh, \nButterfly Friends";
                        string html = @"<p>Ditt medlemskap har blitt akseptert og vi er veldig glad for å ha deg med på laget!<br><br>Ditt kan passord settes <a href=" + callbackUrl + ">her</a>.<br>Dette kan også byttes på dine profilsider.<br><br>Mvh,<br>Butterfly Friends</p>";
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null,
                            MediaTypeNames.Text.Plain));
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null,
                            MediaTypeNames.Text.Html));
                    }
                }
                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));
                var SendGridAPIList = _context.SendGridAPI.ToList();
                var SendGridAPI = new DbTables.SendGridAPI();
                if (SendGridAPIList.Any())
                {
                    SendGridAPI = SendGridAPIList.First();
                    if (!SendGridAPI.Enabeled)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }


                System.Net.NetworkCredential credentials =
                        new System.Net.NetworkCredential(SendGridAPI.UserName,
                            SendGridAPI.PassWord);
                    smtpClient.Credentials = credentials;

                    smtpClient.Send(mailMsg);   //send email
                    
                }
                catch (Exception)
                {
                    return false;
                }
            return true;    //email successfully sent
        }

        /// <summary>
        /// Resolve user role based on user number
        /// </summary>
        /// <param name="roleNr">rolenumber of user</param>
        /// <returns>return the identity role</returns>
        public string ResolveUserRole(int roleNr)
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