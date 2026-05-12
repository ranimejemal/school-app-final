using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Controllers;

[Authorize(Roles = "Etudiant")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext         _ctx;
    private readonly UserManager<ApplicationUser> _users;

    public StudentController(ApplicationDbContext ctx, UserManager<ApplicationUser> users)
    {
        _ctx   = ctx;
        _users = users;
    }

    /// <summary>Shows the logged-in student's exam results, grouped by module, with weighted average.</summary>
    public async Task<IActionResult> MyExamens(int? moduleId)
    {
        var user = await _users.GetUserAsync(User);
        if (user?.PersonneId == null) return Forbid();

        var etudiant = await _ctx.Etudiants
            .Include(e => e.Groupe).ThenInclude(g => g!.Niveau)
            .Include(e => e.Groupe).ThenInclude(g => g!.Specialite)
            .FirstOrDefaultAsync(e => e.Id == user.PersonneId);

        if (etudiant == null) return NotFound();

        var q = _ctx.Examens
            .Include(x => x.Module)
            .Where(x => x.EtudiantId == user.PersonneId);

        if (moduleId.HasValue) q = q.Where(x => x.ModuleId == moduleId);

        var examens = await q.OrderBy(x => x.Module!.Libelle).ThenByDescending(x => x.Date_Ex).ToListAsync();

        // Compute weighted average across all modules
        double? moyennePonderee = null;
        var allExamens = await _ctx.Examens
            .Include(x => x.Module)
            .Where(x => x.EtudiantId == user.PersonneId)
            .ToListAsync();

        if (allExamens.Any())
        {
            double totalCoef  = allExamens.Sum(x => x.Module?.Coef ?? 1);
            double totalNotes = allExamens.Sum(x => x.Note_Ex * (x.Module?.Coef ?? 1));
            moyennePonderee = totalCoef > 0 ? totalNotes / totalCoef : 0;
        }

        var modules = await _ctx.Modules.OrderBy(m => m.Libelle).ToListAsync();
        ViewData["ModuleId"]        = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(modules, "Id_m", "Libelle", moduleId);
        ViewData["MoyennePonderee"] = moyennePonderee;
        ViewData["Etudiant"]        = etudiant;

        return View(examens);
    }
}
