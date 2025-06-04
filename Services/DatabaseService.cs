using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HealthAssist.Models;
using Windows.Storage;

namespace HealthAssist.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _databasePath;

        public DatabaseService()
        {
            _databasePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HealthAssist.db");
        }

        private async Task InitializeAsync()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(_databasePath);

            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<Medication>();
            await _database.CreateTableAsync<MedicationReminder>();
            await _database.CreateTableAsync<Appointment>();
            await _database.CreateTableAsync<AppointmentReminder>();
            await _database.CreateTableAsync<AppSettings>();
        }

        public async Task<User?> GetUserAsync()
        {
            await InitializeAsync();
            return await _database!.Table<User>().FirstOrDefaultAsync();
        }

        public async Task<int> SaveUserAsync(User user)
        {
            await InitializeAsync();
            user.UpdatedAt = DateTime.Now;

            if (user.Id != 0)
                return await _database!.UpdateAsync(user);
            else
                return await _database!.InsertAsync(user);
        }

        // Medication operations
        public async Task<List<Medication>> GetMedicationsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<Medication>().Where(m => m.IsActive).ToListAsync();
        }

        public async Task<Medication?> GetMedicationAsync(int id)
        {
            await InitializeAsync();
            return await _database!.Table<Medication>().Where(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveMedicationAsync(Medication medication)
        {
            await InitializeAsync();
            medication.UpdatedAt = DateTime.Now;

            if (medication.Id != 0)
                return await _database!.UpdateAsync(medication);
            else
                return await _database!.InsertAsync(medication);
        }

        public async Task<int> DeleteMedicationAsync(Medication medication)
        {
            await InitializeAsync();
            medication.IsActive = false;
            return await _database!.UpdateAsync(medication);
        }

        // Medication Reminder operations
        public async Task<List<MedicationReminder>> GetMedicationRemindersAsync(int medicationId)
        {
            await InitializeAsync();
            return await _database!.Table<MedicationReminder>()
                .Where(r => r.MedicationId == medicationId && r.IsActive)
                .ToListAsync();
        }

        public async Task<int> SaveMedicationReminderAsync(MedicationReminder reminder)
        {
            await InitializeAsync();

            if (reminder.Id != 0)
                return await _database!.UpdateAsync(reminder);
            else
                return await _database!.InsertAsync(reminder);
        }

        public async Task<int> DeleteMedicationRemindersAsync(int medicationId)
        {
            await InitializeAsync();
            var reminders = await GetMedicationRemindersAsync(medicationId);
            foreach (var reminder in reminders)
            {
                reminder.IsActive = false;
                await _database!.UpdateAsync(reminder);
            }
            return reminders.Count;
        }

        // Appointment operations
        public async Task<List<Appointment>> GetAppointmentsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<Appointment>()
                .Where(a => !a.IsCompleted)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentAsync(int id)
        {
            await InitializeAsync();
            return await _database!.Table<Appointment>().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveAppointmentAsync(Appointment appointment)
        {
            await InitializeAsync();
            appointment.UpdatedAt = DateTime.Now;

            if (appointment.Id != 0)
                return await _database!.UpdateAsync(appointment);
            else
                return await _database!.InsertAsync(appointment);
        }

        public async Task<int> DeleteAppointmentAsync(Appointment appointment)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(appointment);
        }

        // Appointment Reminder operations
        public async Task<List<AppointmentReminder>> GetAppointmentRemindersAsync(int appointmentId)
        {
            await InitializeAsync();
            return await _database!.Table<AppointmentReminder>()
                .Where(r => r.AppointmentId == appointmentId && r.IsActive)
                .ToListAsync();
        }

        public async Task<int> SaveAppointmentReminderAsync(AppointmentReminder reminder)
        {
            await InitializeAsync();

            if (reminder.Id != 0)
                return await _database!.UpdateAsync(reminder);
            else
                return await _database!.InsertAsync(reminder);
        }

        // App Settings operations
        public async Task<string?> GetSettingAsync(string key)
        {
            await InitializeAsync();
            var setting = await _database!.Table<AppSettings>().Where(s => s.Key == key).FirstOrDefaultAsync();
            return setting?.Value;
        }

        public async Task<int> SaveSettingAsync(string key, string value)
        {
            await InitializeAsync();
            var existingSetting = await _database!.Table<AppSettings>().Where(s => s.Key == key).FirstOrDefaultAsync();

            if (existingSetting != null)
            {
                existingSetting.Value = value;
                existingSetting.UpdatedAt = DateTime.Now;
                return await _database!.UpdateAsync(existingSetting);
            }
            else
            {
                var newSetting = new AppSettings { Key = key, Value = value };
                return await _database!.InsertAsync(newSetting);
            }
        }

        public async Task<bool> IsOnboardingCompletedAsync()
        {
            var user = await GetUserAsync();
            return user != null;
        }
    }
}
