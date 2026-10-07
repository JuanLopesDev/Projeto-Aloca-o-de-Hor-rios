using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjetoAlocacaoHorarios.Web.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(120)] public string FullName { get; set; } = string.Empty;
}

public class Course
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(60)] public string EducationLevel { get; set; } = string.Empty;
    public ICollection<ClassGroup> ClassGroups { get; set; } = [];
}

public class ClassGroup
{
    public int Id { get; set; }
    [Required, MaxLength(40)] public string Name { get; set; } = string.Empty;
    [MaxLength(40)] public string Grade { get; set; } = string.Empty;
    [MaxLength(30)] public string AcademicTerm { get; set; } = string.Empty;
    [MaxLength(20)] public string Shift { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public ICollection<ClassSubject> ClassSubjects { get; set; } = [];
}

public class Subject
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = [];
}

public class TeacherProfile
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(450)] public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}

public class TeacherSubject { public int TeacherId { get; set; } public TeacherProfile? Teacher { get; set; } public int SubjectId { get; set; } public Subject? Subject { get; set; } }
public class ClassSubject { public int Id { get; set; } public int ClassGroupId { get; set; } public ClassGroup? ClassGroup { get; set; } public int SubjectId { get; set; } public Subject? Subject { get; set; } [Range(1, 20)] public int WeeklyLessons { get; set; } }

public class TimeSlot
{
    public int Id { get; set; }
    [Required] public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    [Range(30, 300)] public int DurationMinutes { get; set; }
    public string Label => $"{DayOfWeekText()} - {StartTime:HH\\:mm}";
    private string DayOfWeekText() => DayOfWeek switch { DayOfWeek.Monday => "Segunda", DayOfWeek.Tuesday => "Terça", DayOfWeek.Wednesday => "Quarta", DayOfWeek.Thursday => "Quinta", DayOfWeek.Friday => "Sexta", _ => DayOfWeek.ToString() };
}

public class TeacherAvailability { public int Id { get; set; } public int TeacherId { get; set; } public TeacherProfile? Teacher { get; set; } public int TimeSlotId { get; set; } public TimeSlot? TimeSlot { get; set; } }
public class TeachingAssignment { public int Id { get; set; } public int TeacherId { get; set; } public TeacherProfile? Teacher { get; set; } public int ClassGroupId { get; set; } public ClassGroup? ClassGroup { get; set; } public int SubjectId { get; set; } public Subject? Subject { get; set; } [Range(1, 20)] public int WeeklyLessons { get; set; } }
public class ScheduleEntry { public int Id { get; set; } public int TeachingAssignmentId { get; set; } public TeachingAssignment? TeachingAssignment { get; set; } public int TeacherId { get; set; } public TeacherProfile? Teacher { get; set; } public int ClassGroupId { get; set; } public ClassGroup? ClassGroup { get; set; } public int SubjectId { get; set; } public Subject? Subject { get; set; } public int TimeSlotId { get; set; } public TimeSlot? TimeSlot { get; set; } }
