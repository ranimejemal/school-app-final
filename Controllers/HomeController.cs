using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.Models.ViewModels;
using SchoolApp.Services;

namespace SchoolApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext   _ctx;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IStatisticsService     _stats;

    public HomeController(ApplicationDbContext ctx,
                          UserManager<ApplicationUser> users,
                          IStatisticsService stats)
    {
        _ctx   = ctx;
        _users = users;
        _stats = stats;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var roles   = await _users.GetRolesAsync(user);
        var isAdmin = roles.Contains("Admin");

        var vm = new DashboardViewModel
        {
            UserName = user.UserName ?? "Utilisateur",
            Role     = isAdmin ? "Administrateur" : roles.FirstOrDefault() ?? "Utilisateur",
            Stats    = await _stats.GetStatisticsAsync()
        };

        if (!isAdmin && user.PersonneId.HasValue)
        {
            var etudiant = await _ctx.Etudiants
                .Include(e => e.Groupe)
                .FirstOrDefaultAsync(e => e.Id == user.PersonneId);

            if (etudiant != null)
            {
                vm.Absences = await _ctx.Absences
                    .Include(a => a.Professeur)
                    .Where(a => a.EtudiantId == etudiant.Id)
                    .OrderByDescending(a => a.Date_debut)
                    .Take(5)
                    .ToListAsync();

                vm.Examens = await _ctx.Examens
                    .Include(x => x.Module)
                    .Where(x => x.EtudiantId == etudiant.Id)
                    .OrderByDescending(x => x.Date_Ex)
                    .Take(5)
                    .ToListAsync();
            }
        }

        return View(vm);
    }

    public IActionResult Error() => View();
}
// Note: MyExamens is in a separate controller action added below
