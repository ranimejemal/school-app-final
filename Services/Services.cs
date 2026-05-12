using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.Models.ViewModels;

namespace SchoolApp.Services;

// ── Absence Service ───────────────────────────────────────────────────────────
public interface IAbsenceService
{
    Task<List<Absence>> GetAbsencesByEtudiantAsync(int etudiantId);
    Task<List<Absence>> SearchAsync(DateTime? dateDebut, DateTime? dateFin,
                                    string? justification, int? etudiantId, int? moduleId);
}

public class AbsenceService : IAbsenceService
{
    private readonly ApplicationDbContext _ctx;
    public AbsenceService(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<List<Absence>> GetAbsencesByEtudiantAsync(int etudiantId)
        => await _ctx.Absences
                     .Include(a => a.Professeur)
                     .Where(a => a.EtudiantId == etudiantId)
                     .OrderByDescending(a => a.Date_debut)
                     .ToListAsync();

    public async Task<List<Absence>> SearchAsync(DateTime? dateDebut, DateTime? dateFin,
                                                  string? justification, int? etudiantId, int? moduleId)
    {
        var q = _ctx.Absences
                    .Include(a => a.Etudiant)
                    .Include(a => a.Professeur)
                    .Include(a => a.Module)
                    .AsQueryable();

        if (etudiantId.HasValue)    q = q.Where(a => a.EtudiantId == etudiantId);
        if (moduleId.HasValue)      q = q.Where(a => a.ModuleId   == moduleId);
        if (dateDebut.HasValue)     q = q.Where(a => a.Date_debut >= dateDebut);
        if (dateFin.HasValue)       q = q.Where(a => a.Date_fin   <= dateFin);
        if (!string.IsNullOrEmpty(justification))
            q = q.Where(a => a.Justification != null &&
                              a.Justification.Contains(justification));

        return await q.OrderByDescending(a => a.Date_debut).ToListAsync();
    }
}

// ── Statistics Service ────────────────────────────────────────────────────────
public interface IStatisticsService
{
    Task<StatisticsViewModel> GetStatisticsAsync();
}

public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _ctx;
    public StatisticsService(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<StatisticsViewModel> GetStatisticsAsync()
    {
        var vm = new StatisticsViewModel
        {
            TotalEtudiants   = await _ctx.Etudiants.CountAsync(),
            TotalProfesseurs = await _ctx.Professeurs.CountAsync(),
            TotalModules     = await _ctx.Modules.CountAsync(),
            TotalGroupes     = await _ctx.Groupes.CountAsync(),
            TotalAbsences    = await _ctx.Absences.CountAsync(),
            TotalExamens     = await _ctx.Examens.CountAsync(),
        };

        // Absences par groupe
        var absByGroupe = await _ctx.Absences
            .Include(a => a.Etudiant).ThenInclude(e => e!.Groupe)
            .GroupBy(a => a.Etudiant!.Groupe!.Libelle)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToListAsync();

        vm.GroupeLabels      = absByGroupe.Select(x => x.Label).ToList();
        vm.AbsencesParGroupe = absByGroupe.Select(x => x.Count).ToList();

        // Moyenne notes par module
        var notesByModule = await _ctx.Examens
            .Include(x => x.Module)
            .GroupBy(x => x.Module!.Libelle)
            .Select(g => new { Label = g.Key, Moy = g.Average(x => (double)x.Note_Ex) })
            .ToListAsync();

        vm.ModuleLabels  = notesByModule.Select(x => x.Label).ToList();
        vm.MoyennesNotes = notesByModule.Select(x => Math.Round(x.Moy, 2)).ToList();

        // Absences justifiées vs non justifiées
        vm.AbsencesJustifiees    = await _ctx.Absences.CountAsync(a => a.Justification != null && a.Justification != "");
        vm.AbsencesNonJustifiees = vm.TotalAbsences - vm.AbsencesJustifiees;

        return vm;
    }
}
