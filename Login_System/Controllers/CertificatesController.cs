using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin, Superadmin, User")]
    public class CertificatesController : Controller
    {
        private readonly GeneralDataContext _context;
        private UserManager<AppUser> UserMgr { get; }

        public CertificatesController(GeneralDataContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Index(string searchString)
        {
            var certificates = from c in _context.Certificates select c;
            TempData["SearchString"] = Resources.Certificates.Index_Search;
            TempData["SearchValue"] = null;
            if (!String.IsNullOrEmpty(searchString))
            {
                certificates = certificates.Where(s => (s.Name.Contains(searchString)) || (s.Organization.Contains(searchString)));
                TempData["SearchValue"] = searchString;
            }
            return View(await certificates.ToListAsync());
        }
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        [Authorize(Roles = "Admin, Superadmin, User")]
        public IActionResult Create()
        {
            return View();
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Superadmin, User")]
        public async Task<IActionResult> Create([Bind("Id,Name,Organization")] Certificate certificate)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
                _context.Add(certificate);
                await _context.SaveChangesAsync();
                

                return RedirectToAction(nameof(Index));
            }
            return View(certificate);
        }
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }
            return View(certificate);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Organization")] Certificate certificate)
        {
            if (id != certificate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //This updates the existing entries of usercertificates with the new name
                    foreach (var userCertificate in _context.UserCertificates.Where(x => x.CertificateID == id))
                    {
                        userCertificate.CertificateName = certificate.Name;
                        _context.Update(userCertificate);
                    }
                    await _context.SaveChangesAsync();

                    _context.Update(certificate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificateExists(certificate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(certificate);
        }
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }
        [Authorize(Roles = "Admin, Superadmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool CertificateExists(int id)
        {
            return _context.Certificates.Any(e => e.Id == id);
        }
    }
}
