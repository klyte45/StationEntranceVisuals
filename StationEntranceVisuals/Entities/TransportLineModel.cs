using Color = UnityEngine.Color;

namespace StationEntranceVisuals.Entities;

public class TransportLineModel(string type, string name, Color  color)
{
    public string Type = type;
    public string Name = name;
    public Color Color = color;
}