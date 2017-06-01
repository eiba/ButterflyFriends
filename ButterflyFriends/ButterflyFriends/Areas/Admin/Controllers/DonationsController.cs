using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;
using ButterflyFriends.Models;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class DonationsController : Controller
    {
        public int pageSize = 10;   //donations per page
        ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/Donations
        /// <summary>
        /// index page. get plans and donations
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var donations = from s in _context.Donations
                orderby s.DonationTime descending
                select s;

            var plans = from s in _context.Subscriptions
                        orderby s.Amount
                        select s;
            var model = new DonationModel
            {
                Donations = donations.ToPagedList(1, pageSize),
                Plans = plans.ToPagedList(1, pageSize)
            };
            return View(model);
        }

        /// <summary>
        /// get list of donastions
        /// </summary>
        /// <param name="page">current page</param>
        /// <returns>filter donations result</returns>
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

        /// <summary>
        /// show modal page for new plan create
        /// </summary>
        /// <param name="planPage">current plan page</param>
        /// <returns></returns>
        public ActionResult showPlanCreate(int? planPage)
        {
            ViewBag.planPage = (planPage ?? 1);

            return PartialView("_CreatePlanPartial", new DbTables.Subscriptions());
        }

        /// <summary>
        /// create new plan with stripe
        /// </summary>
        /// <param name="model">plan parameters</param>
        /// <param name="planPage">plan page</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreatePlan(DbTables.Subscriptions model, int? planPage)
        {
            ViewBag.planPage = (planPage ?? 1);
            if (ModelState.IsValid)
            {
                var stripeList = _context.StripeAPI.ToList();
                if (!stripeList.Any())  //stripe is not configured
                {
                    ViewBag.Error = "Stripe er ikke konfigurert for applikasjonen";

                    var ErrorPlan = from s in _context.Subscriptions
                                    orderby s.Amount
                                    select s;
                    return PartialView("_PlanListPartial", ErrorPlan.ToPagedList(1, pageSize));

                }

                var plan = new DbTables.Subscriptions   
                {
                    Amount = model.Amount,
                    Enabeled = true,
                    Name = model.Name
                };
                _context.Subscriptions.Add(plan);
                _context.SaveChanges();             //need to add here and save to create an unique id

                var client = new WebClient();

                var data = new NameValueCollection();
                data["amount"] = (model.Amount * 100).ToString(CultureInfo.InvariantCulture); // Stripe charges are øre-based in NOK, so 100x the price.
                data["currency"] = "nok";
                data["interval"] = "month";
                data["name"] = model.Name;
                data["id"] = (plan.Id).ToString(CultureInfo.InvariantCulture);  
                client.UseDefaultCredentials = true;

                byte[] response;    //create new plan with stripe API useing data parameters

                client.Credentials = new NetworkCredential(_context.StripeAPI.ToList().First().Secret, "");

                try
                {
                    response = client.UploadValues("https://api.stripe.com/v1/plans", "POST", data); //response
                }
                catch (WebException exception)
                {
                    string responseString;
                    using (var reader = new StreamReader(exception.Response.GetResponseStream()))   //execion happened, read and return string
                    {
                        responseString = reader.ReadToEnd();
                    }

                    ViewBag.Error = responseString;
                    _context.Subscriptions.Remove(plan);    //upon failure remove added plan from database
                    _context.SaveChanges();
                    var ErrorPlan = from s in _context.Subscriptions
                                    orderby s.Amount
                                    select s;
                    return PartialView("_PlanListPartial", ErrorPlan.ToPagedList(1, pageSize));
                }
                //we made it, plan created and accepted by API
                ViewBag.Id = plan.Id;
                ViewBag.Success = "Plan ble suksessfult laget";

                var Succsess = from s in _context.Subscriptions
                               orderby s.Amount
                               select s;
                return PartialView("_PlanListPartial", Succsess.ToPagedList(1, pageSize));

            }

            var errormsg = "";
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors) //error happened, return model errors
                {
                    errormsg += error.ErrorMessage + " \n";
                }
            }

            ViewBag.Error = "Noe gikk galt: " + errormsg;

            var errorPlan = from s in _context.Subscriptions
                            orderby s.Amount
                            select s;

            return PartialView("_PlanListPartial", errorPlan.ToPagedList(1, pageSize));
        }

        /// <summary>
        /// Filter donations result based on parameters
        /// </summary>
        /// <param name="search"></param>
        /// <param name="active"></param>
        /// <param name="order"></param>
        /// <param name="filter"></param>
        /// <param name="dob"></param>
        /// <param name="sponsor"></param>
        /// <param name="pageNumber"></param>
        /// <returns>return partial view with list</returns>
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

        /// <summary>
        /// show details about donation in modal box
        /// </summary>
        /// <param name="id">id of donations</param>
        /// <param name="page">current donation page</param>
        /// <returns>partial view with donation modal box</returns>
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