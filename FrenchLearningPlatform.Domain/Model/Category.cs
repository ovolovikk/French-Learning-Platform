using System;
using System.Collections.Generic;
using FrenchLearningPlatform.Domain;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Category : BaseEntity
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();

    public virtual ICollection<Word> Words { get; set; } = new List<Word>();
}
