using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrenchLearningPlatform.Domain.Model;

public partial class TestAttempt : BaseEntity
{
    [Display(Name = "Студент")]
    public int? UserId { get; set; }

    [Display(Name = "Тест")]
    public int? TestId { get; set; }

    [Display(Name = "Результат (балів)")]
    public int? Score { get; set; }

    [Display(Name = "Помилки (JSON)")]
    public string? MistakesJsonb { get; set; }

    [Display(Name = "Завершено о")]
    public DateTime? CompletedAt { get; set; }

    public virtual Test? Test { get; set; }

    public virtual User? User { get; set; }
}
