using System;
using SQLite;

namespace HealthAssist.Models
{
    [Table("AppSettings")]
    public class AppSettings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
