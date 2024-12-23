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

        public ChecklistTask(string description, string? scheduleString, string? assignedTo, TaskAssignmentLevel assignmentLevel)
        {
            ScheduleString = scheduleString;
            UpdateCron();

            Description = description;
            AssignmentLevel = assignmentLevel;
            State = TaskState.Incomplete;
            AssignedTo = assignedTo;


            Id = nextId++;
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
        Personal,
        Communal
    }
}
