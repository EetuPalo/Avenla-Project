using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Login_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Login_System.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Data;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin, Superadmin")]
    public class AdvancedSearchController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public AdvancedSearchController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }

        public async Task<IActionResult> Index(string?[] Skill, string Certificate, int? Groups, string Company, int?[] min, int?[] max, bool CompanyGroupSearch)
        {
            var model = new AdvancedSearchVM();
            var userList = new List<AppUser>();
            var companyIdList = new List<int>();
            var resultList = new List<(AppUser, List<int>,string)>();

            //filter lists
            var SkillList = new List<AppUser>();
            var GroupList = new List<AppUser>();
            var CertList = new List<AppUser>();
            var CompanyList = new List<AppUser>();
           
            //booleans to check if other filters have been used
            bool skillbool = Skill != null ? true : false;
            bool groupbool = Groups != null ?true :false;
            bool certificatebool = Certificate != null ? true :false;

            //Skill[0] = null;    
            var user = await UserMgr.GetUserAsync(HttpContext.User);

            model.CompanyGroupSearch = CompanyGroupSearch;
            List<int> usercompanies = new List<int>();
            foreach (var item in _context.CompanyMembers.Where(x => x.UserId== user.Id).ToList())
            {
                usercompanies.Add(_context.Company.FirstOrDefault(x => x.Id == item.CompanyId).Id);
            }
            model.adminCompanyIds = usercompanies;
            
            var companygrouplist = new List<CompanyGroups>();
            foreach(var id in _context.CompanyGroupMembers.Where(x=> x.CompanyId == user.Company).Select(x=> x.CompanyGroupId))
            {
                var group = _context.CompanyGroups.FirstOrDefault(x=> x.CompanyGroupId == id);
                if (!companygrouplist.Contains(group))
                {
                    companygrouplist.Add(group);
                }
            }

            //ViewBag.CurrentCompany = userCompanies;

            //Populating certificate dropdown with certificates
            if (User.IsInRole("Superadmin"))
            {
                foreach (var certificate in _context.Certificates)
                {
                    model.CertificateList.Add(new SelectListItem() { Text = certificate.Name, Value = certificate.Name });
                }
            }
            else
            {
                foreach (var companygrp in companygrouplist)
                {
                    foreach (var certofgroup in _context.CompanyGroupCertificates.Where(x => ((x.CompanyGroupId == companygrp.CompanyGroupId) && (x.CompanyId == user.Company)) || ((x.CompanyGroupId == companygrp.CompanyGroupId && (x.CompanyId == (int?)null)))))
                    {
                        var cert = _context.Certificates.FirstOrDefault(x => x.Id == certofgroup.CertificateId);
                        model.CertificateList.Add(new SelectListItem() { Text = cert.Name, Value = cert.Name});
                    }
                }
            }
            //populating skill dropdown with skills
            if (User.IsInRole("Superadmin"))
            {
                foreach(var skill in _context.Skills)
                {
                    model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Skill });
                }
            }
            else
            {
                foreach (var companygrp in companygrouplist)
                {
                    foreach (var skillofgroup in _context.CompanyGroupSkills.Where(x => ((x.CompanyGroupId == companygrp.CompanyGroupId) && (x.CompanyId == user.Company)) || ((x.CompanyGroupId == companygrp.CompanyGroupId && (x.CompanyId == (int?)null)))))
                    {
                        var skill = _context.Skills.FirstOrDefault(x => x.Id == skillofgroup.SkillId);
                        model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Skill });
                    }
                }

            }
      

            if (User.IsInRole("Superadmin"))
            {
                foreach (var companies in _context.Company)
                {
                    model.CompanyList.Add(new SelectListItem() { Text = companies.Name, Value = companies.Id.ToString()});
                }

                // Populating group dropdown with groups
                foreach (var group in _context.Group)
                {
                    model.GroupList.Add(new SelectListItem() { Text = group.name, Value = group.id.ToString() });
                }

            }
            else if (User.IsInRole("Admin"))
            {
                companyIdList = _context.CompanyMembers.Where(x => x.UserId == user.Id).Select(x => x.CompanyId).ToList();
                foreach(var id in companyIdList)
                {
                    foreach (var company in _context.Company.Where(x=> x.Id == id))
                    {
                        model.CompanyList.Add(new SelectListItem() { Text = company.Name, Value = company.Id.ToString()});  
                    }

                    foreach(var group in _context.Group)
                    {
                        model.GroupList.Add(new SelectListItem() { Text = group.name, Value = group.id.ToString() });
                    }
                }


            }
            
            if(Skill.Length> 0)
            {
                switch (Skill[0])
                {
                    case null:
                        skillbool = false;
                        break;

                    default:
                        SkillList = SkillFilter(SkillList, Skill, min, max);
                        if (userList.Count == 0)
                        {
                            userList.AddRange(SkillList);
                        }

                        break;
                }
            }
            
            switch (Certificate)
            {
                case null:
                    break;
                default:
                    CertificateFilter(CertList, Certificate);
                    if (userList.Count == 0 && !skillbool)
                    {
                        userList.AddRange(CertList);
                    }
                    else
                    {
                        userList = userList.Intersect(CertList).ToList();
                    }
                   
                    break;
            }

            switch (Groups)
            {
                case null:
                    break;
                default:
                    GroupFilter(GroupList, Groups);
                    if (userList.Count == 0 && (!skillbool && !certificatebool))
                    {
                        userList.AddRange(GroupList);
                    }
                    else
                    {
                        userList = userList.Intersect(GroupList).ToList();
                    }
               
                    break;
            }

            switch (Company)
            {
                case null:
                    break;
                default:
                    CompanyFilter(CompanyList, Company);
                    if (userList.Count == 0&& (!skillbool && !certificatebool && !groupbool))
                    {
                        userList.AddRange(CompanyList);
                    }
                    else
                    {
                        userList = userList.Intersect(CompanyList).ToList();
                    }
                    break;
            }
         

            foreach (var applicableUser in userList)
            {
                var companyGroup = _context.CompanyGroupMembers.FirstOrDefault(x => x.CompanyId == user.Company).CompanyGroupId;
                List<int> ids = _context.CompanyMembers.Where(x=>x.UserId == applicableUser.Id).Select(x=> x.CompanyId).ToList();

                foreach(var id in ids)
                {
                    var companyName = _context.Company.FirstOrDefault(x => x.Id == id).Name;
                    var list = _context.CompanyGroupMembers.Where(x => x.CompanyId == id).Select(x => x.CompanyGroupId).ToList();
                    if (list.Contains(companyGroup))
                    {
                        resultList.Add((applicableUser, ids, companyName));
                    }
                }
            }

            model.Users = resultList;
            //model.differentCompanyUsers = differentCompanyUsers;
            return View(model);
        }

   

        // Skill Filter
        public List<AppUser> SkillFilter(List<AppUser> SkillList, string[] Skill, int?[] min, int?[] max)
        {
            var index = 0;
            IEnumerable<AppUser> usr;
      
            foreach (var skillInList in Skill)
            {
                var currentSkillList = new List<AppUser>();
                var skillQuery = from m in _context.UserSkills
                                 where m.SkillName == skillInList
                                 select m;

                foreach (var items in skillQuery.Where(x => x.SkillName == skillInList))
                {
                    var skillQuerySecond = (from t in _context.UserSkills
                                           group t by t.UserID into g
                                           select new { UserID = g.Key, Date = g.Max(t => t.Date) }).ToList();

                    foreach (var it in skillQuerySecond.Where(x => x.Date == items.Date))
                    {
                        //foreach(var ploo in _context.UserSkills) { }
                       
                        foreach (AppUser user in UserMgr.Users.Where(x => x.Id == items.UserID))
                        {
                            
                            if (min[index] == null && max[index] == null)
                            {
                                if (!currentSkillList.Contains(user))
                                {
                                    currentSkillList.Add(user);
                                }
                            }
                            else
                            {
                                if ((items.SkillLevel >= min[index] && items.SkillLevel <= max[index]) || (min[index] == null && items.SkillLevel <= max[index]) || (max[index] == null && items.SkillLevel >= min[index]))
                                {
                                    if (!currentSkillList.Contains(user))
                                    {
                                        currentSkillList.Add(user);
                                    }
                                }
                            }
                        }
                    }
                 }
                if (SkillList.Count == 0)
                {
                    SkillList.AddRange(currentSkillList);
                    currentSkillList.Clear();
                 
                }
                else
                {
                    usr = SkillList.AsQueryable().Intersect(currentSkillList);
                    
                    SkillList = usr.ToList();
                    currentSkillList.Clear();
                }
                index++;
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
        public List<AppUser> GroupFilter(List<AppUser> GroupList, int? Groups)
        {
            List<AppUser> tempList = new List<AppUser>();

            var groupQuery = from i in _context.Group
                             where i.id == Groups
                             select i;

            foreach (var Uname in _context.GroupMembers.Where(x => x.GroupID == Groups))
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

        //company filter
        private List<AppUser> CompanyFilter(List<AppUser> CompanyList, string Company)
        {
            List<AppUser> tempList = new List<AppUser>();

            /*var groupQuery = from i in _context.Company
                             where i.name == Company
                             select i;*/

            foreach (var Uname in _context.CompanyMembers.Where(x => x.CompanyId == Int32.Parse(Company)))
            {
                foreach (AppUser user in UserMgr.Users.Where(x => x.Id == Uname.UserId))
                {
                    //This is to prevent duplicates
                    if (!CompanyList.Contains(user))
                    {
                        CompanyList.Add(user);
                    }
                }
            }
            return CompanyList;
        }
    }
}

