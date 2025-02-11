using Go2GrooveApi.Domain.Dtos;
using Go2GrooveApi.Domain.Models;
using Go2GrooveApi.Extensions;
using Go2GrooveApi.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Go2GrooveApi.Hubs
{
    [Authorize]
    public class ChatHub(UserManager<ApplicationUser> _userManager, Go2GrooveDbContext _context) : Hub
    {
        public static readonly ConcurrentDictionary<string, OnlineUserDto> onlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            var receiverId = httpContext?.Request.Query["senderId"].ToString();

            var userName = Context.User!.Identity!.Name;

            var currentUser = await _userManager.FindByNameAsync(userName);

            var connectedId = Context.ConnectionId;

            if(onlineUsers.ContainsKey(userName) )
            {
                onlineUsers[userName].ConnectionId = connectedId;
            }
            else
            {
                var user = new OnlineUserDto
                {
                    ConnectionId = connectedId,
                    UserName = userName,
                    ProfilePicture = currentUser!.ProfilePicture,
                    FullName = currentUser!.FullName
                };

                onlineUsers.TryAdd(userName, user);

                await Clients.AllExcept(connectedId).SendAsync("Notify", currentUser);
            }

            if(!string.IsNullOrEmpty(receiverId))
            {
                await LoadMessages(receiverId);
            }

            await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
        }

        public async Task LoadMessages(string receiverId, int pageNumber = 1)
        {
            int pageSize = 10;

            var userName = Context.User!.Identity.Name;

            var currentUser = await _userManager.FindByNameAsync(userName);

            if(currentUser is null)
            {
                return;
            }

            List<MessageReponseDto> messages = await _context.Chats.Where(x => x.ReceiverId == currentUser!.Id && 
                x.SenderId == receiverId || x.SenderId == currentUser!.Id && x.ReceiverId == receiverId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber -1 ) * pageSize)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new MessageReponseDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    ReceiverId = x.ReceiverId,
                    SenderId = x.SenderId,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            foreach(var message in messages)
            {
                var msg = await _context.Chats.FirstOrDefaultAsync(x => x.Id == message.Id);

                if(msg != null && msg.ReceiverId == currentUser.Id)
                {
                    msg.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }

            await Clients.User(currentUser.Id).SendAsync("ReceiveMessageList", messages);
        }

        public async Task SendMessage(MessageDto message)
        {
            var senderId = Context.User!.Identity.Name;

            var receiverId = message.ReceiverId;

            var newMessage = new Chat
            {
                Sender = await _userManager.FindByNameAsync(senderId!),
                Receiver = await _userManager.FindByIdAsync(receiverId!),
                IsRead = false,
                Content = message.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Chats.Add(newMessage);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId!).SendAsync("ReceiveNewMessage", newMessage);
        }

        public async Task NotifyTyping(string receiverUserName)
        {
            var senderUserName = Context.User!.Identity.Name;

            if(senderUserName is null)
            {
                return;
            }

            var connectionId = onlineUsers.Values.FirstOrDefault(x => x.UserName == receiverUserName)?.ConnectionId;

            if(connectionId != null)
            {
                await Clients.Client(connectionId).SendAsync("NotifyTyping", senderUserName);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User!.Identity.Name;

            onlineUsers.TryRemove(userName, out _);

            await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
        }

        private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
        {
            var userName = Context.User!.Identity.Name;

            var onlineUsersSet = new HashSet<string>(onlineUsers.Keys);

            var users = await _userManager.Users.Select(u => new OnlineUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                ProfilePicture = u.ProfilePicture,
                IsOnline = onlineUsersSet.Contains(u.UserName!),
                UnreadCount = _context.Chats.Count(x => x.ReceiverId == userName &&
                x.SenderId == u.Id && !x.IsRead)
            }).OrderByDescending(u => u.IsOnline).ToListAsync();

            return users;
        }
    }
}
