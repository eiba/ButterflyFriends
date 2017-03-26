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

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class MemberRequestsController : Controller
    {
        ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/MemberRequests
        public ActionResult Index()
        {

            return View(_context.MembershipRequests.ToList());
        }


        [HttpPost]
        public async Task<ActionResult> RequestAccept()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
            var email = Request.Form["email"];
            var id = int.Parse(Request.Form["requestid"]);
            var req = _context.MembershipRequests.Find(id);

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
                        return PartialView("_AccordionPartial", _context.MembershipRequests.ToList());


                    }
                }

            }/*
            var fname = Request.Form["fname"];
            var lname = Request.Form["lname"];
            var phone = Request.Form["phone"];
            var streetadress = Request.Form["streetadress"];
            var city = Request.Form["city"];
            var state = Request.Form["state"];
            var postcode = Request.Form["postcode"];*/


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
            var result = await userManager.CreateAsync(newUser,password);
            if (result.Succeeded)
            {
                try
                {
                    userManager.AddToRole(newUser.Id, newUser.AccessLvL);
                    _context.MembershipRequests.Remove(req);
                    _context.SaveChanges();
                    ViewBag.Success = "Brukeren " + newUser.Email + " ble lagt til i databasen";
                    return PartialView("_AccordionPartial", _context.MembershipRequests.ToList());
                }
                catch (EntityException ex)
                {
                    ViewBag.Error = "Error "+ex.Message;
                    return PartialView("_AccordionPartial", _context.MembershipRequests.ToList());
                }

            }
            ViewBag.Error = "Noe gikk galt " + result.Errors;
            return PartialView("_AccordionPartial",_context.MembershipRequests.ToList());
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