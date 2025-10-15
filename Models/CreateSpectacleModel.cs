using System.ComponentModel.DataAnnotations;


public class CreateSpectacleModel
{
    [Required(ErrorMessage = "Название спектакля обязательно.")]
    public string title { get; set; }

    public string description { get; set; }

    [Required(ErrorMessage = "Время начала обязательно.")]
    public DateTime start_time { get; set; }

    [Required(ErrorMessage = "Длительность обязательна.")]
    public TimeSpan duration { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительной.")]
    public decimal price { get; set; }

    [Required(ErrorMessage = "ID зала обязателен.")]
    public int hall_id { get; set; }
}
