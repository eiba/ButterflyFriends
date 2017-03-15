using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ButterflyFriends.Areas.Admin.Models;
using ButterflyFriends.Areas.Admin.Models.HRmanagementModels;
using ButterflyFriends.Controllers;
using ButterflyFriends.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using PagedList;

namespace ButterflyFriends.Areas.Admin.Controllers
{
    public class HRManagementController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();

        // GET: Admin/HRManagement
        public ActionResult Index()
        {
            var model = new HRManagamentModel
            {
                EmployeeModel = new EmployeeModel { Employees = _context.Users.ToList()}
    };
            return View(model);
        }

        [HttpGet]
        public ActionResult showEmployeeDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = _context.Users.Find(id);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return PartialView("DetailPartials/_EmployeeDetailsPartial",user);
        }

        [HttpGet]
        public ActionResult showEmployeeEdit(string id)
        {
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

            return PartialView("EditPartials/_EmployeeEditPartial",model);
        }
        [HttpGet]
        public ActionResult showEmployeeCreate()
        {
            

            return PartialView("CreatePartials/_EmployeeCreatePartial", new RegisterViewModel());
        }

        [HttpGet]
        public async Task<ActionResult> employeeDeactivate(string id)
        {
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

            user.LockoutEnabled = true;
            user.LockoutEndDateUtc = new DateTime(9999, 12, 30);
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
                
                ViewBag.Error = "Noe gikk galt - "+ result.Errors;
                ViewBag.Id = user.Id;
                ViewBag.Worked = false;
            }

            return PartialView("ListPartials/_EmployeePartial",new EmployeeModel {Employees = _context.Users.ToList()});
        }

        [HttpGet]
        public async Task<ActionResult> employeeActivate(string id)
        {
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

            user.LockoutEnabled = false;
            user.IsEnabeled = true;

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

            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList() });
        }

        [HttpPost]
        public async Task<ActionResult> EditEmployee(changeUserInfo model)
        {
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

                if (results.Any()) { 
                foreach (var appUser in results)
                {
                    if (appUser.Id != model.Id)
                    {
                        ViewBag.Id = user.Id;
                        ViewBag.Error = "Emailen er allerede i bruk";
                        return PartialView("ListPartials/_EmployeePartial", new EmployeeModel {Employees = _context.Users.ToList()});
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
                            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList() });
                        }
                    }

                    ViewBag.Success = "Brukeren "+ user.Email+" ble oppdatert.";
                    ViewBag.Id = user.Id;
                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList()});
                    
                }

                    ViewBag.Id = user.Id;
                    ViewBag.Error = "Noe gikk galt "+result.Errors;
                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList() });
                
            }
          
                ViewBag.Id = model.Id;
                ViewBag.Error = "Noe gikk galt";
                return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList() });
            


        }

        [HttpPost]
        public async Task<ActionResult> CreateEmployee(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_context));
                var results = (from s in _context.Users
                              where
                                  s.Email.Contains(model.Email)
                              select s).ToList();
                
                if (results.Any())
                {
                    foreach (var r in results)
                    {
                        if (r.Email == model.Email)
                        {
                            
                            ViewBag.Error = "Emailen er allerede i bruk.";
                           
                                return PartialView("ListPartials/_EmployeePartial",
                                    new EmployeeModel {Employees = _context.Users.ToList()});
                           
                        }
                    }

                }
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Fname = model.Fname,
                    Lname = model.Lname,
                    Phone = model.Phone,
                    AccessLvL = "Employee",
                    IsEnabeled = true,
                    RoleNr = 0
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
                        return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList() });

                    }

                }
                user.Adress = UserAdress;

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    
                    userManager.AddToRole(user.Id, user.AccessLvL);
                    ViewBag.Success = "Brukeren "+user.Email+" ble lagt til i databasen";
                    ViewBag.Id = user.Id;
                    //var users = _context.Users.ToList();
                    return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList() });
                }
                ViewBag.Error = "Noe gikk galt "+ result.Errors;
                return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList() });
            }

            ViewBag.Error = "Noe gikk galt";
            return PartialView("ListPartials/_EmployeePartial", new EmployeeModel { Employees = _context.Users.ToList() });
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
    }
}