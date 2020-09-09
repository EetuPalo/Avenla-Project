using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Login_System.ViewModels;
using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Cryptography.X509Certificates;

namespace Login_System.Controllers
{
    [Authorize(Roles = "User, Admin, Superadmin")]
    public class AppUsersController : Controller
    {
        private readonly IdentityDataContext _context;
        private readonly GeneralDataContext dataContext;
        private UserManager<AppUser> UserMgr { get; }
        private readonly RoleManager<AppRole> roleManager;
        private SignInManager<AppUser> SignInMgr { get; }
        private readonly GeneralDataContext CompanyList;

        public AppUsersController(GeneralDataContext dataCon, IdentityDataContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, GeneralDataContext CompList )
        {
            _context = context;
            UserMgr = userManager;
            SignInMgr = signInManager;
            this.roleManager = roleManager;
           
            dataContext = dataCon;
            CompanyList = CompList;
        }
        [Authorize(Roles = "Admin, Superadmin")]
        // GET: AppUsers
        public async Task<IActionResult> Index(string searchString, string order)
        {
           
            var user = await UserMgr.GetUserAsync(HttpContext.User);

            //  Stores the search input to preserve it in the text field
            if (!String.IsNullOrEmpty(searchString))
            {
                TempData["LastSearch"] = searchString;
            }
            IQueryable<AppUser> employees;
            if (User.IsInRole("Superadmin"))
            {
                employees = from e in _context.Users  select e;
            }
            else
            {
                employees = from e in _context.Users where e.Company == user.Company select e;
            }
            switch (order)
            {
                case "FirstName":
                    employees = employees.OrderBy(x => x.FirstName);
                    TempData["order"] = "FirstName";
                    break;

                case "FirstNameDesc":
                    employees = employees.OrderByDescending(x => x.FirstName);
                    TempData["order"] = "FirstNameDesc";
                    break;

                case "LastName":
                    employees = employees.OrderBy(x => x.LastName);
                    TempData["order"] = "LastName";
                    break;

                case "LastNameDesc":
                    employees = employees.OrderByDescending(x => x.LastName);
                    TempData["order"] = "LastNameDesc";
                    break;

                case "Email":
                    employees = employees.OrderBy(x => x.Email);
                    break;

            }
            TempData["SearchString"] = Resources.Resources.Employee_Index_SearchPholder;
            TempData["SearchValue"] = null;
            
            //SEARCH
            //The search searches by name, email, phone
            if (!String.IsNullOrEmpty(searchString))
            {                
                employees = employees.Where(s => (s.UserName.Contains(searchString)) || (s.FirstName.Contains(searchString)) || (s.LastName.Contains(searchString)) || (s.Email.Contains(searchString)) || (s.PhoneNumber.Contains(searchString)));
                TempData["SearchValue"] = searchString;
            }           
            return View(await employees.ToListAsync());
        }

        // GET: AppUsers/Details/5
#nullable enable
        [HttpGet]
        public async Task<IActionResult> Details(string? source, int? id, string? sourceId)
        {
            var skillList = new List<UserSkills>();
            var currentUser = await UserMgr.GetUserAsync(HttpContext.User);

            //AppUser tempUser = await UserMgr.FindByIdAsync(currentUser.Id.ToString());

            //Sources are for storing the info of the previous page, so that the back button can take the user back to the right place
            if (String.IsNullOrWhiteSpace(source))
            {
                TempData["Source"] = "AppUser";
            }
            if (source != null)
            {
                TempData["Source"] = source;
            }
            if (sourceId != null)
            {
                TempData["SourceId"] = sourceId;
            }
            if (id == null)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }

            //Finding the correct user
            var appUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            var companyID = tempUser.Company;
            string companyName = dataContext.Company.FirstOrDefault(x => x.Id == companyID).Name;
            TempData["UserId"] = id;
            TempData["UserFullName"] = tempUser.FirstName + " " + tempUser.LastName;
            ViewBag.CurrentCompany = companyName;

            //Populating the VM with user info
            AppUserVM model = new AppUserVM
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Active = appUser.Active,
                EmpStatus = appUser.EmpStatus,
                UserName = appUser.UserName,
                Email = appUser.Email,
                PhoneNumber = appUser.PhoneNumber,
                Id = appUser.Id
            };

