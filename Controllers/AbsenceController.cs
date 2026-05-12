using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.Models.ViewModels;
using SchoolApp.Services;

namespace SchoolApp.Controllers;

[Authorize]
public class AbsenceController : Controller
{
    private readonly ApplicationDbContext        _ctx;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IAbsenceService             _service;

    public AbsenceController(ApplicationDbContext ctx,
                              UserManager<ApplicationUser> users,
                              IAbsenceService service)
    {
        _ctx     = ctx;
        _users   = users;
        _service = service;
    }

    // ── Admin/Prof: list all absences with filters ────────────────────────────
    [Authorize(Roles = "Admin,Professeur")]
    public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin,
                                            string? justification, int? etudiantId, int? moduleId)
    {
        var vm = new AbsenceSearchViewModel
        {
            DateDebut     = dateDebut,
            DateFin       = dateFin,
            Justification = justification,
            EtudiantId    = etudiantId,
            ModuleId      = moduleId,
            Results       = await _service.SearchAsync(dateDebut, dateFin, justification, etudiantId, moduleId)
        };
        await PopulateEtudiantsAsync(etudiantId);
        await PopulateModulesAsync(moduleId);
        return View(vm);
    }

    // ── Student: my own absences ──────────────────────────────────────────────
    [Authorize(Roles = "Etudiant")]
    public async Task<IActionResult> MyAbsences(DateTime? dateDebut, DateTime? dateFin, string? justification)
    {
        var user = await _users.GetUserAsync(User);
        if (user?.PersonneId == null) return Forbid();

        var absences = await _service.SearchAsync(dateDebut, dateFin, justification,
                                                   etudiantId: user.PersonneId, moduleId: null);
        var vm = new AbsenceSearchViewModel
        {
            DateDebut = dateDebut, DateFin = dateFin,
            Justification = justification, Results = absences
        };
        return View(vm);
    }

    // ── Create ────────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,Professeur")]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Professeur")]
    public async Task<IActionResult> Create(Absence model)
    {
        if (ModelState.IsValid)
        {
            // ── Anti-doublon rule: same student, same professor, same module, same day ──
            var duplicate = await _ctx.Absences.AnyAsync(a =>
                a.EtudiantId   == model.EtudiantId   &&
                a.ProfesseurId == model.ProfesseurId &&
                a.ModuleId     == model.ModuleId     &&
                a.Date_debut.Date == model.Date_debut.Date);

            if (duplicate)
            {
                ModelState.AddModelError(string.Empty,
                    "⚠️ Une absence a déjà été enregistrée pour cet étudiant dans cette séance (même module, même professeur, même date).");
                await PopulateDropdowns(model.EtudiantId, model.ProfesseurId, model.ModuleId);
                return View(model);
            }

            _ctx.Absences.Add(model);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Absence enregistrée.";
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns(model.EtudiantId, model.ProfesseurId, model.ModuleId);
        return View(model);
    }

    // ── Edit ──────────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var a = await _ctx.Absences.FindAsync(id);
        if (a == null) return NotFound();
        await PopulateDropdowns(a.EtudiantId, a.ProfesseurId, a.ModuleId);
        return View(a);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Absence model)
    {
        if (id != model.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            // Anti-doublon (excluding self)
            var duplicate = await _ctx.Absences.AnyAsync(a =>
                a.Id           != model.Id           &&
                a.EtudiantId   == model.EtudiantId   &&
                a.ProfesseurId == model.ProfesseurId &&
                a.ModuleId     == model.ModuleId     &&
                a.Date_debut.Date == model.Date_debut.Date);

            if (duplicate)
            {
                ModelState.AddModelError(string.Empty,
                    "⚠️ Une absence existe déjà pour cet étudiant dans cette séance.");
                await PopulateDropdowns(model.EtudiantId, model.ProfesseurId, model.ModuleId);
                return View(model);
            }

            _ctx.Update(model);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Absence modifiée.";
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns(model.EtudiantId, model.ProfesseurId, model.ModuleId);
        return View(model);
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _ctx.Absences
            .Include(x => x.Etudiant).Include(x => x.Professeur).Include(x => x.Module)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();
        return View(a);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var a = await _ctx.Absences.FindAsync(id);
        if (a != null) { _ctx.Absences.Remove(a); await _ctx.SaveChangesAsync(); }
        TempData["Success"] = "Absence supprimée.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task PopulateDropdowns(int? etId = null, int? prId = null, int? modId = null)
    {
        var ets  = await _ctx.Etudiants.OrderBy(e => e.Nom).ToListAsync();
        var pros = await _ctx.Professeurs.OrderBy(p => p.Nom).ToListAsync();
        var mods = await _ctx.Modules.OrderBy(m => m.Libelle).ToListAsync();
        ViewData["EtudiantId"]   = new SelectList(ets.Select(e  => new { e.Id,  Name = $"{e.Prenom} {e.Nom}" }),  "Id", "Name", etId);
        ViewData["ProfesseurId"] = new SelectList(pros.Select(p => new { p.Id,  Name = $"{p.Prenom} {p.Nom}" }), "Id", "Name", prId);
        ViewData["ModuleId"]     = new SelectList(mods, "Id_m", "Libelle", modId);
    }

    private async Task PopulateEtudiantsAsync(int? selectedId = null)
    {
        var ets = await _ctx.Etudiants.OrderBy(e => e.Nom).ToListAsync();
        ViewData["EtudiantId"] = new SelectList(ets.Select(e => new { e.Id, Name = $"{e.Prenom} {e.Nom}" }), "Id", "Name", selectedId);
    }

    private async Task PopulateModulesAsync(int? selectedId = null)
    {
        ViewData["ModuleId"] = new SelectList(await _ctx.Modules.OrderBy(m => m.Libelle).ToListAsync(), "Id_m", "Libelle", selectedId);
    }
}
