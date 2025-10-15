
public class UserVisit
{
    public int id { get; set; }
    public int user_id { get; set; }
    public User user { get; set; }
    public int spectacle_id { get; set; }
    public Spectacle spectacle { get; set; }
    public int seat { get; set; }
    public decimal price { get; set; }
    public DateTime visit_date { get; set; }
}
