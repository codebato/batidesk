using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WestDesk.Infrastructure.Hubs
{
    [Authorize]
    public class TicketHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var tenantId = Context.User?.FindFirstValue("tenantId");
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant-{tenantId}");
            }
            await base.OnConnectedAsync();
        }
    }
}