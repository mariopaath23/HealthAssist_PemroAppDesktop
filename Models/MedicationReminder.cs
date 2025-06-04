using SQLite;
using System;

namespace HealthAssist.Models
{
    [Table("MedicationReminders")]
    public class MedicationReminder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int MedicationId { get; set; }

        public TimeSpan ReminderTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
