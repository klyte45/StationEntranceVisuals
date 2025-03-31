using Game.Prefabs;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

public record struct LineDescriptor(
    Entity Entity,
    TransportType TransportType,
    bool IsCargo,
    bool IsPassenger,
    string Acronym,
    int Number,
    UnityEngine.Color Color)
{
}