using Login_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login_System.Controllers
{
    //Commented for ease of access
    //Add role "Admin" to test authorization
    [Authorize(Roles= "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<AppRole> roleManager;
        private readonly UserManager<AppUser> userManager;
        public AdminController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        //Creates the new role, checks if there are any errors, and if successful, redirects the user to the home page.
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRole roleModel)
        {
            if (ModelState.IsValid)
            {
                AppRole identityRole = new AppRole
                {
                    Name = roleModel.RoleName
                };
                IdentityResult result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    TempData["ActionResult"] = Resources.ActionMessages.ActionResult_RoleSuccessful;
                    return RedirectToAction("ListRoles", "Admin");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }  
            //If the result is not successful, the role creation view is returned with the errors shown.
            return View(roleModel);
        }

        public IActionResult ListRoles(string searchString)
        {
            var roles = from c in roleManager.Roles select c;
            TempData["SearchValue"] = null;
            if (!String.IsNullOrEmpty(searchString))
            {
                roles = roles.Where(s => s.Name.Contains(searchString));
                TempData["SearchValue"] = searchString;
            }
            return View(roles.ToList());
        }
        [HttpGet]
        //The id of a specific role is passed to this method.
        public async Task<IActionResult> EditRole(string id)
        {

            var role = await roleManager.FindByIdAsync(id);

            if(role==null)
            {
                return RedirectToAction("Error");
            }

            var model = new EditRole
            {
                Id = role.Id.ToString(),
                RoleName = role.Name,
                OldName = role.Name
            };

            foreach(var user in userManager.Users)
            {
                if(await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRole roleModel)
        {
            if (roleModel.OldName != "Admin" && roleModel.OldName != "User")
            {
                var role = await roleManager.FindByIdAsync(roleModel.Id);

                if (role == null)
                {
                    return RedirectToAction("Error");
                }
                else
                {
                    role.Name = roleModel.RoleName;
                    var result = await roleManager.UpdateAsync(role);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(roleModel);
                }
            }
            else if (roleModel.OldName == "Admin" || roleModel.OldName == "User")
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_NoPermissionRole;
                return RedirectToAction("ListRoles");
            }
            else
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_GeneralException;
                return RedirectToAction("ListRoles");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return RedirectToAction("Error");
            }

            var model = new List<UserRole>();

            foreach (var user in userManager.Users)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id.ToString(),
                    UserName = user.UserName
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRole.IsSelected = true;
                }
                else
                {
                    userRole.IsSelected = false;
                }

                model.Add(userRole);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRole> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            //Protects from removing all admin users
            if (role.Name == "Admin")
            {
                int counter = 0;
                foreach (var user in model.Where(x => x.IsSelected))
                {
                    counter++;
                }
                
                if (counter == 0)
                {
                    TempData["ActionResult"] = Resources.ActionMessages.ActionResult_AdminRemove;
                    return RedirectToAction("ListRoles");
                }
            }
            //

            if (role == null)
            {
                return RedirectToAction("Error");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result = null;

                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }

        [HttpGet]
        public async Task<IActionResult> EditRoleOfUser(int? id, string source)
        {
            ViewBag.UserId = id;
            AppUser tempUser = await userManager.FindByIdAsync(id.ToString());
            TempData["Source"] = source;

            var model = new List<AppRole>();

            foreach (var role in roleManager.Roles)
            {
                model.Add(role);
                if (await userManager.IsInRoleAsync(tempUser, role.Name))
                {
                    role.IsSelected = true;
                }
                else
                {
                    role.IsSelected = false;
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoleOfUser(List<AppRole> model, int? id)
        {
            var tempUser = await userManager.FindByIdAsync(id.ToString());

            for (int i = 0; i < model.Count; i++)
            {
                AppRole role = await roleManager.FindByIdAsync(model[i].Id.ToString());

                //PROTECTS USERS IN ROLE
                if (role.Name == "Admin")
                {
                    //var tempList = new List<AppRole>();
                    var tempList = await userManager.GetUsersInRoleAsync(role.Name);
                    if (tempList.Count == 1)
                    {
                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_AdminRemove;
                        return RedirectToAction("Edit", "AppUsers", new { Id = id });
                    }
                }
                //

                IdentityResult result = null;

                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(tempUser, model[i].Name)))
                {
                    result = await userManager.AddToRoleAsync(tempUser, role.Name);
                }
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(tempUser, model[i].Name))
                {
                    result = await userManager.RemoveFromRoleAsync(tempUser, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("Edit","AppUsers", new { Id = id });
                }
            }

            string source = TempData["Source"].ToString();
            if (source == "edit")
            {
                return RedirectToAction("Edit", "AppUsers", new { Id = id });
            }
            else
            {
                return RedirectToAction("Index", "AppUsers");
            }

        }

        public async Task<IActionResult> Delete(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if(role == null)
            {
                return RedirectToAction("Error");
            }
            //This is to prevent the User and Admin roles for being deleted
            else if(role.Name == "Admin" || role.Name == "User")
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_RoleDeleteException;
                return RedirectToAction(nameof(ListRoles));
            }
            var model = new DeleteRole
            {         
                Id = role.Id.ToString(),
                RoleName = role.Name
            };
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ActionResult"] = Resources.ActionMessages.ActionResult_RoleDeleteFail;
                return RedirectToAction(nameof(ListRoles));
            }
            else
            {
                AppRole tempRole = await roleManager.FindByIdAsync(id.ToString());

                if (tempRole.Name != "Admin" && tempRole.Name != "User")
                {              
                    var result = await roleManager.DeleteAsync(tempRole);
                    if (result.Succeeded)
                    {
                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_RoleDeleteSuccess;
                        return RedirectToAction(nameof(ListRoles));
                    }
                    else if (!result.Succeeded)
                    {
                        TempData["ActionResult"] = Resources.ActionMessages.ActionResult_RoleDeleteException;
                        return RedirectToAction(nameof(ListRoles));
                    }
                    return RedirectToAction(nameof(ListRoles));
                }
                else
                {
                    TempData["ActionResult"] = Resources.ActionMessages.ActionResult_RoleDeleteException;
                    return RedirectToAction(nameof(ListRoles));
                }
               
            }
        }


    }
}
