using Quartz;

namespace ChecklistManager.Models
{
    public class ChecklistTask
    {
        private static int nextId = 0;

        public int Id { get; set; }
        public string Description { get; set; } // Description of the task itself
        public string? AssignedTo { get; set; } // If null, task is communal
        public TaskState State { get; set; }
        public TaskAssignmentLevel AssignmentLevel { get; set; }

        public string? ScheduleString { get; set; }
        internal CronExpression? Schedule { get; set; } // If null, task is one-off (will rollover, but not repeat, aka always display null scheduled tasks)

        public ChecklistTask(string description, TaskAssignmentLevel assignmentLevel, string? scheduleString, string? assignedTo)
        {
            Id = nextId++;
            Description = description;
            AssignmentLevel = assignmentLevel;
            State = TaskState.Incomplete;
            AssignedTo = assignedTo;

            ScheduleString = scheduleString;
            Schedule = scheduleString != null ? new CronExpression(scheduleString) : null;
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
        Personal,
        Communal
    }
}
