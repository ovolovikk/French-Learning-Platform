using System;
using System.Collections.Generic;

namespace FrenchLearningPlatform.Domain.Model;

public partial class TestAttempt :BaseEntity
{

    public int? UserId { get; set; }

    public int? TestId { get; set; }

    public int? Score { get; set; }

    public string? MistakesJsonb { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Test? Test { get; set; }

    public virtual User? User { get; set; }
}
