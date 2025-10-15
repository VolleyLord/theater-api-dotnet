
public class Ticket
{
    public int id { get; set; }
    public int spectacle_id { get; set; }
    public Spectacle spectacle { get; set; }
    public int? user_id { get; set; } 
    public User user { get; set; }
    public int seat { get; set; } 
    public string status { get; set; } 
    public DateTime? reserved_until { get; set; } 
    public decimal price { get; set; }
}
