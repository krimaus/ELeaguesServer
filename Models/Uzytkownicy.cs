using System;
using System.Collections.Generic;

namespace ELeaguesServer.Models;

public partial class Uzytkownicy
{
    public int Iduzytkownika { get; set; }

    public string Nazwa { get; set; } = null!;

    public string Haslo { get; set; } = null!;

    public bool Administrator { get; set; }

    public virtual ICollection<Ligi> Ligis { get; set; } = new List<Ligi>();
}
