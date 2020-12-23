using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebApi.Entities;

namespace WebApi.Helpers
{
    [Authorize]
    public class NotiHub : Hub
    {
        public async Task Approsve(string message)
        {
            await Clients.All.SendAsync("approve", "loooooooooooooooooo");
        }
    }

}
