using System.ComponentModel.DataAnnotations;
using French_Learning_Platform.Security;

namespace French_Learning_Platform.ViewModels;

public class ChangeRoleViewModel
{
    public int UserId { get; set; }

    [Display(Name = "Email")]
    public string UserEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Оберіть роль")]
    [Display(Name = "Роль")]
    public string SelectedRole { get; set; } = AppRoles.Student;

    public IReadOnlyList<string> AvailableRoles { get; } =
        new[] { AppRoles.Teacher, AppRoles.Student };
}
