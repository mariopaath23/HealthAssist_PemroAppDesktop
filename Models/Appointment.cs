using SQLite;
using System;

namespace HealthAssist.Models
{
    [Table("Appointments")]
    public class Appointment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string DoctorName { get; set; } = string.Empty;

        public DateTime AppointmentDateTime { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
