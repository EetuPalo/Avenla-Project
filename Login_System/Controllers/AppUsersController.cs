using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Login_System.ViewModels;
using System.Text;

namespace Login_System.Controllers
{
    public class AppUsersController : Controller
    {
        private readonly IdentityDataContext _context;
        private UserManager<AppUser> UserMgr { get; }
        private SignInManager<AppUser> SignInMgr { get; }
        private GroupsDataContext groupContext { get; }
        private GroupMembersDataContext memberContext { get; }
        private UserSkillsDataContext userSkillContext { get; }
        private SkillCourseMemberDataContext courseMemberContext { get; }

        public AppUsersController(IdentityDataContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, GroupsDataContext gContext, GroupMembersDataContext memContext, UserSkillsDataContext uskillCon, SkillCourseMemberDataContext cMemCon)
        {
            _context = context;
            UserMgr = userManager;
            SignInMgr = signInManager;
            groupContext = gContext;
            memberContext = memContext;
            userSkillContext = uskillCon;
            courseMemberContext = cMemCon;
        }

        // GET: AppUsers
        public async Task<IActionResult> Index(string searchString)
        {

            //  Stores the search input to preserve it in the text field
            if (!String.IsNullOrEmpty(searchString))
            {
                TempData["LastSearch"] = searchString;
            }

            var employees = from e in _context.Users select e;
            TempData["SearchString"] = Resources.Resources.Employee_Index_SearchPholder;
            TempData["SearchValue"] = null;
            if (!String.IsNullOrEmpty(searchString))
            {                
                employees = employees.Where(s => (s.UserName.Contains(searchString)) || (s.FirstName.Contains(searchString)) || (s.LastName.Contains(searchString)) || (s.Email.Contains(searchString)) || (s.PhoneNumber.Contains(searchString)));
                TempData["SearchValue"] = searchString;
            }           
            return View(await employees.ToListAsync());
        }

