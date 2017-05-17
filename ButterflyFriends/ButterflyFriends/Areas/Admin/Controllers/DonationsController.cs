using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;
using ButterflyFriends.Models;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class DonationsController : Controller
    {
        public int pageSize = 10;
        ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/Donations
        public ActionResult Index()
        {
            var donations = from s in _context.Donations
                orderby s.DonationTime descending
                select s;

            return View(donations.ToPagedList(1, pageSize));
        }

        public ActionResult DonationList(int? page)
        {

            var search = Request.Form["search"];
            var active = Request.Form["active"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var ChildDoB = (Request.Form["ChildDoB"]);
            var ChildSponsor = Request.Form["ChildSponsor"];

            int pageNumber = (page ?? 1);
            ViewBag.page = (page ?? 1);

            return FilterResultDonations(search, active, order, filter, ChildDoB, ChildSponsor, pageNumber);

        }

        public PartialViewResult FilterResultDonations(string search, string active, string order, string filter, string dob, string sponsor, int pageNumber)
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
            var donations = from s in _context.Donations
                           select s;
            if (DoB.HasValue)
            {
                donations = donations.Where(s => s.DonationTime.Equals(DoB.Value));
            }

            if (order == "descending")
            {
                switch (filter)
                {
                    case "1":
                        
                        break;
                    default:
                       
                        break;
                }
            }
            else
            {
                switch (filter)
                {
                    case "1":
                       
                        break;
                    default:
                        
                        break;
                }
            }
            if (pageNumber > 1 && donations.Any())
            {
                if (donations.Count() <= pageSize * (pageNumber))
                {

                    pageNumber = (int)Math.Ceiling((double)donations.Count() / (double)pageSize);
                    ViewBag.page = pageNumber;
                    return PartialView("DonationListPartial", donations.ToPagedList(pageNumber, pageSize));
                }
            }
            else if (!donations.Any())
            {
                ViewBag.page = 1;
                return PartialView("DonationListPartial", donations.ToPagedList(pageNumber, pageSize));

            }
            return PartialView("DonationListPartial", donations.ToPagedList(pageNumber, pageSize));

        }
        [HttpGet]
        public ActionResult showDonationDetails(string id, int? page)
        {
            ViewBag.page = (page ?? 1);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbTables.Donations donation = _context.Donations.Find(int.Parse(id));
            if (donation == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return PartialView("_DonationDetailsPartial",donation);


        }
    }
}