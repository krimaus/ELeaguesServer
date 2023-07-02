using System;
using System.Collections.Generic;

namespace ELeaguesServer.Models;

public partial class Turnieje
{
    public int Idturnieju { get; set; }

    public int Idligi { get; set; }

    public int Liczbarund { get; set; }

    public virtual Ligi IdligiNavigation { get; set; } = null!;

    public virtual ICollection<Mecze> Meczes { get; set; } = new List<Mecze>();
}
