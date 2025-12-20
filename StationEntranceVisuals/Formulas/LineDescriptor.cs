using Game.Prefabs;
using StationEntranceVisuals.Systems;
using Unity.Collections;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

public record struct LineDescriptor(
    Entity Entity,
    TransportType TransportType,
    bool IsCargo,
    bool IsPassenger,
    FixedString32Bytes Acronym,
    int Number,
    FixedString32Bytes SmallName,
    UnityEngine.Color Color) : System.IEquatable<LineDescriptor>
{

    public string GetDisplayName()
    {
        return SEV_SettingSystem.Instance.LineDisplayName switch
        {
            Settings.LineDisplayNameOptions.Custom => SmallName.ToString(),
            Settings.LineDisplayNameOptions.WriteEverywhere => Acronym.ToString(),
            Settings.LineDisplayNameOptions.Generated => Number.ToString(),
            _ => SmallName.ToString()
        };
    }

    public string GetOrderingIndex()
    {
        return SEV_SettingSystem.Instance.LineDisplayName switch
        {
            Settings.LineDisplayNameOptions.Custom => SmallName.ToString(),
            Settings.LineDisplayNameOptions.WriteEverywhere => Acronym.ToString(),
            Settings.LineDisplayNameOptions.Generated => Number.ToString(),
            _ => SmallName.ToString()
        };
    }
    public bool Equals(LineDescriptor other)
    {
        return Entity.Equals(other.Entity);
    }
    public override int GetHashCode()
    {
        return Entity.GetHashCode();
    }
}