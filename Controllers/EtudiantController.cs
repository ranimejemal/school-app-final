using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Controllers;

[Authorize(Roles = "Admin")]
public class EtudiantController : Controller
{
    private readonly ApplicationDbContext      _ctx;
    private readonly UserManager<ApplicationUser> _userManager;

    public EtudiantController(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager)
    {
        _ctx         = ctx;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var q = _ctx.Etudiants.Include(e => e.Groupe).AsQueryable();
        if (!string.IsNullOrEmpty(search))
            q = q.Where(e => e.Nom.Contains(search) || e.Prenom.Contains(search)
                              || e.Email.Contains(search) || e.CNE.ToString().Contains(search));
        ViewData["Search"] = search;
        return View(await q.OrderBy(e => e.Nom).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var e = await _ctx.Etudiants
            .Include(e => e.Groupe).ThenInclude(g => g!.Niveau)
            .Include(e => e.Groupe).ThenInclude(g => g!.Specialite)
            .Include(e => e.Absences).ThenInclude(a => a.Professeur)
            .Include(e => e.Absences).ThenInclude(a => a.Module)
            .Include(e => e.Examens).ThenInclude(x => x.Module)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (e == null) return NotFound();
        return View(e);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateGroupesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Etudiant model, string tempPassword)
    {
        // Validate the temp password supplied by admin
        if (string.IsNullOrWhiteSpace(tempPassword))
            ModelState.AddModelError("tempPassword", "Le mot de passe temporaire est obligatoire.");

        if (ModelState.IsValid)
        {
            // Save the student record first
            _ctx.Etudiants.Add(model);
            await _ctx.SaveChangesAsync();

            // Create the Identity login account
            var userName = model.Email.Split('@')[0];
            var existing = await _userManager.FindByNameAsync(userName);
            if (existing == null)
            {
                var user = new ApplicationUser
                {
                    UserName          = userName,
                    Email             = model.Email,
                    PersonneId        = model.Id,
                    MustChangePassword = true          // forces popup on first login
                };
                var result = await _userManager.CreateAsync(user, tempPassword);
                if (result.Succeeded)
                    await _userManager.AddToRoleAsync(user, "Etudiant");
                else
                    TempData["Warning"] = "Étudiant ajouté mais le compte login n'a pas pu être créé : "
                                          + string.Join(", ", result.Errors.Select(e => e.Description));
            }
            else
            {
                TempData["Warning"] = "Étudiant ajouté. Un compte avec ce login existe déjà.";
            }

            TempData["Success"] = $"Étudiant ajouté. Login : {model.Email.Split('@')[0]}";
            return RedirectToAction(nameof(Index));
        }
        await PopulateGroupesAsync(model.GroupeId);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var e = await _ctx.Etudiants.FindAsync(id);
        if (e == null) return NotFound();
        await PopulateGroupesAsync(e.GroupeId);
        return View(e);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Etudiant model)
    {
        if (id != model.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            _ctx.Update(model);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Étudiant modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }
        await PopulateGroupesAsync(model.GroupeId);
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var e = await _ctx.Etudiants.Include(e => e.Groupe)
                                     .FirstOrDefaultAsync(e => e.Id == id);
        if (e == null) return NotFound();
        return View(e);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var etudiant = await _ctx.Etudiants.FindAsync(id);
        if (etudiant != null)
        {
            // Permanently delete the associated Identity account
            var appUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PersonneId == etudiant.Id);
            if (appUser != null)
                await _userManager.DeleteAsync(appUser);

            _ctx.Etudiants.Remove(etudiant);
            await _ctx.SaveChangesAsync();
        }
        TempData["Success"] = "Étudiant et son compte supprimés définitivement.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateGroupesAsync(int? selectedId = null)
    {
        var groupes = await _ctx.Groupes
            .Include(g => g.Niveau).Include(g => g.Specialite)
            .OrderBy(g => g.Libelle).ToListAsync();
        ViewData["GroupeId"] = new SelectList(groupes, "ID_gp", "Libelle", selectedId);
    }
}
