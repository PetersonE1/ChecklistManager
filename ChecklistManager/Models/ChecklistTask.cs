using Quartz;

namespace ChecklistManager.Models
{
    public class ChecklistTask
    {
        public long Id { get; set; } // Handled by SQLite primary key
        public string Description { get; set; } // Description of the task itself
        public string? AssignedTo { get; set; } // If null, task is communal
        public string? DoneBy { get; set; }
        public TaskState State { get; set; }
        public TaskAssignmentLevel AssignmentLevel { get; set; }

        // Seconds Minutes Hours Day-of-month Month Day-of-week Year(Optional)
        // Must assign to exactly one of either Day-of-month or Day-of-week, other must be ?
        public string? ScheduleString { get; set; }
        internal CronExpression? Schedule { get; set; } // If null, task is one-off (will rollover, but not repeat, aka always display null scheduled tasks)

        public ChecklistTask(string description, string? scheduleString, string? assignedTo, TaskAssignmentLevel assignmentLevel)
        {
            ScheduleString = scheduleString;
            UpdateCron();

            Description = description;
            AssignmentLevel = assignmentLevel;
            State = TaskState.Incomplete;
            AssignedTo = assignedTo;
        }

        public void UpdateCron()
        {
            Schedule = ScheduleString != null ? new CronExpression(ScheduleString) : null;
        }
    }

    public enum TaskState
    {
        Incomplete,
        InProgress,
        Complete
    }

    public enum TaskAssignmentLevel
    {
        Communal,
        Personal
    }
}
