using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Test : BaseEntity
{
    [Display(Name = "Категорія")]
    public int? CategoryId { get; set; }

    [Display(Name = "Кількість слів")]
    public int? Words { get; set; }

    [Required(ErrorMessage = "Назва тесту є обов'язковою")]
    [Display(Name = "Назва тесту")]
    public string? Title { get; set; }

    [Display(Name = "Ліміт часу (секунди)")]
    public int? TimeLimitSeconds { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
}