        // GET: AppUsers/Details/5
#nullable enable
        public async Task<IActionResult> Details(string? source, int? id, string? sourceId)
        {
            if (source == null)
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

            var appUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserId"] = id;
            TempData["UserFullName"] = tempUser.FirstName + " " + tempUser.LastName;

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

            var tempList = new List<string>();
            var courseList = new List<SkillCourseMember>();
            foreach (var groupMember in memberContext.GroupMembers.Where(x => x.UserID == id))
            {
                tempList.Add(groupMember.GroupName);
            }
            foreach (var courseMember in courseMemberContext.SkillCourseMembers.Where(x => x.UserID == id))
            {
                courseList.Add(courseMember);
            }
            model.UserGroups = tempList;
            model.UserCourses = courseList;

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("EMail, FirstName, LastName, PhoneNumber, Password, ConfirmPassword")] RegisterVM appUser)
        {
            if (ModelState.IsValid)
            {
                //This constructs the username from the users first and last names
                string userName = appUser.FirstName + appUser.LastName;
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
                    try
                    {
                        user = new AppUser
                        {
                            //we create a new user and set his credentials to the data received from the Register form.
                            UserName = fixedUn,
                            Email = appUser.EMail,
                            FirstName = appUser.FirstName,
                            LastName = appUser.LastName,
                            PhoneNumber = appUser.PhoneNumber
                        };
                        //we then create a new user through usermanager
                        IdentityResult result;
                        IdentityResult roleResult;
                        result = await UserMgr.CreateAsync(user, appUser.Password);
                        roleResult = await UserMgr.AddToRoleAsync(user, "User");
                    }
                    catch
                    {
                        //Console.WriteLine("An error occured but the account may have still been created. Check the account list!");
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
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.IsInRole("Admin") || UserMgr.GetUserId(User) == id.ToString())
            {
                if (id == null)
                {
                    return NotFound();
                }

                var appUser = await UserMgr.FindByIdAsync(id.ToString());

                if (appUser == null)
                {
                    return NotFound();
                }

                AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());

                TempData["UserId"] = id;
                TempData["UserFullName"] = tempUser.FirstName + " " + tempUser.LastName;
                appUser.TempUserName = tempUser.UserName;
                return View(appUser);
            }           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
       //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("FirstName, LastName, Email, NewPassword, ConfirmNewPassword, PhoneNumber, EmpStatus, TempUserName")] AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                var compareUser = User.Identity.Name;
                var user = await UserMgr.FindByIdAsync(id.ToString());

                //This constructs the username from the users first and last names
                string userName = appUser.TempUserName;
                //This is supposed to remove any Ä's Ö's and Å's from the userName string
                byte[] tempBytes;
                tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(userName);
                string fixedUn = Encoding.UTF8.GetString(tempBytes);
                fixedUn = RemoveSpecialCharacters(fixedUn);
                appUser.UserName = fixedUn;
                //This is just an extra step to make sure the user is authorized to edit the account
                if (user.UserName == compareUser || User.IsInRole("Admin"))
                {
                    user.FirstName = appUser.FirstName;
                    user.LastName = appUser.LastName;
                    user.UserName = fixedUn;
                    user.Email = appUser.Email;
                    user.PhoneNumber = appUser.PhoneNumber;
                    if (appUser.EmpStatus != "-1")
                    {
                        user.EmpStatus = appUser.EmpStatus;
                    }
                    else if (appUser.EmpStatus == "-1")
                    {
                        user.EmpStatus = "Active";
                    }
                }

                if (appUser.NewPassword == null)
                {
                    try
                    {
                        var result = await UserMgr.UpdateAsync(user);
                        TempData["ActionResult"] = "User" + " " + appUser.UserName + " " + "edited!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        Console.WriteLine("Updating user information failed!");
                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_EditFail;
                        return View(appUser);
                    }
                }
                else if (appUser.NewPassword != null)
                {
                    try
                    {
                        //This changes the password if the user has edited it
                        var hashResult = UserMgr.PasswordHasher.HashPassword(appUser, appUser.NewPassword);
                        var token = await UserMgr.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await UserMgr.ResetPasswordAsync(user, token, appUser.NewPassword);
                        var result = await UserMgr.UpdateAsync(user);

                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_EditSuccess;
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        Console.WriteLine("Updating user information with password failed!");
                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_EditFail;
                        return View(appUser);
                    }
                }                  
                else
                {
                    Console.WriteLine("You do not have the permission to edit this user!");
                    TempData["ActionResult"] = Resources.ActionMessages.ActionResult_EditFail;
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(appUser);
        }

        // GET: AppUsers/Delete/5
        //[Authorize(Roles = "Admin")]
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

        // POST: AppUsers/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appUser = await _context.Users.FindAsync(id);

            //Checking for user info in the DB
            if (memberContext.GroupMembers.Where(x => x.UserID == id).Count() != 0)
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_UserDeleteFailInfo;
                return RedirectToAction("Index");
            }
            if (userSkillContext.UserSkills.Where(x => x.UserID == id).Count() != 0)
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_UserDeleteFailInfo;
                return RedirectToAction("Index");
            }
            if (courseMemberContext.SkillCourseMembers.Where(x => x.UserID == id).Count() != 0)
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_UserDeleteFailInfo;
                return RedirectToAction("Index");
            }
            //

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

            foreach (var group in groupContext.Group)
            {
                userMembership = memberContext.GroupMembers.Where(x => (x.UserID == tempUser.Id) && (x.GroupName == group.name)).ToList();
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
            var groupList = groupContext.Group;
            var memberList = memberContext.GroupMembers;
            var memberIndex = memberContext.GroupMembers.ToList();
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
                                memberContext.Add(tempMember);
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
                                memberContext.Remove(member);
                                delList.Add(member);
                            }
                        }
                    }                   
                }
                else
                {
                    continue;
                }
                //var role = await groupContext.FindByIdAsync(model[i].id.ToString());
                await memberContext.SaveChangesAsync();

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
