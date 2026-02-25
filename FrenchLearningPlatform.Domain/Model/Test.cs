using System;
using System.Collections.Generic;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Test :BaseEntity
{
    public int? CategoryId { get; set; }

    public int? Words { get; set; }

    public string? Title { get; set; }

    public int? TimeLimitSeconds { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
}
