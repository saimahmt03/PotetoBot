using Microsoft.EntityFrameworkCore;
using PotetoBot.API.Entities;

namespace PotetoBot.API.Data;

public class DBContext : DbContext
{
    public DBContext(
        DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; }
}