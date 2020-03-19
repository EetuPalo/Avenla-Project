using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Login_System.Controllers
{
    public class UserCertificatesController : Controller
    {
        private readonly UserCertificateDataContext _context;
        private readonly CertificateDataContext certificateContext;
        private UserManager<AppUser> UserMgr;

        public UserCertificatesController(UserCertificateDataContext context, CertificateDataContext certCon, UserManager<AppUser> userManager)
        {
            _context = context;
            certificateContext = certCon;
            UserMgr = userManager;
        }

        // GET: UserCertificates
        public async Task<IActionResult> Index(int? id, string searchString, string source)
        {
            if (id == null)
            {
                id = Convert.ToInt32(UserMgr.GetUserId(User));
            }

            TempData["UserID"] = id;
            if (source != null)
            {
                TempData["Source"] = source;
            }
            else
            {
                TempData["Source"] = "appuser";
            }

            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            TempData["UserName"] = tempUser.FirstName + " " + tempUser.LastName;

            var certificates = from c in _context.UserCertificates.Where(c => c.UserID == id) select c;
            if (!String.IsNullOrEmpty(searchString))
            {
                certificates = certificates.Where(s => (s.CertificateName.Contains(searchString))|| (s.Organization.Contains(searchString)));
            }
            return View(await certificates.ToListAsync());
        }

        // GET: UserCertificates/Create
        public async Task<IActionResult> Create(int id)
        {
            AppUser tempUser = await UserMgr.FindByIdAsync(id.ToString());
            var certificates = certificateContext.Certificates.ToList();

            //This prevents the user from adding duplicates
            foreach (var userCertificate in _context.UserCertificates.Where(x => x.UserID == id))
            {
                foreach (var certificate in certificateContext.Certificates.Where(x => x.Id == userCertificate.CertificateID))
                {
                    certificates.Remove(certificate);
                }
            }
            var model = new UserCertificate
            {
                UserID = id,
                UserName = tempUser.UserName,
            };
            model.CertificateList = certificates.Select(x => new SelectListItem
            {
               Value = x.Name,
               Text = x.Name
            });
            return View(model);
        }

        // POST: UserCertificates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserID,UserName,CertificateName,GrantDate")] UserCertificate userCertificate)
        {
            if (ModelState.IsValid)
            {
                var certificate = certificateContext.Certificates.Where(c => c.Name == userCertificate.CertificateName).ToList();
                if (certificate.Count == 1)
                {
                    userCertificate.CertificateID = certificate[0].Id;
                    userCertificate.Organization = certificate[0].Organization;
                }
                else
                {
                    TempData["ActionResult"] = "An exception occured.";
                    return RedirectToAction(nameof(Index), "UserCertificates", new { id = userCertificate.UserID });
                }

                //Adding the complete model to the DB
                _context.Add(userCertificate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "UserCertificates", new { id = userCertificate.UserID });
            }
            return View(userCertificate);
        }       

        // GET: UserCertificates/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCertificate = await _context.UserCertificates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userCertificate == null)
            {
                return NotFound();
            }

            return View(userCertificate);
        }

        // POST: UserCertificates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userCertificate = await _context.UserCertificates.FindAsync(id);
            _context.UserCertificates.Remove(userCertificate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), "UserCertificates", new { id = userCertificate.UserID});
        }

        private bool UserCertificateExists(int id)
        {
            return _context.UserCertificates.Any(e => e.Id == id);
        }
    }
}
