using System;
using System.Collections.Generic;

namespace ELeaguesServer.Models;

public partial class Mecze
{
    public int Idmeczu { get; set; }

    public int? Idturnieju { get; set; }

    public int? Idzawodnikajeden { get; set; }

    public int? Idzawodnikadwa { get; set; }

    public int? Wynikjeden { get; set; }

    public int? Wynikdwa { get; set; }

    public int? Idnastepnegomeczu { get; set; }

    public virtual Mecze? IdnastepnegomeczuNavigation { get; set; }

    public virtual Turnieje? IdturniejuNavigation { get; set; }

    public virtual Uzytkownicy? IdzawodnikadwaNavigation { get; set; }

    public virtual Uzytkownicy? IdzawodnikajedenNavigation { get; set; }

    public virtual ICollection<Mecze> InverseIdnastepnegomeczuNavigation { get; set; } = new List<Mecze>();
}
