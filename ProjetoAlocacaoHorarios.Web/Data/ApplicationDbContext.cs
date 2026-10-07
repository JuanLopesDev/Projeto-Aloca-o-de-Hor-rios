using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using ProjetoAlocacaoHorarios.Web.Models;

namespace ProjetoAlocacaoHorarios.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<ClassGroup> ClassGroups => Set<ClassGroup>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherProfile> Teachers => Set<TeacherProfile>();
    public DbSet<TeacherSubject> TeacherSubjects => Set<TeacherSubject>();
    public DbSet<ClassSubject> ClassSubjects => Set<ClassSubject>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<TeacherAvailability> TeacherAvailabilities => Set<TeacherAvailability>();
    public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();
    public DbSet<ScheduleEntry> ScheduleEntries => Set<ScheduleEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Course>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<Subject>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<TimeSlot>().HasIndex(x => new { x.DayOfWeek, x.StartTime }).IsUnique();
        builder.Entity<TeacherSubject>().HasKey(x => new { x.TeacherId, x.SubjectId });
        builder.Entity<ClassSubject>().HasIndex(x => new { x.ClassGroupId, x.SubjectId }).IsUnique();
        builder.Entity<TeacherAvailability>().HasIndex(x => new { x.TeacherId, x.TimeSlotId }).IsUnique();
        builder.Entity<ScheduleEntry>().HasIndex(x => new { x.TeacherId, x.TimeSlotId }).IsUnique();
        builder.Entity<ScheduleEntry>().HasIndex(x => new { x.ClassGroupId, x.TimeSlotId }).IsUnique();
    }
}
