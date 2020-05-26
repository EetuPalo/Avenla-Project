using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Login_System.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Resources;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdvancedSearchController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public AdvancedSearchController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }

        public async Task<IActionResult> Index(string Skill, string Certificate, string Groups, int? min, int? max)
        {

            var model = new AdvancedSearchVM();
            var userList = new List<AppUser>();
            var SkillList = new List<AppUser>();
            var GroupList = new List<AppUser>();
            var CertList = new List<AppUser>();
          

            var user = await UserMgr.GetUserAsync(HttpContext.User);
            ViewBag.CurrentCompany = user.Company;


            //Populating the dropdown with certificates
            foreach (var certificate in _context.Certificates)
            {
                model.CertificateList.Add(new SelectListItem() { Text = certificate.Name, Value = certificate.Name });
            }

            // Populating the dropdown with groups
            foreach (var group in _context.Group)
            {
                model.GroupList.Add(new SelectListItem() { Text = group.name, Value = group.name });
            }

            // Populating the dropdown with skills
            foreach (var skill in _context.Skills)
            {
                model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Skill });
            }



            //SkillFilter(userList, Skill, min, max);
            //CertificateFilter(userList, Certificate);
            //GroupFilter(userList, Groups);

            switch (Skill)
            {
                case null:
                    break;
                default:
                    SkillFilter(SkillList, Skill, min, max);
                    break;
            }

            switch (Certificate)
            {
                case null:
                    break;
                default:
                    CertificateFilter(CertList, Certificate);
                    break;
            }

            switch (Groups)
            {
                case null:
                    break;
                default:
                    GroupFilter(GroupList, Groups);
                    break;
            }

            var SkillCertList = new List<AppUser>();
            
            if (SkillList.Count == 0 && CertList.Count == 0)
            {
                    userList = GroupList;
            }

            if (CertList.Count == 0 && GroupList.Count == 0)
            {      
                    userList = SkillList;
            }

            if(GroupList.Count == 0 && SkillList.Count == 0)
            {
                    userList = CertList; 
            }
            if(GroupList.Count > 0 && SkillList.Count > 0 && CertList.Count > 0)
            {
                SkillCertList = SkillList.Intersect(CertList).ToList();
                userList = SkillCertList.Intersect(GroupList).ToList();
            }
            if (GroupList.Count == 0 && SkillList.Count > 0 && CertList.Count > 0) 
            {
                userList = SkillList.Intersect(CertList).ToList();
            }
            if (GroupList.Count > 0 && SkillList.Count == 0 && CertList.Count > 0)
            {
                userList = CertList.Intersect(GroupList).ToList();
            }
            if (GroupList.Count > 0 && SkillList.Count > 0 && CertList.Count == 0)
            {
                userList = GroupList.Intersect(SkillList).ToList();
            }

            userList = userList.OrderBy(x=> x.Company != user.Company).ToList();
            model.Users = userList;
            

            return View(model);
        }


        public List<AppUser> SkillFilter(List<AppUser> SkillList, string Skill, int? min, int? max)
        {

            var skillQuery = from m in _context.UserSkills
                             where m.SkillName == Skill
                             select m;

            foreach (var items in skillQuery.Where(x => x.SkillName == Skill))
            {
                var skillQuerySecond = from t in _context.UserSkills
                                       group t by t.UserID into g
                                       select new { UserID = g.Key, Date = g.Max(t => t.Date) };

                foreach (var it in skillQuerySecond.Where(x => x.Date == items.Date))
                {

                    foreach (AppUser user in UserMgr.Users.Where(x => x.Id == items.UserID))
                    {

                        if (min == null && max == null)
                        {
                            if (!SkillList.Contains(user))
                            {
                                SkillList.Add(user);
                            }
                        }
                        else
                        {
                            if ((items.SkillLevel >= min && items.SkillLevel <= max) || (min == null && items.SkillLevel <= max) || (max == null && items.SkillLevel >= min))
                            {
                                if (!SkillList.Contains(user))
                                {
                                    SkillList.Add(user);
                                }
                            }
                        }

                    }
                }
            }
            return SkillList;
        }
        public List<AppUser> CertificateFilter(List<AppUser> CertList, string Certificate)
        {
            List<AppUser> tempList = new List<AppUser>();

            var certQuery = from s in _context.Certificates
                            where s.Name == Certificate
                            select s;

            foreach (var UserName in _context.UserCertificates.Where(x => x.CertificateName == Certificate))
            {
                foreach (AppUser user in UserMgr.Users.Where(x => x.Id == UserName.UserID))
                {
                    if (!CertList.Contains(user))
                    {
                        CertList.Add(user);
                    }
                }
            }

            

            // this is only if only filter used is Certificate filter
            //if (userList.Count == 0)
            //{
            //    return userList;
            //}

            //else
            //{
            //    for (int i = userList.Count() - 1; i >= 0; i--)
            //    {
            //        if (!tempList.Contains(userList[i]))
            //        {
            //            userList.Remove(userList[i]);
            //        }
            //    }
            //}
            

            return CertList;
        }
        public List<AppUser> GroupFilter(List<AppUser> GroupList, string Groups)
        {
            List<AppUser> tempList = new List<AppUser>();

            var groupQuery = from i in _context.Group
                             where i.name == Groups
                             select i;

            //if (!string.IsNullOrEmpty(Groups))
            //{
            foreach (var Uname in _context.GroupMembers.Where(x => x.GroupName == Groups))
            {
                foreach (AppUser user in UserMgr.Users.Where(x => x.Id == Uname.UserID))
                {
                    //This is to prevent duplicates
                    if (!GroupList.Contains(user))
                    {
                        GroupList.Add(user);
                    }
                }
            }

            //}
            // this is only if only filter used is group filter
            //if (userList.Count == 0)
            //{
            //    return tempList;
            //}

            //else
            //{
            //    for (int i = userList.Count() - 1; i >= 0; i--)
            //    {
            //        if (!tempList.Contains(userList[i]))
            //        {
            //            userList.Remove(userList[i]);
            //        }
            //    }
            //}

            return GroupList;
        }

    }

}

