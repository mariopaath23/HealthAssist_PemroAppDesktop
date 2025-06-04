using SQLite;
using System;

namespace HealthAssist.Models
{
    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(10)]
        public string BloodType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Allergies { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public double Height { get; set; }

        public double Weight { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
