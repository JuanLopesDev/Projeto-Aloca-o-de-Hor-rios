namespace ProjetoAlocacaoHorarios.Web.Models;

public class ManagementViewModel
{
    public List<Course> Courses { get; init; } = [];
    public List<ClassGroup> Classes { get; init; } = [];
    public List<Subject> Subjects { get; init; } = [];
    public List<TeacherProfile> Teachers { get; init; } = [];
    public List<TimeSlot> Slots { get; init; } = [];
    public List<TeachingAssignment> Assignments { get; init; } = [];
    public List<TeacherAvailability> Availabilities { get; init; } = [];
}
