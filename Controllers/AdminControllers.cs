using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Controllers;

// ══════════════════════════════════════════════════════════════════════════════
//  NiveauController
// ══════════════════════════════════════════════════════════════════════════════
[Authorize(Roles = "Admin")]
public class NiveauController : Controller
{
    private readonly ApplicationDbContext _ctx;
    public NiveauController(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index() =>
        View(await _ctx.Niveaux.OrderBy(n => n.Libelle).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Niveau m)
    {
        if (!ModelState.IsValid) return View(m);
        _ctx.Niveaux.Add(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Niveau créé."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var n = await _ctx.Niveaux.FindAsync(id);
        return n == null ? NotFound() : View(n);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Niveau m)
    {
        if (id != m.Id_ni) return BadRequest();
        if (!ModelState.IsValid) return View(m);
        _ctx.Update(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Niveau modifié."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var n = await _ctx.Niveaux.FindAsync(id);
        return n == null ? NotFound() : View(n);
    }

    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var n = await _ctx.Niveaux.FindAsync(id);
        if (n != null) { _ctx.Niveaux.Remove(n); await _ctx.SaveChangesAsync(); }
        TempData["Success"] = "Niveau supprimé."; return RedirectToAction(nameof(Index));
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  SpecialiteController
// ══════════════════════════════════════════════════════════════════════════════
[Authorize(Roles = "Admin")]
public class SpecialiteController : Controller
{
    private readonly ApplicationDbContext _ctx;
    public SpecialiteController(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index() =>
        View(await _ctx.Specialites.OrderBy(s => s.Libelle).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Specialite m)
    {
        if (!ModelState.IsValid) return View(m);
        _ctx.Specialites.Add(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Spécialité créée."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var s = await _ctx.Specialites.FindAsync(id);
        return s == null ? NotFound() : View(s);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Specialite m)
    {
        if (id != m.Id_sp) return BadRequest();
        if (!ModelState.IsValid) return View(m);
        _ctx.Update(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Spécialité modifiée."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var s = await _ctx.Specialites.FindAsync(id);
        return s == null ? NotFound() : View(s);
    }

    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var s = await _ctx.Specialites.FindAsync(id);
        if (s != null) { _ctx.Specialites.Remove(s); await _ctx.SaveChangesAsync(); }
        TempData["Success"] = "Spécialité supprimée."; return RedirectToAction(nameof(Index));
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  GroupeController
// ══════════════════════════════════════════════════════════════════════════════
[Authorize(Roles = "Admin")]
public class GroupeController : Controller
{
    private readonly ApplicationDbContext _ctx;
    public GroupeController(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index() =>
        View(await _ctx.Groupes.Include(g => g.Niveau).Include(g => g.Specialite)
                                .OrderBy(g => g.Libelle).ToListAsync());

    public async Task<IActionResult> Create()
    {
        await PopulateAsync(); return View();
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Groupe m)
    {
        if (!ModelState.IsValid) { await PopulateAsync(); return View(m); }
        _ctx.Groupes.Add(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Groupe créé."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var g = await _ctx.Groupes.FindAsync(id);
        if (g == null) return NotFound();
        await PopulateAsync(g.NiveauId, g.SpecialiteId); return View(g);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Groupe m)
    {
        if (id != m.ID_gp) return BadRequest();
        if (!ModelState.IsValid) { await PopulateAsync(m.NiveauId, m.SpecialiteId); return View(m); }
        _ctx.Update(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Groupe modifié."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var g = await _ctx.Groupes.Include(g => g.Niveau).Include(g => g.Specialite)
                                   .FirstOrDefaultAsync(g => g.ID_gp == id);
        return g == null ? NotFound() : View(g);
    }

    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var g = await _ctx.Groupes.FindAsync(id);
        if (g != null) { _ctx.Groupes.Remove(g); await _ctx.SaveChangesAsync(); }
        TempData["Success"] = "Groupe supprimé."; return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAsync(int? nId = null, int? sId = null)
    {
        ViewData["NiveauId"]     = new SelectList(await _ctx.Niveaux.ToListAsync(),     "Id_ni", "Libelle", nId);
        ViewData["SpecialiteId"] = new SelectList(await _ctx.Specialites.ToListAsync(), "Id_sp", "Libelle", sId);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  ModuleController
// ══════════════════════════════════════════════════════════════════════════════
[Authorize(Roles = "Admin")]
public class ModuleController : Controller
{
    private readonly ApplicationDbContext _ctx;
    public ModuleController(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index() =>
        View(await _ctx.Modules.OrderBy(m => m.Libelle).ToListAsync());

    public IActionResult Create() => View();

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Module m)
    {
        if (!ModelState.IsValid) return View(m);
        _ctx.Modules.Add(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Module créé."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var m = await _ctx.Modules.FindAsync(id);
        return m == null ? NotFound() : View(m);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Module m)
    {
        if (id != m.Id_m) return BadRequest();
        if (!ModelState.IsValid) return View(m);
        _ctx.Update(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Module modifié."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var m = await _ctx.Modules.FindAsync(id);
        return m == null ? NotFound() : View(m);
    }

    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var m = await _ctx.Modules.FindAsync(id);
        if (m != null) { _ctx.Modules.Remove(m); await _ctx.SaveChangesAsync(); }
        TempData["Success"] = "Module supprimé."; return RedirectToAction(nameof(Index));
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  ProfesseurController
// ══════════════════════════════════════════════════════════════════════════════
[Authorize(Roles = "Admin")]
public class ProfesseurController : Controller
{
    private readonly ApplicationDbContext         _ctx;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfesseurController(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager)
    {
        _ctx         = ctx;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var q = _ctx.Professeurs.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            q = q.Where(p => p.Nom.Contains(search) || p.Prenom.Contains(search));
        ViewData["Search"] = search;
        return View(await q.OrderBy(p => p.Nom).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _ctx.Professeurs
            .Include(x => x.Affectations).ThenInclude(a => a.Module)
            .FirstOrDefaultAsync(x => x.Id == id);
        return p == null ? NotFound() : View(p);
    }

    public IActionResult Create() => View();

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Professeur m, string tempPassword)
    {
        if (string.IsNullOrWhiteSpace(tempPassword))
            ModelState.AddModelError("tempPassword", "Le mot de passe temporaire est obligatoire.");

        if (!ModelState.IsValid) return View(m);

        _ctx.Professeurs.Add(m);
        await _ctx.SaveChangesAsync();

        // Create Identity account
        var userName = m.Email.Split('@')[0];
        var existing = await _userManager.FindByNameAsync(userName);
        if (existing == null)
        {
            var user = new ApplicationUser
            {
                UserName           = userName,
                Email              = m.Email,
                PersonneId         = m.Id,
                MustChangePassword = true
            };
            var result = await _userManager.CreateAsync(user, tempPassword);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, "Professeur");
            else
                TempData["Warning"] = "Professeur ajouté mais le compte login n'a pas pu être créé : "
                                      + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        TempData["Success"] = $"Professeur ajouté. Login : {userName}";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var p = await _ctx.Professeurs.FindAsync(id);
        return p == null ? NotFound() : View(p);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Professeur m)
    {
        if (id != m.Id) return BadRequest();
        if (!ModelState.IsValid) return View(m);
        _ctx.Update(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Professeur modifié."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var p = await _ctx.Professeurs.FindAsync(id);
        return p == null ? NotFound() : View(p);
    }

    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var p = await _ctx.Professeurs.FindAsync(id);
        if (p != null)
        {
            // Permanently delete the associated Identity account
            var appUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PersonneId == p.Id);
            if (appUser != null)
                await _userManager.DeleteAsync(appUser);

            _ctx.Professeurs.Remove(p);
            await _ctx.SaveChangesAsync();
        }
        TempData["Success"] = "Professeur et son compte supprimés définitivement.";
        return RedirectToAction(nameof(Index));
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  ExamenController
// ══════════════════════════════════════════════════════════════════════════════
[Authorize(Roles = "Admin")]
public class ExamenController : Controller
{
    private readonly ApplicationDbContext _ctx;
    public ExamenController(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index(int? etudiantId, int? moduleId)
    {
        var q = _ctx.Examens.Include(x => x.Etudiant).Include(x => x.Module).AsQueryable();
        if (etudiantId.HasValue) q = q.Where(x => x.EtudiantId == etudiantId);
        if (moduleId.HasValue)   q = q.Where(x => x.ModuleId   == moduleId);
        await PopulateAsync(etudiantId, moduleId);
        return View(await q.OrderByDescending(x => x.Date_Ex).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await PopulateAsync(); return View();
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Examen m)
    {
        if (!ModelState.IsValid) { await PopulateAsync(); return View(m); }
        _ctx.Examens.Add(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Examen ajouté."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var x = await _ctx.Examens.FindAsync(id);
        if (x == null) return NotFound();
        await PopulateAsync(x.EtudiantId, x.ModuleId); return View(x);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Examen m)
    {
        if (id != m.Id) return BadRequest();
        if (!ModelState.IsValid) { await PopulateAsync(m.EtudiantId, m.ModuleId); return View(m); }
        _ctx.Update(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Examen modifié."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var x = await _ctx.Examens.Include(e => e.Etudiant).Include(e => e.Module)
                                   .FirstOrDefaultAsync(e => e.Id == id);
        return x == null ? NotFound() : View(x);
    }

    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var x = await _ctx.Examens.FindAsync(id);
        if (x != null) { _ctx.Examens.Remove(x); await _ctx.SaveChangesAsync(); }
        TempData["Success"] = "Examen supprimé."; return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAsync(int? etId = null, int? modId = null)
    {
        var ets  = await _ctx.Etudiants.OrderBy(e => e.Nom).ToListAsync();
        var mods = await _ctx.Modules.OrderBy(m => m.Libelle).ToListAsync();
        ViewData["EtudiantId"] = new SelectList(ets.Select(e => new { e.Id, Name=$"{e.Prenom} {e.Nom}" }), "Id", "Name", etId);
        ViewData["ModuleId"]   = new SelectList(mods, "Id_m", "Libelle", modId);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
//  AffectationController
// ══════════════════════════════════════════════════════════════════════════════
[Authorize(Roles = "Admin")]
public class AffectationController : Controller
{
    private readonly ApplicationDbContext _ctx;
    public AffectationController(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index() =>
        View(await _ctx.Affectations.Include(a => a.Professeur).Include(a => a.Module)
                                     .OrderByDescending(a => a.Date_affectation).ToListAsync());

    public async Task<IActionResult> Create()
    {
        await PopulateAsync(); return View();
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Affectation m)
    {
        if (!ModelState.IsValid) { await PopulateAsync(); return View(m); }
        _ctx.Affectations.Add(m); await _ctx.SaveChangesAsync();
        TempData["Success"] = "Affectation créée."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var a = await _ctx.Affectations.Include(x => x.Professeur).Include(x => x.Module)
                                        .FirstOrDefaultAsync(x => x.Id == id);
        return a == null ? NotFound() : View(a);
    }

    [HttpPost, ActionName("Delete")][ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var a = await _ctx.Affectations.FindAsync(id);
        if (a != null) { _ctx.Affectations.Remove(a); await _ctx.SaveChangesAsync(); }
        TempData["Success"] = "Affectation supprimée."; return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAsync(int? prId = null, int? modId = null)
    {
        var pros = await _ctx.Professeurs.OrderBy(p => p.Nom).ToListAsync();
        var mods = await _ctx.Modules.OrderBy(m => m.Libelle).ToListAsync();
        ViewData["ProfesseurId"] = new SelectList(pros.Select(p => new { p.Id, Name=$"{p.Prenom} {p.Nom}" }), "Id", "Name", prId);
        ViewData["ModuleId"]     = new SelectList(mods, "Id_m", "Libelle", modId);
    }
}
