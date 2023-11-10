using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            this.context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await this.context.Connections.FindAsync(connectionId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await this.context.Groups.Include(e => e.Connections).FirstOrDefaultAsync(e => e.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = this.context.Messages.OrderByDescending(e => e.MessageSend).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(e => e.RecipientUsername == messageParams.Username && e.RecipientDeleted == false),
                "Outbox" => query.Where(e => e.SenderUsername == messageParams.Username && e.SenderDeleted == false),
                _ => query.Where(e => e.RecipientUsername == messageParams.Username && e.RecipientDeleted == false && e.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await this.context.Messages.Include(e => e.Sender).ThenInclude(e => e.Photos).Include(e => e.Recipient).ThenInclude(e => e.Photos)
                            .Where(e => e.RecipientUsername == currentUserName && e.RecipientDeleted == false && e.SenderUsername == recipientUserName ||
                                    e.RecipientUsername == recipientUserName && e.SenderDeleted == false && e.SenderUsername == currentUserName).OrderBy(e => e.MessageSend).ToListAsync();

            var unreadMessages = messages.Where(e => e.DateRead == null && e.RecipientUsername == currentUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                await this.context.SaveChangesAsync();
            }

            return this.mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public void RemoveConnection(Connection connection)
        {
            this.context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.context.SaveChangesAsync() > 0;
        }
    }
}