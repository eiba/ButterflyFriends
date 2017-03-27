using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.BuilderProperties;
using PagedList;

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
            return View(requests.ToPagedList(3,pageSize));
        }


        [HttpPost]
        public async Task<ActionResult> RequestAccept()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var email = Request.Form["email"];
            var id = int.Parse(Request.Form["requestid"]);
            var message = Request.Form["message"];
            var req = _context.MembershipRequests.Find(id);

            ViewBag.page = Request.Form["page"];
            var pageNumber = int.Parse(Request.Form["page"]);
            var requests = from s in _context.MembershipRequests
                           orderby s.Lname
                           select s;

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
                        return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber,pageSize));


                    }
                }

            }

            var newUser = new ApplicationUser
            {
                Email = email,
                UserName = email,
                Fname = Request.Form["fname"],
                Lname = Request.Form["lname"],
                Phone = Request.Form["phone"],
                AccessLvL = "Sponsor",
                IsEnabeled = true,
                RoleNr = 0
            };
            var adress = new DbTables.Adresses
            {
                StreetAdress = Request.Form["streetadress"],
                City = Request.Form["city"],
                PostCode = int.Parse(Request.Form["postcode"]),
                County = Request.Form["state"]
            };
            var userAdress = AdressExist(adress);
            newUser.Adress = userAdress;
            var password = Membership.GeneratePassword(12, 1);
            var result = await userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                var successRequests = from s in _context.MembershipRequests
                                      orderby s.Lname
                                      select s;
                try
                {
                    

                    userManager.AddToRole(newUser.Id, newUser.AccessLvL);
                    _context.MembershipRequests.Remove(req);
                    _context.SaveChanges();
                    ViewBag.Success = "Brukeren " + newUser.Email + " ble lagt til i databasen";
                    return PartialView("_AccordionPartial", successRequests.ToPagedList(pageNumber, pageSize));
                }
                catch (EntityException ex)
                {
                    ViewBag.Error = "Error " + ex.Message;
                    return PartialView("_AccordionPartial", successRequests.ToPagedList(pageNumber, pageSize));
                }

            }
            ViewBag.Error = "Noe gikk galt " + result.Errors;
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

                var requests = from s in _context.MembershipRequests
                               orderby s.Lname
                               select s;

                ViewBag.Success = "Forespørselen fra "+name+" ble fjernet";
                return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber, pageSize));

            }
            catch (EntityException ex)
            {
                var requests = from s in _context.MembershipRequests
                               orderby s.Lname
                               select s;

                ViewBag.Error = "Noe gikk galt: "+ex.Message;
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

            return PartialView("_AccordionPartial", requests.ToPagedList(pageNumber,pageSize));

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
            return adress;
        }
    }
}