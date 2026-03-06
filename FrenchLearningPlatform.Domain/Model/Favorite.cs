using System;
using System.Collections.Generic;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Favorite
{
    public int UserId { get; set; }

    public int WordId { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Word Word { get; set; } = null!;
}
