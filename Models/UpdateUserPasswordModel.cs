using System.ComponentModel.DataAnnotations;

public class UpdateUserPasswordModel
{
    [Required(ErrorMessage = "Пароль обязателен.")]
    public string password { get; set; }
}