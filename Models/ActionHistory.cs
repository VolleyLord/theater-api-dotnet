
public class ActionHistory
{
    public int id { get; set; }
    public int? user_id { get; set; } 
    public User user { get; set; }
    public string action_type { get; set; } 
    public string entity_type { get; set; } 
    public int entity_id { get; set; } 
    public string? old_value { get; set; } 
    public string? new_value { get; set; } 
    public string? metadata { get; set; } 
    public DateTime timestamp { get; set; }
}
