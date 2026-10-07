namespace ProjetoAlocacaoHorarios.Web.Models;

public class DashboardViewModel
{
    public int Teachers { get; init; }
    public int Classes { get; init; }
    public int Assignments { get; init; }
    public int ScheduledLessons { get; init; }
    public int TotalLessons { get; init; }
    public List<ScheduleEntry> UpcomingEntries { get; init; } = [];
}
