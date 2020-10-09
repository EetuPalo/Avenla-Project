using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Login_System.Models;
using Login_System.ViewModels;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.Rendering;

//using System.Web.Mvc;

namespace Login_System.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> UserMgr { get; }
        private SignInManager<AppUser> SignInMgr { get; }

        private readonly IdentityDataContext _context;
        private readonly RoleManager<AppRole> roleMgr;

        private readonly GeneralDataContext generalContext;

        private IMemoryCache _cache;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IdentityDataContext context, RoleManager<AppRole> roleManager, IMemoryCache memoryCache, GeneralDataContext genCon)
        {
            UserMgr = userManager;
            SignInMgr = signInManager;
            _context = context;
            roleMgr = roleManager;
            _cache = memoryCache;
            generalContext = genCon;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterVM();
  
            foreach (var company in generalContext.Company)
            {
                model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Name });
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind("EMail, FirstName, LastName, PhoneNumber, Company, CompanyList, Password, ConfirmPassword")] RegisterVM newUser)
        {
            if (ModelState.IsValid)
            {
                //This constructs the username from the users first and last names
                string userName = newUser.FirstName + newUser.LastName;
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

                AppUser user = await UserMgr.FindByNameAsync(fixedUn);
                if (user == null)
                {                    
                    user = new AppUser();

                    //we create a new user and set his credentials to the data received from the Register form.
                    user.UserName = fixedUn;
                    user.Email = newUser.EMail;
                    user.FirstName = newUser.FirstName;
                    user.LastName = newUser.LastName;
                    user.PhoneNumber = newUser.PhoneNumber;
                    user.EmpStatus = "Active";
                    user.Company = newUser.Company;
                    
                        //we then create a new user through usermanager
                    await UserMgr.CreateAsync(user, newUser.Password);
                    

                    //This is only for the case that DB roles is empty
                    if (await roleMgr.RoleExistsAsync("User"))
                    {
                        await UserMgr.AddToRoleAsync(user, "User");
                    }
                    else
                    {
                        AppRole userRole = new AppRole
                        {
                            Name = "User"
                        };
                        await roleMgr.CreateAsync(userRole);
                        await UserMgr.AddToRoleAsync(user, "User");
                    }
                    if (!await roleMgr.RoleExistsAsync("Superadmin"))
                    {
                        AppRole superAdminRole = new AppRole
                        {
                            Name = "Superadmin"
                        };
                        AppRole adminRole = new AppRole
                        {
                            Name = "Admin"
                        };
                        await roleMgr.CreateAsync(superAdminRole);
                        await roleMgr.CreateAsync(adminRole);
                        await UserMgr.AddToRoleAsync(user, "Superadmin");
                    }

                    ViewBag.Message = Resources.ActionMessages.ActionResult_UserCreated;
                    TempData["UserFullNames"] = user.FirstName + " " + user.LastName;
                    return View("Index");
                }
                else
                {
                    ViewBag.Message = Resources.ActionMessages.ActionResult_UserTaken;
                    return View("Index");
                }
            }
            //In case form is not valid, the companylist is populated again
            foreach (var company in generalContext.Company)
            {
                newUser.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Name });
            }

            return View(newUser);

        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM user)
        {
            var tempUser = UserMgr.Users.FirstOrDefault(x => x.Email == user.Email);
            if (tempUser != null)
            {
                
                var result = await SignInMgr.PasswordSignInAsync(tempUser.UserName, user.Password, false, false);
                if (result.Succeeded)
                {
                    /* After User logs in, that user's "Active" field's value is changed to 'Active' */
                    var appUser = _context.Users.FirstOrDefault(acc => acc.UserName == tempUser.UserName);

                    // check if person who is trying to log in is inactive
                    
                    if (appUser.EmpStatus == "Inactive") 
                    {
                        await SignInMgr.SignOutAsync();
                        ViewBag.Message = "Your account has been locked due inactivity";
                        return View();
                    }
                    
                    //Constructs a string from users first and last names to be shown in loginpartial
                    TempData["UserFullNames"] = appUser.FirstName + " " + appUser.LastName;
                    //Sends the userID in viewbag to the view
                    ViewBag.UserID = appUser.Id;
                    if(user.Password=="Koodaus1!" && user.Email =="admin@admin.fi")
                    {
                        return RedirectToAction("Edit", "Appusers", new { id = appUser.Id});
                    }
                    else
                    {
                        // Setting User roles

                        Company Comp = generalContext.Company.FirstOrDefault(x => x.Id == tempUser.Company);
                        //list users roles in companies
                        var userRoles = generalContext.CompanyMembers.Where(x => x.UserId == tempUser.Id).ToList();
                        //get role id:s used in remove and set in userroles table
                        var oldRoleId = userRoles.FirstOrDefault(x => x.CompanyId == tempUser.Company).CompanyRole;
                        var newRoleId = userRoles.FirstOrDefault(x => x.CompanyId == tempUser.Company).CompanyRole;
                        //get roles
                        var oldRole = _context.Roles.FirstOrDefault(x => x.Id == oldRoleId);
                        var role = _context.Roles.FirstOrDefault(x => x.Id == newRoleId);
                        await UserMgr.RemoveFromRoleAsync(tempUser, oldRole.Name);

                      
                        await UserMgr.AddToRoleAsync(tempUser, role.Name);
                    

                        //end of User role set
                        return RedirectToAction("Index", "Dashboard", new { id=appUser.Id});
                    }
              
                }
                else
                {
                    ViewBag.Result = result.ToString();
                }
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid username or password");
            return View(user);
        }        

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await SignInMgr.SignOutAsync();
            /* Basically the same as in the Login method but we find the current user that is logging out and set his status to 'Inactive'*/

            try
            {
                var appUser = _context.Users.FirstOrDefault(acc => acc.UserName == SignInMgr.UserManager.GetUserName(User));
                appUser.Active = "Inactive";
                _context.Users.Attach(appUser);
                _context.Entry(appUser).Property(x => x.Active).IsModified = true;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
       
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM user)
        {
        
       //checks for the emailaddress given by the user, if doesn't exist, error.
            if(UserMgr.Users.FirstOrDefault(x => x.Email == user.Email) == null)
            {
                 ModelState.AddModelError("", "Invalid Email");
            }
            //if no errors were given...
            if (ModelState.IsValid)
            {
                var tempUser = await UserMgr.FindByEmailAsync(user.Email);

                if (tempUser != null)
                {
                     var token = await UserMgr.GeneratePasswordResetTokenAsync(tempUser);

                    //This token is required to actually reset the password. Should be sent by email, or straight up used in next page
                    await UserMgr.SetAuthenticationTokenAsync(tempUser, "MyApp", "RefreshToken", token);

                    var passwordResetLink = Url.Action("ResetPassword", "Account", new { Email = user.Email, token = token }, Request.Scheme);

                    SendEmail(passwordResetLink, user);

                    return View("PasswordEmailSent");
                }
                return View(user);
            }
            return View(user);
        }
            
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            //Check for real token, instead of anything
            if (email == null || token == null)
            {
                ModelState.AddModelError("", "Invalid token");
            }

            else
            {
                return View("ResetPassword");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM user)
        {
            if (ModelState.IsValid)
            {
                //search user by email
                var tempUser = await UserMgr.FindByEmailAsync(user.Email);

                var token = await UserMgr.GetAuthenticationTokenAsync(tempUser, "MyApp", "RefreshToken");
                
                if (tempUser != null)
                { 
                    var result = await UserMgr.ResetPasswordAsync(tempUser, token, user.Password);

                    if (result.Succeeded)
                    {
                        await UserMgr.RemoveAuthenticationTokenAsync(tempUser, "MyApp", "RefreshToken");
                        return View("ResetPasswordConfirmation");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(user);
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

        public static void SendEmail(string link, ForgotPasswordVM user)
        {
            try
            {
                MailMessage mailMessage = new MailMessage("otto.kyllonen@hotmail.com", user.Email, "aihe", "Click this link to reset your password: " + link);

                SmtpClient smptClient = new SmtpClient();
                smptClient.Host = "smtp-mail.outlook.com";
                smptClient.Port = 587;
                smptClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smptClient.UseDefaultCredentials = false;
                System.Net.NetworkCredential credentials =
                new System.Net.NetworkCredential("", "");
                smptClient.EnableSsl = true;
                smptClient.Credentials = credentials;
                smptClient.Send(mailMessage);
            }

            catch(Exception ex)
            {
                 string error = ex.StackTrace.ToString();
            }
        }  
    }
}