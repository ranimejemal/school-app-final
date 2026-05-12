using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Services;

namespace SchoolApp.Controllers;

[Authorize]
public class StatisticsController : Controller
{
    private readonly IStatisticsService _stats;
    public StatisticsController(IStatisticsService stats) => _stats = stats;

    public async Task<IActionResult> Index()
    {
        var vm = await _stats.GetStatisticsAsync();
        return View(vm);
    }
}
