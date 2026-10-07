using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjetoAlocacaoHorarios.Web.Models;
using ProjetoAlocacaoHorarios.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjetoAlocacaoHorarios.Web.Controllers;

public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var entries = await db.ScheduleEntries.Include(x => x.TimeSlot).Include(x => x.Teacher).Include(x => x.Subject).Include(x => x.ClassGroup).OrderBy(x => x.TimeSlot!.DayOfWeek).ThenBy(x => x.TimeSlot!.StartTime).Take(6).ToListAsync();
        return View(new DashboardViewModel { Teachers = await db.Teachers.CountAsync(), Classes = await db.ClassGroups.CountAsync(), Assignments = await db.TeachingAssignments.CountAsync(), ScheduledLessons = await db.ScheduleEntries.CountAsync(), TotalLessons = await db.TeachingAssignments.SumAsync(x => (int?)x.WeeklyLessons) ?? 0, UpcomingEntries = entries });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
