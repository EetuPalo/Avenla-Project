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

        // Skill Filter
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
        // Certificate Filter
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
            return CertList;
        }
        // Group Filter
        public List<AppUser> GroupFilter(List<AppUser> GroupList, string Groups)
        {
            List<AppUser> tempList = new List<AppUser>();

            var groupQuery = from i in _context.Group
                             where i.name == Groups
                             select i;


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
            return GroupList;
        }

    }

}

