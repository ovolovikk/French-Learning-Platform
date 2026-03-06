using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrenchLearningPlatform.Domain.Model;

public partial class User : BaseEntity
{
    [Required(ErrorMessage = "Email є обов'язковим")]
    [EmailAddress(ErrorMessage = "Некоректний формат Email")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Пароль є обов'язковим")]
    [Display(Name = "Пароль (хеш)")]
    public string? PasswordHash { get; set; }

    [Display(Name = "Роль")]
    public string? Role { get; set; }

    [Display(Name = "Email підтверджено")]
    public bool? IsEmailConfirmed { get; set; }

    [Display(Name = "Зареєстровано")]
    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Profile> Profiles { get; set; } = new List<Profile>();

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
}
