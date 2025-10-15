
public class User
{
    public int id { get; set; }
    public string email { get; set; }
    public string password_hash { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public bool is_blocked { get; set; }
    public DateTime? blocked_until { get; set; }
    public int role_id { get; set; }
    public Role role { get; set; }
    
}
