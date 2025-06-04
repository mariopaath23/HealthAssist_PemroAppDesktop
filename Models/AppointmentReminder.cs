using SQLite;
using System;

namespace HealthAssist.Models
{
    public enum ReminderFrequency
    {
        Daily,
        Weekly,
        Monthly
    }

    [Table("AppointmentReminders")]
    public class AppointmentReminder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int AppointmentId { get; set; }

        public ReminderFrequency Frequency { get; set; }

        public TimeSpan ReminderTime { get; set; }

        public int DaysBefore { get; set; } = 1; // How many days before the appointment

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
