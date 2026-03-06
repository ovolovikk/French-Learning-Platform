using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Profile : BaseEntity
{
    [Display(Name = "Користувач")]
    public int? UserId { get; set; }

    [Display(Name = "URL аватара")]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Про себе")]
    public string? Bio { get; set; }

    [Display(Name = "Мова інтерфейсу")]
    public string? PrefferedLanguage { get; set; }

    public virtual User? User { get; set; }
}
