using Quartz;

namespace ChecklistManager.Models
{
    public class ChecklistTask
    {
        internal static uint nextId = 0;

        public uint Id { get; set; }
        public string Description { get; set; } // Description of the task itself
        public string? AssignedTo { get; set; } // If null, task is communal
        public TaskState State { get; set; }
        public TaskAssignmentLevel AssignmentLevel { get; set; }

        // Seconds Minutes Hours Day-of-month Month Day-of-week Year(Optional)
        // Must assign to exactly one of either Day-of-month or Day-of-week, other must be ?
        public string? ScheduleString { get; set; }
        internal CronExpression? Schedule { get; set; } // If null, task is one-off (will rollover, but not repeat, aka always display null scheduled tasks)

        public ChecklistTask(uint id, string description, string? scheduleString, string? assignedTo, TaskAssignmentLevel assignmentLevel)
        {
            ScheduleString = scheduleString;
            UpdateCron();

            Description = description;
            AssignmentLevel = assignmentLevel;
            State = TaskState.Incomplete;
            AssignedTo = assignedTo;
            Id = id;
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
