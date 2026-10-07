using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoAlocacaoHorarios.Web.Data;
using ProjetoAlocacaoHorarios.Web.Models;

namespace ProjetoAlocacaoHorarios.Web.Controllers;

[Authorize(Roles = "Administrador,Coordenador")]
public class ManagementController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await BuildViewModel());
    }

    public async Task<IActionResult> Teachers() => View(await BuildViewModel());
    public async Task<IActionResult> Courses() => View(await BuildViewModel());
    public async Task<IActionResult> Classes() => View(await BuildViewModel());
    public async Task<IActionResult> Subjects() => View(await BuildViewModel());
    public async Task<IActionResult> Slots() => View(await BuildViewModel());
    public async Task<IActionResult> Availability() => View(await BuildViewModel());
    public async Task<IActionResult> Assignments() => View(await BuildViewModel());

    private async Task<ManagementViewModel> BuildViewModel()
    {
        return new ManagementViewModel
        {
            Courses = await db.Courses.OrderBy(x => x.Name).ToListAsync(),
            Classes = await db.ClassGroups.Include(x => x.Course).OrderBy(x => x.Name).ToListAsync(),
            Subjects = await db.Subjects.OrderBy(x => x.Name).ToListAsync(),
            Teachers = await db.Teachers.OrderBy(x => x.Name).ToListAsync(),
            Slots = await db.TimeSlots.OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime).ToListAsync(),
            Assignments = await db.TeachingAssignments.Include(x => x.Teacher).Include(x => x.ClassGroup).Include(x => x.Subject).OrderBy(x => x.ClassGroup!.Name).ToListAsync(),
            Availabilities = await db.TeacherAvailabilities.Include(x => x.Teacher).Include(x => x.TimeSlot).OrderBy(x => x.Teacher!.Name).ToListAsync()
        };
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTeacher(string name, string email, string password = "Horario@2026")
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email)) return BackError("Informe nome e e-mail do professor.");
        if (await users.FindByEmailAsync(email) is not null || await db.Teachers.AnyAsync(x => x.Name == name)) return BackError("Já existe um professor ou usuário com estes dados.");
        var user = new ApplicationUser { FullName = name.Trim(), UserName = email.Trim(), Email = email.Trim(), EmailConfirmed = true };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded) return BackError(string.Join(" ", result.Errors.Select(x => x.Description)));
        await users.AddToRoleAsync(user, "Professor");
        db.Teachers.Add(new TeacherProfile { Name = name.Trim(), UserId = user.Id });
        await db.SaveChangesAsync();
        return BackSuccess("Professor criado com acesso ao sistema.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(string name, string educationLevel)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(educationLevel)) return BackError("Informe o nome e o nível de ensino do curso.");
        if (await db.Courses.AnyAsync(x => x.Name == name.Trim())) return BackError("Este curso já está cadastrado.");
        db.Courses.Add(new Course { Name = name.Trim(), EducationLevel = educationLevel.Trim() }); await db.SaveChangesAsync(); return BackSuccess("Curso cadastrado.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClass(int courseId, string name, string grade, string academicTerm, string shift, int studentCount)
    {
        if (!await db.Courses.AnyAsync(x => x.Id == courseId) || string.IsNullOrWhiteSpace(name)) return BackError("Selecione um curso e informe a turma.");
        db.ClassGroups.Add(new ClassGroup { CourseId = courseId, Name = name.Trim(), Grade = grade?.Trim() ?? "", AcademicTerm = academicTerm?.Trim() ?? "", Shift = shift?.Trim() ?? "", StudentCount = Math.Max(0, studentCount) }); await db.SaveChangesAsync(); return BackSuccess("Turma cadastrada.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubject(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return BackError("Informe o nome da disciplina.");
        if (await db.Subjects.AnyAsync(x => x.Name == name.Trim())) return BackError("Esta disciplina já está cadastrada.");
        db.Subjects.Add(new Subject { Name = name.Trim() }); await db.SaveChangesAsync(); return BackSuccess("Disciplina cadastrada.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSlot(DayOfWeek dayOfWeek, TimeOnly startTime, int durationMinutes)
    {
        if (durationMinutes < 30 || durationMinutes > 300) return BackError("A duração deve estar entre 30 e 300 minutos.");
        if (await db.TimeSlots.AnyAsync(x => x.DayOfWeek == dayOfWeek && x.StartTime == startTime)) return BackError("Já existe um slot nesse dia e horário.");
        db.TimeSlots.Add(new TimeSlot { DayOfWeek = dayOfWeek, StartTime = startTime, DurationMinutes = durationMinutes }); await db.SaveChangesAsync(); return BackSuccess("Slot cadastrado.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAvailability(int teacherId, int timeSlotId)
    {
        if (!await db.Teachers.AnyAsync(x => x.Id == teacherId) || !await db.TimeSlots.AnyAsync(x => x.Id == timeSlotId)) return BackError("Selecione professor e slot válidos.");
        if (await db.TeacherAvailabilities.AnyAsync(x => x.TeacherId == teacherId && x.TimeSlotId == timeSlotId)) return BackError("Essa disponibilidade já foi informada.");
        db.TeacherAvailabilities.Add(new TeacherAvailability { TeacherId = teacherId, TimeSlotId = timeSlotId }); await db.SaveChangesAsync(); return BackSuccess("Disponibilidade adicionada.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssignment(int teacherId, int classGroupId, int subjectId, int weeklyLessons)
    {
        if (weeklyLessons < 1 || weeklyLessons > 20) return BackError("Informe entre 1 e 20 aulas semanais.");
        var valid = await db.Teachers.AnyAsync(x => x.Id == teacherId) && await db.ClassGroups.AnyAsync(x => x.Id == classGroupId) && await db.Subjects.AnyAsync(x => x.Id == subjectId);
        if (!valid) return BackError("Preencha professor, turma e disciplina.");
        if (await db.TeachingAssignments.AnyAsync(x => x.TeacherId == teacherId && x.ClassGroupId == classGroupId && x.SubjectId == subjectId)) return BackError("Esta alocação já existe.");
        db.TeachingAssignments.Add(new TeachingAssignment { TeacherId = teacherId, ClassGroupId = classGroupId, SubjectId = subjectId, WeeklyLessons = weeklyLessons }); await db.SaveChangesAsync(); return BackSuccess("Alocação criada. Agora a grade pode ser gerada.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string entity, int id)
    {
        switch (entity)
        {
            case "assignment": var assignment = await db.TeachingAssignments.FindAsync(id); if (assignment is not null) db.Remove(assignment); break;
            case "availability": var availability = await db.TeacherAvailabilities.FindAsync(id); if (availability is not null) db.Remove(availability); break;
            case "slot": var slot = await db.TimeSlots.FindAsync(id); if (slot is not null) db.Remove(slot); break;
            case "subject": var subject = await db.Subjects.FindAsync(id); if (subject is not null) db.Remove(subject); break;
            default: return BackError("Item inválido.");
        }
        await db.SaveChangesAsync(); return BackSuccess("Item removido.");
    }

    private IActionResult BackSuccess(string message) { TempData["Success"] = message; return Back(); }
    private IActionResult BackError(string message) { TempData["Error"] = message; return Back(); }
    private IActionResult Back() { var referer = Request.Headers.Referer.FirstOrDefault(); return !string.IsNullOrWhiteSpace(referer) && Url.IsLocalUrl(referer) ? Redirect(referer) : RedirectToAction(nameof(Index)); }
}
