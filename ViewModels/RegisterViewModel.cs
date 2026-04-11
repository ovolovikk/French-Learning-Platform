using System.ComponentModel.DataAnnotations;

namespace French_Learning_Platform.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Вкажіть ім'я")]
    [Display(Name = "Ім'я")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть прізвище")]
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть email")]
    [EmailAddress(ErrorMessage = "Некоректний email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть пароль")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Пароль має містити щонайменше 6 символів")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
    [Display(Name = "Підтвердження паролю")]
    public string PasswordConfirm { get; set; } = string.Empty;

    [Required(ErrorMessage = "Оберіть роль")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = "Student";
}
