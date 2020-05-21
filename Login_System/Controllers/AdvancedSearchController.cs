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

namespace Login_System.Controllers
{
    public class AdvancedSearchController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public AdvancedSearchController(GeneralDataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserMgr = userManager;
        }
        public async Task<IActionResult> Index(string SkillList, int? min, int? max, string Skill, string Certificate, string Groups)
        {
            
            var model = new AdvancedSearchVM();
            var userList = new List<AppUser>();
        

       
            //TODO:
            //SearchString should be replaced with a dropdown that's populated with all the skills that are in the database
            //Later there should also be an option to select multiple skills for the search
            //Next to add similar functions for:
            //Filter by skill level (both min and max)
           
            //Note that all these different forms need to be available at the same time and selected/deselected as the user wants

            if(!string.IsNullOrEmpty(Skill))
            {
                userList = SkillFilter(userList, Skill, min, max);
            }

      
            if (!string.IsNullOrEmpty(Groups))
            {
                userList = GroupFilter(userList, Groups);

            }

            if (!string.IsNullOrEmpty(Certificate))
            {
                userList = CertificateFilter(userList, Certificate);
            }


            /*if (!string.IsNullOrEmpty(SkillList))
            {
                items = items.Where(x => x.Skill == SkillList);
            }*/


            // Search by Group



            // Search by Certificates

            model.Users = userList;


            // List Certificates

            var tempList = new List<Certificate>();

            //Populating the dropdown with certificates
            foreach (var certificate in _context.Certificates)
            {
                model.CertificateList.Add(new SelectListItem() { Text = certificate.Name, Value = certificate.Name });
            }

            // List Groups

            var tempGroups = new List<Group>();

            // Populating the dropdown with groups
            foreach (var group in _context.Group)
            { 
            model.GroupList.Add(new SelectListItem() { Text = group.name, Value = group.name });
            }

            // List Skills

            var tempSkills = new List<Group>();

            // Populating the dropdown with skills
            foreach (var skill in _context.Skills)
            {
                model.SkillList.Add(new SelectListItem() { Text = skill.Skill, Value = skill.Skill });
            }

            // Return
            return View(model);

        }

        public List<AppUser> SkillFilter(List<AppUser> userList, string Skill, int? min, int? max)
        {
            IQueryable<string> SkillQuery = from s in _context.Skills
                                            orderby s.Skill
                                            select s.Skill;
            var items = from m in _context.Skills
                        select m;

            items = items.Where(s => s.Skill.Contains(Skill));

            foreach (var skill in _context.UserSkills.Where(x => x.SkillName == Skill))
            {
                foreach (AppUser user in UserMgr.Users.Where(x => x.Id == skill.UserID))
                {
                    if (min == null && max == null)
                    {
                        //This is to prevent duplicates
                        if (!userList.Contains(user))
                        {
                            userList.Add(user);
                        }
                    }

                    else
                    {
                        //checks that if both filters have values, or only if either one has and adds to userList based on that
                        if ((skill.SkillLevel >= min && skill.SkillLevel <= max) || (min == null && skill.SkillLevel <= max) || (max == null && skill.SkillLevel >= min))
                        {
                            //This is to prevent duplicates
                            if (!userList.Contains(user))
                            {
                                userList.Add(user);
                            }
                        }
                    }

                }
            }
            
            return userList;
        }

        public List<AppUser> GroupFilter(List<AppUser> userList, string Groups)
        {
            List<AppUser> tempList = new List<AppUser>();

            IQueryable<string> GroupQuery = from s in _context.GroupMembers
                                            orderby s.Uname
                                            select s.GroupName;
            var groups = from m in _context.GroupMembers
                         select m;

            //if (!string.IsNullOrEmpty(Groups))
            //{
                groups = groups.Where(s => s.GroupName.Contains(Groups));

                foreach (var Uname in _context.GroupMembers.Where(x => x.GroupName == Groups))
                {
                    foreach (AppUser user in UserMgr.Users.Where(x => x.Id == Uname.UserID))
                    {
                        //This is to prevent duplicates
                        if (!tempList.Contains(user))
                        {
                            tempList.Add(user);
                        }
                    }
                }

            //}
            // this is only if only filter used is group filter
                if (userList.Count == 0)
                {
                    return tempList;
                }

                else
                {
                    for (int i = userList.Count() - 1; i >= 0; i--)
                    {
                        if (!tempList.Contains(userList[i]))
                        {
                            userList.Remove(userList[i]);
                        }
                    }
            }

            return userList;
        }
        public List<AppUser> CertificateFilter(List<AppUser> userList, string Certificate)
        {
            List<AppUser> tempList = new List<AppUser>();
         
          
            IQueryable<string> CertificateQuery = from s in _context.UserCertificates
                                                  orderby s.UserName
                                                  select s.CertificateName;
            var certificates = from m in _context.UserCertificates
                               select m;

            //if (!string.IsNullOrEmpty(Certificate))
            //{
                certificates = certificates.Where(s => s.CertificateName.Contains(Certificate));

                foreach (var UserName in _context.UserCertificates.Where(x => x.CertificateName == Certificate))
                {
                    foreach (AppUser user in UserMgr.Users.Where(x => x.Id == UserName.UserID))
                    {
                        if (!tempList.Contains(user))
                        {
                            tempList.Add(user);
                        }
                    }
                }
            //}

            // this is only if only filter used is Certificate filter
            if (userList.Count==0)
            {
                return tempList;
            }

            else
            {
                for (int i = userList.Count() - 1; i >= 0; i--)
                {
                    if (!tempList.Contains(userList[i]))
                    {
                        userList.Remove(userList[i]);
                    }
                }
            }
           
            return userList;
        }

    }
}