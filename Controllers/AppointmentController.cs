using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Models;
using Azure.Core.Serialization;
using Microsoft.AspNetCore.SignalR;
using Pawlio.Hubs;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FirebaseAdmin.Messaging;

namespace Pawlio.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ApiController
    {
        public AppointmentController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet]
        public async Task<ActionResult<List<dynamic>>> GetAppointments()
        {
            var user = this.GetUser();

            var query = _context.Appointments
                .Where(d =>
                    d.CreaterId == user.Id &&
                    d.FirmId == user.FirmId &&
                    d.BranchId == user.BranchId &&
                    !d.IsDeleted);

            if (user.isVeterinary)
                query = query.Include(a => a.Animals)!.ThenInclude(a => a.Animal);

            var result = await query
                .OrderByDescending(d => d.Id)
                .Take(250)
                .ToAppointment()
                .ToListAsync<dynamic>();

            return result;
        }

        [HttpPost("filter")]
        public async Task<ActionResult<List<dynamic>>> GetAppointments([FromBody] dynamic data)
        {
            var user = this.GetUser();
            if (_context.Appointments == null) return NotFound();

            DateTime startDate = data.startDate;
            startDate = startDate.Date;

            DateTime endDate = startDate.AddDays(1).AddSeconds(-1); // 23:59:59 
            if (data.endDate != null) endDate = data.endDate;

            var query = _context.Appointments
                .Where(d =>
                    d.CreaterId == user.Id &&
                    d.FirmId == user.FirmId &&
                    d.BranchId == user.BranchId &&
                    d.StartTime >= startDate && d.EndTime <= endDate &&
                    !d.IsDeleted);

            if (user.isVeterinary)
                query = query.Include(a => a.Animals)!.ThenInclude(a => a.Animal);

            var result = await query.ToAppointment().ToListAsync<dynamic>();
            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<dynamic>> GetAppointment(int id)
        {
            var user = this.GetUser();
            if (_context.Appointments == null) return NotFound();
            var appointment = await _context.Appointments
                .Include(a => a.Animals)
                .Where(d => d.Id == id && d.FirmId == user.FirmId && !d.IsDeleted)
                .ToAppointment()
                .FirstOrDefaultAsync();

            if (appointment == null) return NotFound();
            return appointment;
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<dynamic>> UpdateAppointment(int id, [FromBody] Appointment appointment)
        {
            var user = this.GetUser();
            if (id != appointment.Id) return Problem("Hatalı istek!");

            var exist = await _context.Appointments.Include(a => a.Animals)
                .FirstOrDefaultAsync(a => a.Id == id && a.CreaterId == user.Id && !a.IsDeleted);

            if (exist == null) return Problem($"{id} nolu randevu bulunamadı!");

            try
            {
                if (exist.JobId > 0)
                    Hangfire.BackgroundJob.Delete(exist.JobId.ToString());
            }
            catch { }

            await _context.Database.BeginTransactionAsync();
            try
            {
                var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == appointment.BranchId);
                if (branch != null && branch.AppointmentNotifyTime > 0)
                {
                    var notifyTime = appointment.StartTime.AddMinutes(-branch.AppointmentNotifyTime);
                    exist.JobId = Hangfire.BackgroundJob.Schedule(() => Notifications.AppoinmentNotify(appointment.Id), notifyTime).ToInt();
                }

                exist.AllDay = appointment.AllDay;
                exist.Status = appointment.Status;
                exist.StartTime = appointment.StartTime;
                exist.EndTime = appointment.EndTime;
                exist.Lat = appointment.Lat;
                exist.Lon = appointment.Lon;
                exist.SubjectId = appointment.SubjectId;
                exist.CustomerId = appointment.CustomerId;
                exist.SupplierId = appointment.SupplierId;
                exist.Notes = appointment.Notes;

                // Yeni hayvan ekleme
                foreach (var ap in appointment.Animals)
                {
                    var ex = exist.Animals.FirstOrDefault(aa => aa.AnimalId == ap.AnimalId);
                    if (ex == null) exist.Animals.Add(ap);
                }

                // Silinen hayvanları kaldırma
                foreach (var ap in exist.Animals)
                {
                    var ex = appointment.Animals.FirstOrDefault(aa => aa.AnimalId == ap.AnimalId);
                    if (ex == null) exist.Animals.Remove(ap);
                }

                await _context.SaveAsync(this);

                var subject = exist.SubjectId == null ? null : await _context.Definitions.FirstOrDefaultAsync(d => d.Id == exist.SubjectId);
                await this.AddEvent(_context, EventTypes.AppointmentUpdate, new Event
                {
                    Flavor = user.Flavor,
                    EventId = appointment.Id,
                    CustomerId = appointment.CustomerId,
                    //AnimalId = appointment.AnimalId,
                    //Title = subject.Name,
                    //Detail = "Randevu bilgileri güncellendi!",
                    EventSubId = subject?.Id ?? 0
                });

                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }

            var res = await _context.Appointments
                .Include(a => a.Animals)
                .Where(d => d.Id == id && d.FirmId == user.FirmId && !d.IsDeleted)
                .ToAppointment()
                .FirstOrDefaultAsync();

            if (res == null) return NotFound();
            return res;
        }

        [HttpPost]
        public async Task<ActionResult<Appointment>> AddAppointment(Appointment appointment)
        {
            var user = this.GetUser();

            appointment.FirmId = user.FirmId;
            appointment.BranchId = user.BranchId;
            _context.Appointments.Add(appointment);
            await _context.SaveAsync(this);

            var subject = await _context.Definitions.FirstOrDefaultAsync(d => d.Id == appointment.SubjectId);
            await this.AddEvent(_context, EventTypes.AppointmentCreate, new Event
            {
                Flavor = user.Flavor,
                EventId = appointment.Id,
                CustomerId = appointment.CustomerId,
                //AnimalId = appointment.AnimalId,
                //Title = subject?.Name ?? "",
                //Detail = "Yeni randevu eklendi!",
                EventSubId = subject?.Id ?? 0
            });

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == appointment.BranchId);
            if (branch != null && branch.AppointmentNotifyTime > 0)
            {
                var notifyTime = appointment.StartTime.AddMinutes(-branch.AppointmentNotifyTime);
                appointment.JobId = Hangfire.BackgroundJob.Schedule(() => Notifications.AppoinmentNotify(appointment.Id), notifyTime).ToInt();
                await _context.SaveAsync(this);
            }

            return CreatedAtAction("GetAppointment", new { id = appointment.Id }, appointment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var user = this.GetUser();
            var appointment = await _context.Appointments.FirstOrDefaultAsync(d => d.Id == id && d.FirmId == user.FirmId);
            if (appointment == null) return NotFound();

            appointment.IsDeleted = true;
            await _context.SaveAsync(this);

            var subject = await _context.Definitions.FirstAsync(d => d.Id == appointment.SubjectId);
            await this.AddEvent(_context, EventTypes.AppointmentDelete, new Event
            {
                Flavor = user.Flavor,
                EventId = appointment.Id,
                CustomerId = appointment.CustomerId,
                //Title = subject?.Name ?? "Genel Randevu",
                //Detail = "Randevu iptal edildi!",
                EventSubId = subject?.Id ?? 0
            });

            try
            {
                if (appointment.JobId > 0)
                    Hangfire.BackgroundJob.Delete(appointment.JobId.ToString());
            }
            catch { }

            return NoContent();
        }

        [HttpPost("settings/{branchId}")]
        public async Task<IActionResult> UpdateAppointmentSettings(int branchId, [FromBody] dynamic settings)
        {
            var user = this.GetUser();
            var branch = await _context.Branches.FirstOrDefaultAsync(d => d.Id == branchId && d.FirmId == user.FirmId);
            if (branch == null) return Problem("Şube bulunamadı!");

            branch.AppointmentStartTimeHour = settings.appointmentStartTimeHour;
            branch.AppointmentStartTimeMinute = settings.appointmentStartTimeMinute;
            branch.AppointmentEndTimeHour = settings.appointmentEndTimeHour;
            branch.AppointmentEndTimeMinute = settings.appointmentEndTimeMinute;

            branch.LunchBreak = settings.lunchBreak;
            branch.LunchBreakStartHour = settings.lunchBreakStartHour;
            branch.LunchBreakStartMinute = settings.lunchBreakStartMinute;
            branch.LunchBreakEndHour = settings.lunchBreakEndHour;
            branch.LunchBreakEndMinute = settings.lunchBreakEndMinute;

            branch.AppointmentTime = settings.appointmentTime;
            branch.AppointmentCount = settings.appointmentCount;
            branch.AppointmentNotifyTime = settings.appointmentNotifyTime;

            await _context.SaveAsync(this);
            return NoContent();
        }
    }
}