//TODO:
//SearchString should be replaced with a dropdown that's populated with all the skills that are in the database
//Later there should also be an option to select multiple skills for the search
//Next to add similar functions for:
//Filter by skill level (both min and max)

//Note that all these different forms need to be available at the same time and selected/deselected as the user wants

//            if (!string.IsNullOrEmpty(Skill))
//            {
//                userList = SkillFilter(userList, Skill, min, max);
//            }


//            if (!string.IsNullOrEmpty(Groups))
//            {
//                userList = GroupFilter(userList, Groups);

//            }

//            if (!string.IsNullOrEmpty(Certificate))
//            {
//                userList = CertificateFilter(userList, Certificate);
//            }


//            /*if (!string.IsNullOrEmpty(SkillList))
//            {
//                items = items.Where(x => x.Skill == SkillList);
//            }*/


//            // Search by Group



//            // Search by Certificates

//            model.Users = userList;


//            // List Certificates

//            var tempList = new List<Certificate>();

//            //Populating the dropdown with certificates
//            foreach (var certificate in _context.Certificates)
//            {
//                model.CertificateList.Add(new SelectListItem() { Text = certificate.Name, Value = certificate.Name });
//            }

//            // List Groups

//            var tempGroups = new List<Group>();

//            // Populating the dropdown with groups
//            foreach (var group in _context.Group)
//            { 
//            model.GroupList.Add(new SelectListItem() { Text = group.name, Value = group.name });
//            }

//            // List Skills

//            var tempSkills = new List<Group>();

//            // Populating the dropdown with skills
//            foreach (var skill in _context.Skills)
//            {
//                model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Skill });
//            }vv

//            // Return
//            return View(model);

//        }


