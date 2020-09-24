using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Login_System.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;

namespace Login_System.Controllers
{
    [Authorize(Roles = "Admin, Superadmin, User")]
    public class CertificatesController : Controller
    {
        private readonly GeneralDataContext _context;
        private readonly IdentityDataContext _identityContext;
        private UserManager<AppUser> UserMgr { get; }

        public CertificatesController(GeneralDataContext context, UserManager<AppUser> userManager, IdentityDataContext identity)
        {
            _context = context;
            UserMgr = userManager;
            _identityContext = identity;
        }
        [Authorize(Roles = "Admin, Superadmin")]
        public async Task<IActionResult> Index(string searchString)
        {
            var id = await UserMgr.GetUserAsync(HttpContext.User);
            List<Certificate> certificates = new List<Certificate>();
            var companyGroupCertificates = new List<CompanyGroupCertificate>();
            if (User.IsInRole("Superadmin"))
            {
                certificates = _context.Certificates.ToList();
            }
            else
            {
                var companygroupid = _context.CompanyGroupMembers.FirstOrDefault(x => x.CompanyId == id.Company).CompanyGroupId;
                companyGroupCertificates = _context.CompanyGroupCertificates.Where(x => ((x.CompanyId == id.Company) && (x.CompanyGroupId == companygroupid)) || (x.CompanyGroupId == companygroupid) && (x.CompanyId == null)).ToList();

                foreach (var certId in companyGroupCertificates)
                {
                    var cert = _context.Certificates.FirstOrDefault(x => x.Id == certId.CertificateId);
                    certificates.Add(cert);
                }
            }
            return View(certificates);
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
        public async Task<IActionResult> Create([Bind("Id,Name,Organization")] Certificate certificate, UserCertificate userCertificate)
        {
            if (ModelState.IsValid)
            {
                  
                var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
                var compGroup = _context.CompanyGroupMembers.FirstOrDefault(x => x.CompanyId == currentUser.Company);
                _context.Add(certificate);
                var certificateExists = _context.Certificates.Any(x => x.Name == certificate.Name);

                if (certificateExists)
                {
                    ModelState.AddModelError("Certificate", "This certificate already exists");
                    return View(certificate);
                }

                await _context.SaveChangesAsync();

                var CertificateID = certificate.Id;

                var newUserCert = new UserCertificate
                {
                    UserID = currentUser.Id,
                    CertificateID = certificate.Id,
                    UserName = currentUser.UserName,
                    CertificateName = certificate.Name,
                    Organization = certificate.Organization,
                    GrantDate = DateTime.Now
                };

                _context.Add(newUserCert);
                _context.Add(new CompanyGroupCertificate
                {
                    CertificateId = certificate.Id,
                    CompanyGroupId = (!User.IsInRole("Superadmin")) ? compGroup.CompanyGroupId : (int?)null,
                    CompanyId = User.IsInRole("Admin") ? currentUser.Company : (int?)null,
                });
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
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Delete(int? id)
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
            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Delete successful" });
        }
        private bool CertificateExists(int id)
        {
            return _context.Certificates.Any(e => e.Id == id);
        }

        //[Authorize(Roles = "Admin, Superadmin")]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var certificate = await _context.Certificates
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (certificate == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(certificate);
        //}
        //[Authorize(Roles = "Admin, Superadmin")]
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var certificate = await _context.Certificates.FindAsync(id);
        //    _context.Certificates.Remove(certificate);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //private bool CertificateExists(int id)
        //{
        //    return _context.Certificates.Any(e => e.Id == id);
        //}
        [HttpGet]
        public async Task<IActionResult> Grant(int id)
        {
            GrantCertificateVM model = new GrantCertificateVM();
            List<int> usersAlreadyGrantedCertificate = _context.UserCertificates.Where(x=> x.CertificateID == id).Select(x=> x.UserID).ToList();
            if (User.IsInRole("Superadmin"))
            {
                foreach (var user in _identityContext.Users.Where(x => !usersAlreadyGrantedCertificate.Contains(x.Id)))
                {
                    model.UserList.Add(new SelectListItem() { Text = user.FirstName + " " + user.LastName, Value = user.Id.ToString() });
                }
            }
            else
            {
                var currentUser = await UserMgr.GetUserAsync(HttpContext.User);
                foreach (var user in _identityContext.Users.Where(x => !usersAlreadyGrantedCertificate.Contains(x.Id) && x.Company == currentUser.Company))
                {
                    model.UserList.Add(new SelectListItem() { Text = user.FirstName + " " + user.LastName, Value = user.Id.ToString() });
                }
            }

            model.Certificate = await _context.Certificates.FindAsync(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Grant(GrantCertificateVM model)
        {
            if (ModelState.IsValid)
            {
             
               // var grantDate = DateTime.Now;

                string tempDateString = DateTime.ParseExact(model.UserCertificate.DateString, "dd.MM.yyyy", CultureInfo.CurrentCulture).ToShortDateString();               
                DateTime tempDateTime = DateTime.Parse(tempDateString, CultureInfo.CurrentCulture);
                foreach (var item in model.UserIds)
                {
                    var user = _identityContext.Users.Find(item);
                    _context.UserCertificates.Add(new UserCertificate { 
                        UserID = user.Id,
                        CertificateID = model.Certificate.Id,
                        GrantDate = model.UserCertificate.GrantDate,
                        ExpiryDate = tempDateTime,
                        CertificateName = model.Certificate.Name,
                        Organization = model.Certificate.Organization,
                        UserName = user.UserName
                    });
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
                
            }
            return View(model);
            
        }
    }
}