            //Populating the VM with group, course, and certificate info
            var tempList = new List<string>();
            var courseList = new List<SkillCourseMember>();
            var certificateList = new List<UserCertificate>();

            // Skills
            foreach (var skill in dataContext.UserSkills.Where(x => x.UserID == id))
            {
                var skillQueryDate = from t in dataContext.UserSkills
                                     group t by t.UserID into g
                                     select new { UserID = g.Key, Date = g.Max(t => t.Date) };

                foreach (var it in skillQueryDate.Where(x => x.Date == skill.Date))
                {
                    if (!skillList.Contains(skill))
                    {
                        skillList.Add(skill);
                    }
                }
            }
            foreach (var groupMember in dataContext.GroupMembers.Where(x => x.UserID == id))
            {
                tempList.Add(groupMember.GroupName);
            }
            foreach (var courseMember in dataContext.SkillCourseMembers.Where(x => x.UserID == id))
            {
                courseList.Add(courseMember);
            }
            foreach (var userCertificate in dataContext.UserCertificates.Where(x => x.UserID == id))
            {
                certificateList.Add(userCertificate);
            }
            model.UserSkills = skillList;
            model.UserGroups = tempList;
            model.UserCourses = courseList;
            model.UserCertificates = certificateList;

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Create()
        {
          
            if (User.IsInRole("Admin") || User.IsInRole("Superadmin"))
            {
                var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
                TempData["Company"] = currentUser.Company;
                var model = new RegisterVM();
                var tempList = new List<Company>();
                List<CompanyMember> companyList = new List<CompanyMember>();
                if (User.IsInRole("Admin"))
                {
                    companyList = CompanyList.CompanyMembers.Where(x => x.UserId == currentUser.Id).ToList();
                }
                
                //Populating the dropdown with companies
                foreach (var company in CompanyList.Company)
                {
                    if(User.IsInRole("Superadmin"))
                    {
                        model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString() });
                    }

                    else
                    {
                        if(companyList.Any(x=> x.CompanyId == company.Id))
                        {
                            model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString() });
                        }
                    }
                  
                }
                foreach (var roles in roleManager.Roles)
                {
                    if((roles.Name != "Superadmin" && User.IsInRole("Admin")) || User.IsInRole("Superadmin"))
                    {
                        model.RolesList.Add(new SelectListItem() { Text = roles.Name, Value = roles.Name });
                    }
                   
                }
                return View(model);
            }

            else if (!_context.Users.Any())
            {
                ViewBag.EmptySet = true;
                var model = new RegisterVM();
                
                var company = (await dataContext.Company.AddAsync(new Company { Name = "Superadmin", Description = "Placeholder"})).Entity;
                foreach (var roles in roleManager.Roles)
                {
                    model.RolesList.Add(new SelectListItem() { Text = roles.Name, Value = roles.Name });
                }
                await dataContext.SaveChangesAsync();
                TempData["Company"] = company.Id;
             
                return View(model);
            }

            return RedirectToAction("Login", "Account");
        }
        public async Task<IActionResult> AppUserEdit()
        {
            var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
            ViewBag.Company = currentUser.Company;
            var model = new EditUserVM();
            var tempList = new List<Company>();
            foreach (var company in CompanyList.Company)
            {
                model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString() });
            }
            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Superadmin")]
      
        public async Task<IActionResult> Create([Bind("EMail, FirstName, LastName, PhoneNumber, Company, Password, ConfirmPassword")] RegisterVM appUser, string SelectedRole,List<int> Companies)
        {
             if (ModelState.IsValid)
            {
                var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
                Console.Write(appUser.Company);
                //This constructs the username from the users first and last names
                string userName = appUser.FirstName + appUser.LastName;
                Company company = await dataContext.Company.FirstOrDefaultAsync(x=> x.Id == Companies.First());
                var k = 1;
                var veryTempUser = await UserMgr.FindByNameAsync(userName);
                while (veryTempUser != null)
                {
                    userName = userName + k;
                    veryTempUser = await UserMgr.FindByNameAsync(userName);
                    k++;
                }
                //This is supposed to remove any special characters from the userName string
                byte[] tempBytes;
                tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(userName);
                string fixedUn = Encoding.UTF8.GetString(tempBytes);
                fixedUn = RemoveSpecialCharacters(fixedUn);

                //Checking that the user with the same name doesnt exist
                AppUser user = await UserMgr.FindByNameAsync(fixedUn);
                if (user == null)
                {
                    //if no roles exist, create required ones
                    
                    try
                    {
                        if (User.IsInRole("Superadmin"))
                        {
                            user = new AppUser
                            {
                                //we create a new user and set his credentials to the data received from the Register form.
                                UserName = fixedUn,
                                Email = appUser.EMail,
                                FirstName = appUser.FirstName,
                                LastName = appUser.LastName,
                                EmpStatus = "Active",
                                PhoneNumber = appUser.PhoneNumber,
                                Company = company.Id,
                            };
                        }
                        else
                        {
                            user = new AppUser
                            {
                                //we create a new user and set his credentials to the data received from the Register form.
                                UserName = fixedUn,
                                Email = appUser.EMail,
                                FirstName = appUser.FirstName,
                                LastName = appUser.LastName,
                                EmpStatus = "Active",
                                PhoneNumber = appUser.PhoneNumber,
                                Company =  currentUser.Company,
                            };
                        }
                      
                        //we then create a new user through usermanager
                        IdentityResult result;
                        IdentityResult roleResult;
                        result = await UserMgr.CreateAsync(user, appUser.Password);

                        //checking if there are users that have roles that exist in database. if not, mostlikely the first user, assign as a superadmin
                        if (!_context.UserRoles.Any())
                        {
                            roleResult = await UserMgr.AddToRoleAsync(user, "Superadmin");
                        }
                        else
                        {
                            roleResult = await UserMgr.AddToRoleAsync(user, SelectedRole);
                        }
                       
                   
                        foreach (var companyId in Companies)
                        {
                            
                            var newMember = new CompanyMember
                            {
                                CompanyId = companyId,

                                UserId = user.Id
                            };
                            CompanyList.CompanyMembers.Add(newMember);
                        }
                        await dataContext.SaveChangesAsync();
                      

                    }
                    catch
                    {
                        TempData["CreateStatus"] = Resources.ActionMessages.CreateStatus_Error;
                    }
                   
                    TempData["ActionResult"] = Resources.ActionMessages.CreateStatus_Success1 + fixedUn + Resources.ActionMessages.CreateStatus_Success2;
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["CreateStatus"] = Resources.ActionMessages.CreateStatus_UserTaken;
                    return RedirectToAction("Index");
                }
            }
          
            return View(appUser);
        }

        // GET: AppUsers/Edit/5
        [HttpGet]
        [Authorize(Roles = "User, Admin, Superadmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            //EDIT has been changed so now edting of user groups, and roles can be edited from the same page. Other edit routes are still available
          
            EditUserVM mainModel = new EditUserVM();
            //GROUP//
            ViewBag.UserId = id;
            AppUser appUser = await UserMgr.FindByIdAsync(id.ToString());

            var currentUser = await UserMgr.GetUserAsync(HttpContext.User);

            var model = new List<Group>();
            var userMembership = new List<GroupMember>();
            var currentCompanies = new List<CompanyMember>();
            List<CompanyMember> companyList = new List<CompanyMember>();
            if (User.IsInRole("Admin"))
            {
                companyList = CompanyList.CompanyMembers.Where(x => x.UserId == currentUser.Id).ToList();
            }

            try
            {
                var oldRole = await roleManager.FindByIdAsync(_context.UserRoles.FirstOrDefault(x => x.UserId == appUser.Id).RoleId.ToString());

                ViewBag.role = oldRole.Name;
            }
          
            catch(Exception e)
            {

            }
            foreach (var group in dataContext.Group)
            {
                userMembership = dataContext.GroupMembers.Where(x => (x.UserID == appUser.Id) && (x.GroupName == group.name)).ToList();
                model.Add(group);
                int index = userMembership.FindIndex(f => f.GroupName == group.name);
                if (index >= 0)
                {
                    group.IsSelected = true;
                }
                else
                {
                    group.IsSelected = false;
                }
            }
            mainModel.Groups = model;

            //ROLE//
            List<AppRole> roleList = new List<AppRole>();
            foreach (var role in roleManager.Roles)
            {               
                if (await UserMgr.IsInRoleAsync(appUser, role.Name))
                {
                    role.IsSelected = true;
                }
                else
                {
                    role.IsSelected = false;
                }
                roleList.Add(role);
            }
            mainModel.Roles = roleList;

            if (User.IsInRole("Admin") || User.IsInRole("Superadmin") || UserMgr.GetUserId(User) == id.ToString())
            {
                if (id == null)
                {
                    return NotFound();
                }

               

                if (appUser == null)
                {
                    return NotFound();
                }
                var tempList = new List<Company>();
                //List<CompanyMember> companyList = new List<CompanyMember>();

                //Populating the dropdown with companies
               
                    foreach (var company in CompanyList.Company)
                    {
                        if (User.IsInRole("Superadmin"))
                        {
                            mainModel.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString() });
                        }

                        else
                        {
                            if (companyList.Any(x => x.CompanyId == company.Id))
                            {
                                mainModel.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString() });
                            }
                        }

                    }

                foreach (var roles in roleManager.Roles)
                {
                    if ((roles.Name!="Superadmin" && User.IsInRole("Admin"))|| User.IsInRole("Superadmin"))
                    {
                        mainModel.RolesList.Add(new SelectListItem() { Text = roles.Name, Value = roles.Name });
                    }
                   
                }
                mainModel.User = appUser;



                currentCompanies = CompanyList.CompanyMembers.Where(x => x.UserId == appUser.Id).ToList();
                ViewBag.Companies = currentCompanies;
                TempData["UserId"] = id;
                TempData["UserFullName"] = appUser.FirstName + " " + appUser.LastName;
                appUser.TempUserName = appUser.UserName;
                return View(mainModel);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User, Admin, Superadmin")]
        public async Task<IActionResult> Edit(int id, EditUserVM model, string SelectedRole, List<int> Companies)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
                TempData["Company"] = currentUser.Company;
                var tempUser = await UserMgr.FindByIdAsync(id.ToString());
                //-GROUP-//
                if (dataContext.Group.Count() > 0)
                {
                    var groupList = dataContext.Group;
                    var memberList = dataContext.GroupMembers.ToList();
                    var tempList = new List<GroupMember>();
                    var delList = new List<GroupMember>();

                    foreach (var group in model.Groups)
                    {
                        if (group.IsSelected == true)
                        {
                            var tempMember = new GroupMember
                            {
                                UserID = tempUser.Id,
                                GroupID = group.id,
                                UserName = tempUser.UserName,
                                GroupName = group.name
                            };
                            if (dataContext.GroupMembers.Where(x => (x.UserID == tempUser.Id) && (x.GroupID == group.id)).Count() == 0)
                            {
                                dataContext.Add(tempMember);
                            }
                            await dataContext.SaveChangesAsync();
                        }
                        else if (group.IsSelected == false)
                        {
                            if (dataContext.GroupMembers.Where(x => (x.UserID == tempUser.Id) && (x.GroupID == group.id)).Count() > 0)
                            {
                                dataContext.Remove(dataContext.GroupMembers.FirstOrDefault(x => (x.UserID == tempUser.Id) && (x.GroupID == group.id)));
                            }
                            await dataContext.SaveChangesAsync();
                        }
                    }
                }

                //--ROLES--//
                //for (int i = 0; i < model.Roles.Count; i++)
                //{
                //AppRole role = await roleManager.FindByIdAsync(model.Roles.Id.ToString());
                ////roleManager.FindByIdAsync(model.Roles.Select(x => x.Id).ToString());


                var previousCompaniesList = CompanyList.CompanyMembers.Where(x => x.UserId == tempUser.Id);
                foreach (var companyMember in previousCompaniesList)
                {
                    CompanyList.CompanyMembers.Remove(companyMember);
                }
                
                foreach(var company in Companies)
                {
                    CompanyList.CompanyMembers.Add(new CompanyMember
                    {
                        CompanyId = company,
                        UserId = tempUser.Id
                    });
                }
                await CompanyList.SaveChangesAsync();
                ////PROTECTS USERS IN ROLE ADMIN
                //if (role.Name == "Superadmin" && !model.Roles[i].IsSelected && await UserMgr.IsInRoleAsync(tempUser, "Superadmin"))
                //{
                //    var tempRoleList = await UserMgr.GetUsersInRoleAsync(role.Name);
                //    if (tempRoleList.Count == 1)
                //    {
                //        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_AdminRemove;
                //        return RedirectToAction("Edit", "AppUsers", new { Id = id });
                //    }
                //}
                ////PROTECTS USERS IN ROLE SUPERADMIN
                //if (role.Name == "Admin" && !model.Roles[i].IsSelected && await UserMgr.IsInRoleAsync(tempUser, "Admin"))
                //{
                //    var tempRoleList = await UserMgr.GetUsersInRoleAsync(role.Name);
                //    if (tempRoleList.Count == 1)
                //    {
                //        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_AdminRemove;
                //        return RedirectToAction("Edit", "AppUsers", new { Id = id });
                //    }
                //}
                //

                IdentityResult ? result = null;
                try
                {
                    var oldRole = await roleManager.FindByIdAsync(_context.UserRoles.FirstOrDefault(x => x.UserId == tempUser.Id).RoleId.ToString());
                    result = await UserMgr.RemoveFromRoleAsync(tempUser, oldRole.Name);
                   
                }
                catch(Exception e)
                {

                }
                result = await UserMgr.AddToRoleAsync(tempUser, SelectedRole);

                //if (model.Roles[i].IsSelected && !(await UserMgr.IsInRoleAsync(tempUser, model.Roles[i].Name)))
                //{
                //    result = await UserMgr.AddToRoleAsync(tempUser, role.Name);
                //}
                //else if (!model.Roles[i].IsSelected && await UserMgr.IsInRoleAsync(tempUser, model.Roles[i].Name))
                //{
                //    result = await UserMgr.RemoveFromRoleAsync(tempUser, role.Name);
                //}
                //}

                //--USER INFO--//
                var compareUser = User.Identity.Name;
                var user = await UserMgr.FindByIdAsync(id.ToString());

                //This constructs the username from the users first and last names
                string userName = model.User.TempUserName;
                //This is supposed to remove any Ä's Ö's and Å's from the userName string
                byte[] tempBytes;
                tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(userName);
                string fixedUn = Encoding.UTF8.GetString(tempBytes);
                fixedUn = RemoveSpecialCharacters(fixedUn);
                model.User.UserName = fixedUn;

                //This is just an extra step to make sure the user is authorized to edit the account
                if (user.UserName == compareUser || (User.IsInRole("Admin") || User.IsInRole("Superadmin")))
                {
                    user.FirstName = model.User.FirstName;
                    user.LastName = model.User.LastName;
                    user.UserName = fixedUn;
                    user.Email = model.User.Email;
                    user.PhoneNumber = model.User.PhoneNumber;
                    
                    user.Company = model.Companies.First();
                    
                  
                    if (model.User.EmpStatus != "-1")
                    {
                        user.EmpStatus = model.User.EmpStatus;
                    }
                    else if (model.User.EmpStatus == "-1")
                    {
                        user.EmpStatus = "Active";
                    }
                }

                if (model.User.NewPassword == null)
                {
                    try
                    {
                        result = await UserMgr.UpdateAsync(user);
                        TempData["ActionResult"] = "User" + " " + model.User.UserName + " " + "edited!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        Console.WriteLine("Updating user information failed!");
                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_EditFail;
                        return View(model);
                    }
                }
                else if (model.User.NewPassword != null)
                {
                    try
                    {
                        //This changes the password if the user has edited it
                        var hashResult = UserMgr.PasswordHasher.HashPassword(model.User, model.User.NewPassword);
                        var token = await UserMgr.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await UserMgr.ResetPasswordAsync(user, token, model.User.NewPassword);
                        result = await UserMgr.UpdateAsync(user);

                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_EditSuccess;
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        Console.WriteLine("Updating user information with password failed!");
                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_EditFail;
                        return View(model);
                    }
                }
                else
                {
                    Console.WriteLine("You do not have the permission to edit this user!");
                    TempData["ActionResult"] = Resources.ActionMessages.ActionResult_EditFail;
                    return RedirectToAction(nameof(Index));
                }

                //--USER INFO END--//
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());

            TempData["UserId"] = id;
            TempData["UserFullName"] = tempUser.FirstName + " " + tempUser.LastName;

            return View(appUser);
        }

        [Authorize(Roles = "Admin, Superadmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appUser = await _context.Users.FindAsync(id);

            //Checking for user info in the DB
            if (dataContext.GroupMembers.Where(x => x.UserID == id).Count() != 0)
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_UserDeleteFailInfo;
                return RedirectToAction("Index");
            }
            if (dataContext.UserSkills.Where(x => x.UserID == id).Count() != 0)
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_UserDeleteFailInfo;
                return RedirectToAction("Index");
            }
            if (dataContext.SkillCourseMembers.Where(x => x.UserID == id).Count() != 0)
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_UserDeleteFailInfo;
                return RedirectToAction("Index");
            }

            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync();
            TempData["ActionResult"] = Resources.ActionMessages.ActionResult_UserDeleted;
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        [HttpGet]
        public async Task<IActionResult> EditGroupOfUser(int? id, string source)
        {
            ViewBag.UserId = id;
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["Source"] = source;
            var model = new List<Group>();
            var userMembership = new List<GroupMember>();

            foreach (var group in dataContext.Group)
            {
                userMembership = dataContext.GroupMembers.Where(x => (x.UserID == tempUser.Id) && (x.GroupName == group.name)).ToList();
                model.Add(group);
                int index = userMembership.FindIndex(f => f.GroupName == group.name);
                if (index >= 0)
                {
                    group.IsSelected = true;
                }
                else
                {
                    group.IsSelected = false;
                }
            }          
            return View(model);
        }
   
        //This stuff could probably use some rewriting, but I can't wrap my head around it anymore.
        [HttpPost]
        public async Task<IActionResult> EditGroupOfUser(List<Group> model, int? id)
        {
            var tempUser = await UserMgr.FindByIdAsync(id.ToString());
            var groupList = dataContext.Group;
            var memberList = dataContext.GroupMembers;
            var memberIndex = dataContext.GroupMembers.ToList();
            var tempList = new List<GroupMember>();
            var delList = new List<GroupMember>();

            for (int i = 0; i < model.Count; i++)
            {
                if (model[i].IsSelected)
                {
                    foreach (var member in memberList)
                    {
                        int index = memberIndex.FindIndex(f => (f.UserID == tempUser.Id) && (f.GroupName == model[i].name));
                        if (index == -1)
                        {
                            int tempIndex = tempList.FindIndex(f => (f.UserID == tempUser.Id) && (f.GroupName == model[i].name));
                            if (tempIndex == -1)
                            {
                                var tempMember = new GroupMember
                                {
                                    UserID = tempUser.Id,
                                    GroupID = model[i].id,
                                    UserName = tempUser.UserName,
                                    GroupName = model[i].name
                                };
                                tempList.Add(tempMember);
                                dataContext.Add(tempMember);
                            }
                        }                       
                    }
                }
                else if (!model[i].IsSelected)
                {
                    foreach (var member in memberList)
                    {
                        int index = memberIndex.FindIndex(f => (f.UserID == tempUser.Id) && (f.GroupName == model[i].name));
                        if (index >= 0)
                        {
                            if (member.GroupID == model[i].id && member.UserID == tempUser.Id && !delList.Contains(member))
                            {
                                dataContext.Remove(member);
                                delList.Add(member);
                            }
                        }
                    }                   
                }
                else
                {
                    continue;
                }

                await dataContext.SaveChangesAsync();
            }
            
            string? source = TempData["Source"].ToString();
            if (source == "edit")
            {
                return RedirectToAction("Edit", "AppUsers", new { Id = id });
            }
            else
            {
                return RedirectToAction("Index", "AppUsers");
            }
        } 
    }
}
