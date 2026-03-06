using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FrenchLearningPlatform.Domain;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Category : BaseEntity
{
    [Required(ErrorMessage = "Назва категорії є обов'язковою")]
    [Display(Name = "Назва")]
    public string? Name { get; set; }

    [Display(Name = "Опис")]
    public string? Description { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();

    public virtual ICollection<Word> Words { get; set; } = new List<Word>();
}
