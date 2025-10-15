using System.ComponentModel.DataAnnotations;

public class UpdateHallModel
{
    [Required(ErrorMessage = "Название зала обязательно.")]
    public string name { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Вместимость должна быть положительной.")]
    public int capacity { get; set; }
}