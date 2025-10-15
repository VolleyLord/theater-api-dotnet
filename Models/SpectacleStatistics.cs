public class SpectacleStatistics
{
    public int SpectacleId { get; set; }
    public string Title { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public int BookedSeats { get; set; }
    public int BoughtSeats { get; set; }
    public decimal MoneyEarned { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
}
