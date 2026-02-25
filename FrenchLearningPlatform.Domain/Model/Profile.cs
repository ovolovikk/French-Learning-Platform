using System;
using System.Collections.Generic;

namespace FrenchLearningPlatform.Domain.Model;

public partial class Profile :BaseEntity
{

    public int? UserId { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public string? PrefferedLanguage { get; set; }

    public virtual User? User { get; set; }
}
