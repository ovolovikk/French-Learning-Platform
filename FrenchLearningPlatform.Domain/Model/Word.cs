using System;
using System.Collections.Generic;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Word :BaseEntity
{
    public int? CategoryId { get; set; }

    public string? FrenchTerm { get; set; }

    public string? Translation { get; set; }

    public int? DifficultyLevel { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
