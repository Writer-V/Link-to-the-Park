using System.Collections.Generic;

namespace LinkToThePark.World.Dialog;
public class DialogEntry
{
    public string Name {get; set;} = "";
    public string Sprite {get; set;} = "";
    public List<List<string>> Steps {get; set;} = new();
}