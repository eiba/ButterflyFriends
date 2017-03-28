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
    public class MemberRequestsController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        public int pageSize = 10;

        // GET: Admin/MemberRequests
        public ActionResult Index()
        {
            var requests = from s in _context.MembershipRequests
                orderby s.Lname
                select s;
            ViewBag.page = 1;
            return View(requests.ToPagedList(1, pageSize));
        }


        [HttpPost]
        public async Task<ActionResult> RequestAccept()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var id = int.Parse(Request.Form["requestid"]);
            var message = Request.Form["message"];
            var req = _context.MembershipRequests.Find(id);

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
                    if (r.Email == email)
                    {

                        ViewBag.Error = "Emailen er allerede i bruk.";
                        return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));


                    }
                }

            }

            var newUser = new ApplicationUser
            {
                Email = email,
                UserName = email,
                Fname = req.Fname,
                Lname = req.Lname,
                Phone = req.Phone,
                RoleNr = 0,
                AccessLvL = "Sponsor",
                IsEnabeled = true
            };
            var adress = new DbTables.Adresses
            {
                StreetAdress = req.StreetAdress,
                City = req.City,
                PostCode = req.PostCode,
                County = req.State
            };
            var userAdress = AdressExist(adress);
            newUser.Adress = userAdress;
            var result = await userManager.CreateAsync(newUser);
            if (result.Succeeded)
            {
                userManager.AddToRole(newUser.Id, newUser.AccessLvL);

                var provider = new DpapiDataProtectionProvider("ButterflyFriends");
                userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(provider.Create("Passwordresetting"));
                string code = await userManager.GeneratePasswordResetTokenAsync(newUser.Id);
                var callbackUrl = Url.Action("SetPassword", "Account", new { userId = newUser.Id, code = code ,area=""}, protocol: Request.Url.Scheme);
                var mailResult= SendEmail(req, callbackUrl, message);
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

        public ActionResult RequestList(int? page)
        {
            var requests = from s in _context.MembershipRequests
                orderby s.Lname
                select s;

            int pageNumber = (page ?? 1);
            ViewBag.page = (page ?? 1);

            return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));

        }

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
                    if (callbackUrl == null)
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
                    mailMsg.Subject = "Medlemskap akseptert";
                        if (message != "")
                        {
                        string text = "Ditt medlemskap har blitt akseptert og vi er veldig glad for å ha deg med på laget! \n\nDitt passord kan settes her: "+ callbackUrl + "\nDette kan også byttes på dine profilsider.\n\n"+ message + "\n\nMvh, \nButterfly Friends";
                        string html = @"<p>Ditt medlemskap har blitt akseptert og vi er veldig glad for å ha deg med på laget!<br><br>Ditt passord kan settes <a href=" + callbackUrl + ">her</a>.<br>Dette kan ogsp byttes på dine profilsider.<br><br>" + message + "<br><br>Mvh,<br>Butterfly Friends</p>";
                        mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null,
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
                    System.Net.NetworkCredential credentials =
                        new System.Net.NetworkCredential("azure_37743dcaeaf80d7f3e17e3f077a91b20@azure.com",
                            "MJK67pm30g");
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
}