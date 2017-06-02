using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Core;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Models;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class DonationsController : Controller
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        public int pageSize = 10; //elements per page

        // GET: Admin/Donations
        /// <summary>
        ///     index page. get plans and donations
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
                Plans = plans.ToPagedList(1, pageSize),
                Subscriptions = new List<Subscription>()
            };
            return View(model);
        }

        /// <summary>
        ///     get list of plans
        /// </summary>
        /// <param name="page">current page</param>
        /// <returns>filter donations result</returns>
        public ActionResult PlanList(int? planPage)
        {
            var pageNumber = planPage ?? 1;
            ViewBag.planPage = planPage ?? 1;

            var plans = from s in _context.Subscriptions
                orderby s.Amount
                select s;

            return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        ///     get list of donastions
        /// </summary>
        /// <param name="page">current page</param>
        /// <returns>filter donations result</returns>
        public ActionResult DonationList(int? page)
        {
            var search = Request.Form["search"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var phone = Request.Form["DonationPhone"];
            var birthnumber = Request.Form["DonationBirthNumber"];

            var pageNumber = page ?? 1;
            ViewBag.page = page ?? 1;

            return FilterResultDonations(search, order, filter, birthnumber, phone, pageNumber);
        }

        /// <summary>
        ///     Filters donations
        /// </summary>
        /// <returns>filtered partial view list</returns>
        public ActionResult FilterDonations()
        {
            var search = Request.Form["search"];
            var order = Request.Form["order"];
            var filter = Request.Form["filter"];
            var phone = Request.Form["DonationPhone"];
            var birthnumber = Request.Form["DonationBirthNumber"];

            var pageNumber = 1;
            ViewBag.page = 1;

            return FilterResultDonations(search, order, filter, birthnumber, phone, pageNumber);
        }

        /// <summary>
        ///     show modal page for new plan create
        /// </summary>
        /// <param name="planPage">current plan page</param>
        /// <returns></returns>
        public ActionResult showPlanCreate(int? planPage)
        {
            ViewBag.planPage = planPage ?? 1;

            return PartialView("_CreatePlanPartial", new DbTables.Subscriptions());
        }

        /// <summary>
        ///     show modal page for deleting plan
        /// </summary>
        /// <param name="planPage">current plan page</param>
        /// <returns></returns>
        public ActionResult showPlanDelete(int id, int? planPage)
        {
            ViewBag.planPage = planPage ?? 1;
            var pageNumber = planPage ?? 1;

            var plans = from s in _context.Subscriptions
                orderby s.Amount
                select s;
            if (id == 0)
            {
                ViewBag.Error = "Ugyldig id";
                return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
            }
            var plan = _context.Subscriptions.Find(id);
            if (plan == null)
            {
                ViewBag.Error = "Plan ble ikke funnet";
                return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
            }

            return PartialView("_DeletePlanPartial", plan);
        }

        /// <summary>
        ///     Deletes plan
        /// </summary>
        /// <param name="id">id of plan to delete</param>
        /// <param name="planPage">page of plan list</param>
        /// <returns>updated list of plans</returns>
        public ActionResult PlanDelete(int id, int? planPage)
        {
            ViewBag.planPage = planPage ?? 1;
            var pageNumber = planPage ?? 1;

            var plans = from s in _context.Subscriptions
                orderby s.Amount
                select s;
            if (id == 0)
            {
                ViewBag.Error = "Ugyldig id";
                return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
            }
            var plan = _context.Subscriptions.Find(id);
            if (plan == null)
            {
                ViewBag.Error = "Plan ble ikke funnet";
                return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
            }

            try
            {
                _context.Subscriptions.Remove(plan);
                _context.SaveChanges();

                ViewBag.Success = "Planen ble slettet";

                var SuccessPlans = from s in _context.Subscriptions
                    orderby s.Amount
                    select s;
                return PartialView("_PlanListPartial", SuccessPlans.ToPagedList(pageNumber, pageSize));
            }
            catch (EntityException ex)
            {
                ViewBag.Id = id;
                ViewBag.Error = "Error: " + ex.Message;
                return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
            }
        }

        /// <summary>
        ///     Deactivates plan
        /// </summary>
        /// <param name="id">id of plan</param>
        /// <param name="planPage">pagnation page for plan list</param>
        /// <returns>updated plan partial view</returns>
        public ActionResult PlanDeactivate(int id, int? planPage)
        {
            ViewBag.planPage = planPage ?? 1;
            var pageNumber = planPage ?? 1;

            if (id == 0)
            {
                var Errorplans = from s in _context.Subscriptions
                    orderby s.Amount
                    select s;
                ViewBag.Error = "Ugyldig id";
                return PartialView("_PlanListPartial", Errorplans.ToPagedList(pageNumber, pageSize));
            }
            var plan = _context.Subscriptions.Find(id);
            if (plan == null)
            {
                var Errorplans = from s in _context.Subscriptions
                    orderby s.Amount
                    select s;
                ViewBag.Error = "Plan ble ikke funnet";
                return PartialView("_PlanListPartial", Errorplans.ToPagedList(pageNumber, pageSize));
            }
            plan.Enabeled = false; //deactivate plan

            _context.SaveChanges();
            ViewBag.Id = id;
            ViewBag.Success = "Planen " + plan.Name + " ble suksessfult deaktivert";

            var plans = from s in _context.Subscriptions
                orderby s.Amount
                select s;

            return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        ///     Activates plan
        /// </summary>
        /// <param name="id">id of plan</param>
        /// <param name="planPage">pagnation page for plan list</param>
        /// <returns>updated plan partial view</returns>
        public ActionResult PlanActivate(int id, int? planPage)
        {
            ViewBag.planPage = planPage ?? 1;
            var pageNumber = planPage ?? 1;

            if (id == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var plan = _context.Subscriptions.Find(id);
            if (plan == null)
                return HttpNotFound();
            plan.Enabeled = true;

            _context.SaveChanges();
            ViewBag.Id = id;
            ViewBag.Success = "Planen " + plan.Name + " ble suksessfult aktivert";

            var plans = from s in _context.Subscriptions
                orderby s.Amount
                select s;

            return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        ///     Search for payment plans based on search input
        /// </summary>
        /// <returns>Partial view list of plans</returns>
        public ActionResult SearchPlans()
        {
            var search = Request.Form["search"];

            var plans = from s in _context.Subscriptions
                where s.Id.ToString().Contains(search) ||
                      s.Name.Contains(search) ||
                      s.Amount.ToString().Contains(search)
                orderby s.Amount
                select s;

            ViewBag.planPage = 1;
            var pageNumber = 1;

            return PartialView("_PlanListPartial", plans.ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        ///     Connects to the stripe API and searches for customers based on id
        /// </summary>
        /// <returns>returns partial view with</returns>
        public ActionResult SearchSubscriptions()
        {
            var search = Request.Form["search"];

            var stripeList = _context.StripeAPI.ToList();
            if (stripeList.Any())
            {
                var client = new WebClient();
                byte[] response;
                client.UseDefaultCredentials = true;
                client.Credentials = new NetworkCredential(stripeList.First().Secret, "");

                try
                {
                    response = client.DownloadData("https://api.stripe.com/v1/subscriptions?customer=" + search);
                }
                catch (WebException) //exepction happen when getting fom API
                {
                    /*string responseString;
                    using (var reader = new StreamReader(exception.Response.GetResponseStream())) //read the errorstring
                    {
                        responseString = reader.ReadToEnd();
                    }
                    ViewBag.Error = responseString; //error happened*/

                    ViewBag.Message = "Ingen kunde matchende kriteriene ble funnet";
                    return PartialView("_SubscriptionListPartial", new List<Subscription>());
                }
                var json_serializer = new JavaScriptSerializer();
                var JsonDict =
                    (IDictionary<string, object>) json_serializer.DeserializeObject(client.Encoding.GetString(response));
                var data = JsonDict["data"] as IEnumerable;
                IList<Subscription> subs = new List<Subscription>();
                foreach (var item in data) //parse the data gotten from API
                {
                    var i = item as IDictionary; //parse object as dictionary
                    var plan = i["plan"] as IDictionary;
                    var id = i["id"].ToString();
                    var customer = i["customer"].ToString();
                    var planId = plan["id"].ToString();
                    var planName = plan["name"].ToString();
                    var amount = int.Parse(plan["amount"].ToString())/100;
                    var Subscription = new Subscription //create subscription object
                    {
                        CustomerId = customer,
                        PlanId = planId,
                        PlanName = planName,
                        SubId = id,
                        Amount = amount
                    };
                    subs.Add(Subscription); //add subscription to list
                }
                return PartialView("_SubscriptionListPartial", subs);
            }
            ViewBag.Error = "Stripe er ikke konfigurert for applikasjonen";
            return PartialView("_SubscriptionListPartial", new List<Subscription>());
        }

        /// <summary>
        ///     create new plan with stripe
        /// </summary>
        /// <param name="model">plan parameters</param>
        /// <param name="planPage">plan page</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreatePlan(DbTables.Subscriptions model, int? planPage)
        {
            ViewBag.planPage = planPage ?? 1;
            if (ModelState.IsValid)
            {
                var stripeList = _context.StripeAPI.ToList();
                if (!stripeList.Any()) //stripe is not configured
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
                _context.SaveChanges(); //need to add here and save to create an unique id

                var client = new WebClient();

                var data = new NameValueCollection();
                data["amount"] = (model.Amount*100).ToString(CultureInfo.InvariantCulture);
                // Stripe charges are øre-based in NOK, so 100x the price.
                data["currency"] = "nok";
                data["interval"] = "month";
                data["name"] = model.Name;
                data["id"] = plan.Id.ToString(CultureInfo.InvariantCulture);
                client.UseDefaultCredentials = true;

                byte[] response; //create new plan with stripe API useing data parameters

                client.Credentials = new NetworkCredential(_context.StripeAPI.ToList().First().Secret, "");

                try
                {
                    response = client.UploadValues("https://api.stripe.com/v1/plans", "POST", data); //response
                }
                catch (WebException exception)
                {
                    string responseString;
                    using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                        //exception happened, read and return string
                    {
                        responseString = reader.ReadToEnd();
                    }

                    ViewBag.Error = responseString;
                    _context.Subscriptions.Remove(plan); //upon failure remove added plan from database
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
            foreach (var modelState in ViewData.ModelState.Values)
                foreach (var error in modelState.Errors) //error happened, return model errors
                    errormsg += error.ErrorMessage + " \n";

            ViewBag.Error = "Noe gikk galt: " + errormsg;

            var errorPlan = from s in _context.Subscriptions
                orderby s.Amount
                select s;

            return PartialView("_PlanListPartial", errorPlan.ToPagedList(1, pageSize));
        }

        /// <summary>
        ///     Filter donations result based on parameters
        /// </summary>
        /// <param name="search"></param>
        /// <param name="order"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <returns>return partial view with list</returns>
        public PartialViewResult FilterResultDonations(string search, string order, string filter, string birthnumber,
            string phone, int pageNumber)
        {
            var donations = from s in _context.Donations
                where s.BirthNumber.Contains(birthnumber) ||
                      s.User.BirthNumber.Contains(birthnumber) || s.Email.Contains(search) ||
                      s.User.Email.Contains(search) || s.Name.Contains(search) ||
                      (s.User.Fname + " " + s.User.Lname).Contains(search) || s.Phone.Contains(phone) ||
                      s.User.Phone.Contains(phone)
                orderby s.Id
                select s;

            if (order == "descending") //filter by decending or ascending order
                switch (filter)
                {
                    case "1":
                        donations.OrderByDescending(s => s.Email).ThenByDescending(s => s.User.Email);
                        break;
                    default:
                        donations.OrderByDescending(s => s.Name)
                            .ThenByDescending(s => s.User.Fname)
                            .ThenByDescending(s => s.User.Lname);
                        break;
                }
            else
                switch (filter)
                {
                    case "1":
                        donations.OrderBy(s => s.Email).ThenBy(s => s.User.Email);
                        break;
                    default:
                        donations.OrderBy(s => s.Name).ThenBy(s => s.User.Fname).ThenBy(s => s.User.Lname);
                        break;
                }

            if ((pageNumber > 1) && donations.Any()) //check if page is empty, if so find highest available
            {
                if (donations.Count() <= pageSize*pageNumber)
                {
                    pageNumber = (int) Math.Ceiling(donations.Count()/(double) pageSize);
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
        ///     show details about donation in modal box
        /// </summary>
        /// <param name="id">id of donations</param>
        /// <param name="page">current donation page</param>
        /// <returns>partial view with donation modal box</returns>
        [HttpGet]
        public ActionResult showDonationDetails(string id, int? page)
        {
            ViewBag.page = page ?? 1;

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var donation = _context.Donations.Find(int.Parse(id));
            if (donation == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return PartialView("_DonationDetailsPartial", donation);
        }
    }
}