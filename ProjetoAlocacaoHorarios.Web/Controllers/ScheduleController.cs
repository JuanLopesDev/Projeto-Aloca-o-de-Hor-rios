using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoAlocacaoHorarios.Web.Data;
using ProjetoAlocacaoHorarios.Web.Services;

namespace ProjetoAlocacaoHorarios.Web.Controllers;

[Authorize(Roles = "Administrador,Coordenador")]
public class ScheduleController(ApplicationDbContext db, ScheduleGenerator generator) : Controller
{
    public async Task<IActionResult> Index()
    {
        var entries = await db.ScheduleEntries.Include(x => x.TimeSlot).Include(x => x.Teacher).Include(x => x.Subject).Include(x => x.ClassGroup).OrderBy(x => x.TimeSlot!.DayOfWeek).ThenBy(x => x.TimeSlot!.StartTime).ToListAsync();
        return View(entries);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate()
    {
        var result = await generator.GenerateAsync();
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<FileResult> Export()
    {
        var rows = await db.ScheduleEntries.Include(x => x.TimeSlot).Include(x => x.Teacher).Include(x => x.Subject).Include(x => x.ClassGroup).OrderBy(x => x.TimeSlot!.DayOfWeek).ThenBy(x => x.TimeSlot!.StartTime).ToListAsync();
        var csv = new StringBuilder("Dia;Horário;Professor;Disciplina;Turma\n");
        foreach (var row in rows) csv.AppendLine($"{row.TimeSlot!.DayOfWeek};{row.TimeSlot.StartTime:HH\\:mm};{row.Teacher!.Name};{row.Subject!.Name};{row.ClassGroup!.Name}");
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(), "text/csv", "grade-horarios.csv");
    }
}
