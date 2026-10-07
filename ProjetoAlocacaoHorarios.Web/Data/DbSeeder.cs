using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoAlocacaoHorarios.Web.Models;

namespace ProjetoAlocacaoHorarios.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Administrador", "Coordenador", "Professor" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await CreateUser(users, "admin@horarios.local", "Administrador do Sistema", "Administrador");
        await CreateUser(users, "coordenador@horarios.local", "Mariana Costa", "Coordenador");
        var teacherUser = await CreateUser(users, "professor@horarios.local", "Carlos Mendes", "Professor");

        if (await db.Courses.AnyAsync()) return;

        var course = new Course { Name = "Análise e Desenvolvimento de Sistemas", EducationLevel = "Graduação" };
        var classes = new[] { new ClassGroup { Name = "ADS 1A", Grade = "1º semestre", AcademicTerm = "2026.2", Shift = "Noturno", StudentCount = 32, Course = course }, new ClassGroup { Name = "ADS 2A", Grade = "2º semestre", AcademicTerm = "2026.2", Shift = "Noturno", StudentCount = 28, Course = course } };
        var subjects = new[] { new Subject { Name = "Algoritmos" }, new Subject { Name = "Banco de Dados" }, new Subject { Name = "Engenharia de Software" } };
        var teachers = new[] { new TeacherProfile { Name = "Carlos Mendes", UserId = teacherUser.Id }, new TeacherProfile { Name = "Ana Ribeiro" }, new TeacherProfile { Name = "Paulo Lima" } };
        db.AddRange(course); db.AddRange(classes); db.AddRange(subjects); db.AddRange(teachers);
        await db.SaveChangesAsync();

        var slots = new List<TimeSlot>();
        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
            foreach (var time in new[] { new TimeOnly(18, 30), new TimeOnly(19, 30), new TimeOnly(20, 30) })
                slots.Add(new TimeSlot { DayOfWeek = day, StartTime = time, DurationMinutes = 50 });
        db.TimeSlots.AddRange(slots);
        await db.SaveChangesAsync();

        db.TeacherSubjects.AddRange(
            new TeacherSubject { TeacherId = teachers[0].Id, SubjectId = subjects[0].Id },
            new TeacherSubject { TeacherId = teachers[1].Id, SubjectId = subjects[1].Id },
            new TeacherSubject { TeacherId = teachers[2].Id, SubjectId = subjects[2].Id });
        db.ClassSubjects.AddRange(
            new ClassSubject { ClassGroupId = classes[0].Id, SubjectId = subjects[0].Id, WeeklyLessons = 2 },
            new ClassSubject { ClassGroupId = classes[0].Id, SubjectId = subjects[1].Id, WeeklyLessons = 2 },
            new ClassSubject { ClassGroupId = classes[1].Id, SubjectId = subjects[2].Id, WeeklyLessons = 2 });
        db.TeachingAssignments.AddRange(
            new TeachingAssignment { TeacherId = teachers[0].Id, ClassGroupId = classes[0].Id, SubjectId = subjects[0].Id, WeeklyLessons = 2 },
            new TeachingAssignment { TeacherId = teachers[1].Id, ClassGroupId = classes[0].Id, SubjectId = subjects[1].Id, WeeklyLessons = 2 },
            new TeachingAssignment { TeacherId = teachers[2].Id, ClassGroupId = classes[1].Id, SubjectId = subjects[2].Id, WeeklyLessons = 2 });
        foreach (var teacher in teachers)
            foreach (var slot in slots.Where(s => s.DayOfWeek is not DayOfWeek.Friday || s.StartTime < new TimeOnly(20, 30)))
                db.TeacherAvailabilities.Add(new TeacherAvailability { TeacherId = teacher.Id, TimeSlotId = slot.Id });
        await db.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> CreateUser(UserManager<ApplicationUser> users, string email, string name, string role)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = name };
            var result = await users.CreateAsync(user, "Horario@2026");
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
        if (!await users.IsInRoleAsync(user, role)) await users.AddToRoleAsync(user, role);
        return user;
    }
}
