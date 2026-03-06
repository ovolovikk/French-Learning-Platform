using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Word : BaseEntity
{
    [Display(Name = "Категорія")]
    public int? CategoryId { get; set; }

    [Required(ErrorMessage = "Французьке слово є обов'язковим")]
    [Display(Name = "Французьке слово")]
    public string? FrenchTerm { get; set; }

    [Required(ErrorMessage = "Переклад є обов'язковим")]
    [Display(Name = "Переклад")]
    public string? Translation { get; set; }

    [Display(Name = "Рівень складності (1–5)")]
    [Range(1, 5, ErrorMessage = "Рівень складності має бути від 1 до 5")]
    public int? DifficultyLevel { get; set; }

    public virtual Category? Category { get; set; }
}
