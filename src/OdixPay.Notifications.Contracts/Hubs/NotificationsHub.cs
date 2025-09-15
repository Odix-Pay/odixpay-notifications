using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OdixPay.Notifications.API.Constants;
using OdixPay.Notifications.Contracts.Constants;
using OdixPay.Notifications.Contracts.Interfaces;

namespace OdixPay.Notifications.Contracts.Hubs;

/// <summary>
/// Manages real-time notifications for clients.
/// Connections are grouped by UserId to ensure targeted messaging.
/// Clients can also subscribe to (join) or unsubscribe from (leave) specific notification groups (e.g order updates - order_{orderId}).
/// </summary>
public class NotificationsHub(ILogger<NotificationsHub> logger, IHubAuthorizer hubAuthorizer) : Hub
{
    private readonly ILogger<NotificationsHub> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHubAuthorizer _hubAuthorizer = hubAuthorizer ?? throw new ArgumentNullException(nameof(hubAuthorizer));

    /// <summary>
    /// Called when a new client connects.
    /// The client is added to a group corresponding to their UserId.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var isAuth = Context.User?.Identity?.IsAuthenticated ?? false;
        if (isAuth)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("User connected: {userId} with ConnectionId: {connectionId}", userId, Context.ConnectionId);
                // Group connections by user ID to send targeted messages
                await JoinGroup(HubPrefixes.GetGroup(HubPrefixes.Users, [userId]));

                _logger.LogInformation("Connection {ConnectionId} established for User {UserId}", Context.ConnectionId, userId);

                // Optionally, send a welcome message or initial data
                await Clients.Caller.SendAsync("Connected", new { Message = "Connected to NotificationsHub", UserId = userId });
            }
        }
        else
        {
            _logger.LogWarning("Unauthenticated connection attempt. ConnectionId: {ConnectionId}", Context.ConnectionId);
            _logger.LogInformation("Allowing anonymous connection {ConnectionId}", Context.ConnectionId);

            await Clients.Caller.SendAsync("Connected", new { Message = "Connected to NotificationsHub", UserId = "Anonymous" });
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects.
    /// The client is removed from their user-specific group.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Connection {ConnectionId} disconnected. Reason: {Reason}", Context.ConnectionId, exception?.Message);
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await LeaveGroup(GetUserGroupName(userId));
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinAdminNotificationsGroup() // Allows permitted clients to join the admins group
    {
        _logger.LogInformation("Connection {ConnectionId} attempting to join Admins group", Context.ConnectionId);
        var isAuth = Context.User?.Identity?.IsAuthenticated ?? false;

        _logger.LogInformation("Connection {ConnectionId} authentication status: {IsAuthenticated}", Context.ConnectionId, isAuth);

        if (isAuth)
        {
            // Manually verify the user has the required permission to join the Admins group
            // This is necessary because SignalR does not natively support policy-based authorization on hub methods
            var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                _logger.LogWarning("Connection {ConnectionId} has no valid user identifier claim", Context.ConnectionId);
                await Clients.Caller.SendAsync("Error", new { Message = "Unauthorized to join Admins group" });
                return;
            }

            var isAuthorized = await _hubAuthorizer.AuthorizeAsync(userIdClaim, Permissions.Notification.ReadAdminNotifications);

            if (!isAuthorized)
            {
                _logger.LogWarning("Connection {ConnectionId} user {UserId} is not authorized to join Admins group", Context.ConnectionId, userIdClaim);
                await Clients.Caller.SendAsync("Error", new { Message = "Unauthorized to join Admins group" });
                return;
            }

            _logger.LogInformation("Authenticated connection {ConnectionId} verified for Admins group", Context.ConnectionId);

            await JoinGroup(HubPrefixes.GetGroup(HubPrefixes.Admins, [HubPrefixes.Notifications]));

            await Clients.Caller.SendAsync("JoinedAdminsGroup", new { Message = "Joined Admins group", UserId = userIdClaim });

            _logger.LogInformation("Connection {ConnectionId} joined Admins group", Context.ConnectionId);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", new { Message = "Unauthorized to join Admins group" });
            _logger.LogWarning("Unauthenticated connection attempt to join Admins group. ConnectionId: {ConnectionId}", Context.ConnectionId);
        }
    }




    // Join a Group
    private async Task JoinGroup(string groupId) // Allows clients to join arbitrary groups
    {
        _logger.LogInformation("Connection {ConnectionId} is attempting to join group {GroupId}", Context.ConnectionId, groupId);

        if (string.IsNullOrEmpty(groupId)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        _logger.LogInformation("Connection {ConnectionId} joined group {GroupId}", Context.ConnectionId, groupId);
    }
    // Leave a Group
    public async Task LeaveGroup(string groupId) // Allows clients to leave arbitrary groups
    {
        if (string.IsNullOrEmpty(groupId)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        _logger.LogInformation("Connection {ConnectionId} left group {GroupId}", Context.ConnectionId, groupId);
    }

    public static string GetUserGroupName(string userId) => $"user_{userId}";
}
