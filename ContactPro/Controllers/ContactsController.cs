﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactPro.Data;
using ContactPro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ContactPro.Enums;
using ContactPro.Services.Interfaces;
using ContactPro.Services;

namespace ContactPro.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;

        public ContactsController(ApplicationDbContext context, UserManager<AppUser> userManager, IImageService imageService, IAddressBookService addressBookService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;  
        }

        // GET: Contacts
        [Authorize]
        public async Task<IActionResult> Index(int catId, string state)
        {
            var contacts = new List<Contact>();
            string appUserId = _userManager.GetUserId(User);
            //return userID and its associated contacts and associated categories;
            AppUser? appUser = _context.Users
                .Include(c => c.Contacts)
                .ThenInclude(c => c.Categories)
                .FirstOrDefault(u => u.Id == appUserId);

            var categories = appUser!.Categories;

            //States drop down filter
            var contact = from s in _context.Contacts select s;
            //var stateResult = contact.Select(c => c.State).ToList();
            var stateResult = from States s in Enum.GetValues(typeof(States)) select new { Id = s.ToString(), Name = s.ToString() };

            //filters
            if (appUser != null)
            {
                if (catId == 0 && state == null || state == "All Contacts")
                {
                    contacts = appUser.Contacts.OrderBy(c => c.FullName).ToList();
                }
                
                else if (catId != 0)
                {
                    contacts = appUser.Categories.FirstOrDefault(c => c.Id == catId)!
                        .Contacts
                        .OrderBy(c => c.FirstName)
                        .ThenBy(c => c.LastName)
                        .ToList();
                }
                //Add in the state filter.
                else if (state != null)
                {
                    contacts = contact.Where(c => c.State == (States)Enum.Parse(typeof(States), state))
                        .OrderBy(c => c.FirstName)
                        .ThenBy(c => c.LastName)
                        .ToList();

                }
            }

            ViewData["States"] = new SelectList(stateResult, "Id", "Name",  state);
            ViewData["CategoryId"] = new SelectList(categories, "Id", "Name", catId);

            return View(contacts);
        }

        [Authorize]
        public IActionResult SearchContacts(string searchString)
        {
            string appUserId = _userManager.GetUserId(User);
            var contacts = new List<Contact>();
            var stateResult = from States s in Enum.GetValues(typeof(States)) select new { Id = s.ToString(), Name = s.ToString() };
            AppUser appUser = _context.Users
                .Include(c => c.Contacts)
                .ThenInclude(c => c.Categories)
                .FirstOrDefault(u => u.Id == appUserId);

            if(String.IsNullOrEmpty(searchString))
            {
                contacts = appUser.Contacts
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();
            }else
            {
                contacts = appUser.Contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();
            }

            ViewData["States"] = new SelectList(stateResult, "Id", "Name", "");
            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", 0);
            return View(nameof(Index), contacts);
        }

        [Authorize]
        public IActionResult EmailContact(int contactId)
        {
            return View();
        }

        // GET: Contacts/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User); 

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");

            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageFile")] Contact contact, List<int> CategoryList)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                contact.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }
                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                //Loop over all the selected categories               
                foreach (int categoryId in CategoryList)
                {
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                }
                //save each category selected to the contactcategories table.

                return RedirectToAction(nameof(Index));
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserId == appUserId).FirstOrDefaultAsync();

            //var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name", await _addressBookService.GetContactCategoryIdsAsync(contact.Id));
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData,ImageType")] Contact contact, List<int>CategoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);
                    if (contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }
                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    

                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    //Save Categories
                    //remove current categories
                    //add selected
                    List<Category> oldCategories = (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();
                    foreach (var category in oldCategories)
                    {
                        await _addressBookService.RemoveContactFromCategoryAsync(category.Id, contact.Id);
                    }

                    foreach (var categoryid in CategoryList)
                    {
                        await _addressBookService.AddContactToCategoryAsync(categoryid, contact.Id);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
            }
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
          return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
