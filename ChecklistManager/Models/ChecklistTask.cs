using Quartz;

namespace ChecklistManager.Models
{
    public class ChecklistTask
    {
        private static int nextId = 0;

        public int Id { get; set; }
        public string Description { get; set; } // Description of the task itself
        CronExpression? Schedule { get; set; } // If null, task is one-off (will rollover, but not repeat, aka always display null scheduled tasks)
        public string? AssignedTo { get; set; } // If null, task is communal
        public TaskState State { get; set; }
        public TaskAssignmentLevel AssignmentLevel { get; set; }

        {
            Id = nextId++;
            Description = description;
            Schedule = schedule;
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
