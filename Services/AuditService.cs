using System.Text.Json;
using System.Text.Json.Serialization;

public class AuditService
{
    private readonly AppDbContext _context;
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAction(int? userId, string actionType, string entityType, int entityId, object oldValue, object newValue, string metadata = null)
    {
        var log = new ActionHistory
        {
            user_id = userId,
            action_type = actionType,
            entity_type = entityType,
            entity_id = entityId,
            old_value = oldValue != null ? JsonSerializer.Serialize(oldValue, _jsonOptions) : null,
            new_value = newValue != null ? JsonSerializer.Serialize(newValue, _jsonOptions) : null,
            metadata = metadata,
            timestamp = DateTime.UtcNow
        };

        _context.ActionHistory.Add(log);
        await _context.SaveChangesAsync();
    }
}
