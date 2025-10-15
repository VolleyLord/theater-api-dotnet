
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Spectacle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public DateTime start_time { get; set; }
    public TimeSpan duration { get; set; }
    public decimal price { get; set; }
    public int? hall_id { get; set; } 
    public Hall hall { get; set; } 
    public int seats_booked { get; set; }
    public int seats_bought { get; set; }
    public decimal money_earned { get; set; }
}
