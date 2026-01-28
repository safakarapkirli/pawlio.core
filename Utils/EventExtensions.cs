using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Pawlio;
using Pawlio.Models;
using Pawlio.Controllers;

public static class EventExtension
{
    public static async Task<Event> AddEvent(this ApiController @this, PostgreSqlDbContext context, EventTypes eventType, Event @event)
    {
        var user = @this.GetUser();
        if (@event.UserId == 0) @event.UserId = user.Id;
        if (@event.FirmId == 0) @event.FirmId = user.FirmId;
        if (@event.BranchId == 0) @event.BranchId = user.BranchId;
        @event.TypeId = (int)eventType;//eventTypes.FirstOrDefault(et => et.Id == (int)eventType)!;
        context.Events.Add(@event);
        await context.SaveAsync(@this);
        return @event;
    }
}
