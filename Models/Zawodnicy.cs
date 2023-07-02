using System;
using System.Collections.Generic;

namespace ELeaguesServer.Models;

public partial class Zawodnicy
{
    public int Idzawodnika { get; set; }

    public string Imie { get; set; } = null!;

    public string Nazwisko { get; set; } = null!;

    public string? Alias { get; set; }

    public virtual ICollection<Mecze> MeczeIdzawodnikadwaNavigations { get; set; } = new List<Mecze>();

    public virtual ICollection<Mecze> MeczeIdzawodnikajedenNavigations { get; set; } = new List<Mecze>();
}
