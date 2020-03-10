using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Login_System.Models;

namespace Login_System.Controllers
{
    public class UserCertificatesController : Controller
    {
        private readonly UserCertificateDataContext _context;

        public UserCertificatesController(UserCertificateDataContext context)
        {
            _context = context;
        }

        // GET: UserCertificates
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserCertificates.ToListAsync());
        }

        // GET: UserCertificates/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: UserCertificates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserCertificates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserID,CertificateID,UserName,CertificateName,GrantDate")] UserCertificate userCertificate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userCertificate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userCertificate);
        }

        // GET: UserCertificates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userCertificate = await _context.UserCertificates.FindAsync(id);
            if (userCertificate == null)
            {
                return NotFound();
            }
            return View(userCertificate);
        }

        // POST: UserCertificates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserID,CertificateID,UserName,CertificateName,GrantDate")] UserCertificate userCertificate)
        {
            if (id != userCertificate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userCertificate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserCertificateExists(userCertificate.Id))
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
            return RedirectToAction(nameof(Index));
        }

        private bool UserCertificateExists(int id)
        {
            return _context.UserCertificates.Any(e => e.Id == id);
        }
    }
}
