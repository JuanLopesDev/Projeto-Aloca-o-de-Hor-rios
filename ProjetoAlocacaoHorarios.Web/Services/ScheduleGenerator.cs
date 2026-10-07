using Microsoft.EntityFrameworkCore;
using ProjetoAlocacaoHorarios.Web.Data;
using ProjetoAlocacaoHorarios.Web.Models;

namespace ProjetoAlocacaoHorarios.Web.Services;

public record GenerationResult(bool Success, string Message, int GeneratedLessons);

public class ScheduleGenerator(ApplicationDbContext db)
{
    public async Task<GenerationResult> GenerateAsync()
    {
        var assignments = await db.TeachingAssignments
            .Include(x => x.Teacher).Include(x => x.ClassGroup).Include(x => x.Subject)
            .OrderByDescending(x => x.WeeklyLessons).ToListAsync();
        var availability = await db.TeacherAvailabilities.ToListAsync();
        var slots = await db.TimeSlots.OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToListAsync();
        if (assignments.Count == 0 || slots.Count == 0)
            return new(false, "Cadastre alocações e slots antes de gerar a grade.", 0);

        foreach (var assignment in assignments)
        {
            var availableCount = availability.Count(x => x.TeacherId == assignment.TeacherId);
            if (availableCount < assignment.WeeklyLessons)
                return new(false, $"{assignment.Teacher!.Name} não possui disponibilidade suficiente para {assignment.Subject!.Name} na turma {assignment.ClassGroup!.Name}.", 0);
        }

        var draft = new List<ScheduleEntry>();
        foreach (var assignment in assignments)
        {
            for (var lesson = 0; lesson < assignment.WeeklyLessons; lesson++)
            {
                var candidate = slots
                    .Where(slot => availability.Any(a => a.TeacherId == assignment.TeacherId && a.TimeSlotId == slot.Id))
                    .Where(slot => !draft.Any(e => e.TeacherId == assignment.TeacherId && e.TimeSlotId == slot.Id))
                    .Where(slot => !draft.Any(e => e.ClassGroupId == assignment.ClassGroupId && e.TimeSlotId == slot.Id))
                    .OrderBy(slot => draft.Count(e => e.TeacherId == assignment.TeacherId && e.TimeSlot!.DayOfWeek == slot.DayOfWeek))
                    .ThenBy(slot => draft.Count(e => e.ClassGroupId == assignment.ClassGroupId && e.TimeSlot!.DayOfWeek == slot.DayOfWeek))
                    .ThenBy(slot => slot.DayOfWeek).ThenBy(slot => slot.StartTime).FirstOrDefault();
                if (candidate is null)
                    return new(false, $"Não foi possível acomodar todas as aulas de {assignment.Subject!.Name} para a turma {assignment.ClassGroup!.Name}. Revise disponibilidades e slots.", 0);
                draft.Add(new ScheduleEntry { TeachingAssignmentId = assignment.Id, TeacherId = assignment.TeacherId, ClassGroupId = assignment.ClassGroupId, SubjectId = assignment.SubjectId, TimeSlotId = candidate.Id, TimeSlot = candidate });
            }
        }

        await db.ScheduleEntries.ExecuteDeleteAsync();
        db.ScheduleEntries.AddRange(draft);
        await db.SaveChangesAsync();
        return new(true, $"Grade gerada com {draft.Count} aulas, sem conflitos de professor ou turma.", draft.Count);
    }
}
