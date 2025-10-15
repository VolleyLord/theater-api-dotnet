using System.ComponentModel.DataAnnotations;

public class CreateUserModel
{
    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    public string email { get; set; }

    [Required(ErrorMessage = "Пароль обязателен.")]
    public string password { get; set; }

    [Required(ErrorMessage = "Имя обязательно.")]
    public string first_name { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна.")]
    public string last_name { get; set; }

    [Required(ErrorMessage = "ID роли обязательно.")]
    public int role_id { get; set; }
}