using Microsoft.EntityFrameworkCore;
using Pawlio;

public static class Notifications
{
    public static async Task AppoinmentNotify(int appointmentId)
    {
        PostgreSqlDbContext _context = new PostgreSqlDbContext();

        var a = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (a == null || a.IsDeleted) return;

        var u = await _context.Users.FirstOrDefaultAsync(u => u.Id == a.CreaterId);
        if (u == null || u.IsDeleted) return;

        var devices = await _context.Devices.Where(d => d.CreaterId == a.CreaterId).ToListAsync();
        FirebasePushNotificationUtils.PushMessage(
            devices,
            a.Customer?.Name + ", randevu",
            a.StartTime.ToString("HH:mm") + " - " + a.EndTime.ToString("HH:mm") + " / " + (a.Subject?.NameTr ?? "Genel İşlem") + " " + a.Notes,
            new Dictionary<string, string>
            {
                { "task", "appointment" },
                { "id", appointmentId.ToString() }
            });
    }
}
