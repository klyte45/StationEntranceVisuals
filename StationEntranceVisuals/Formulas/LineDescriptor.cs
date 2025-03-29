using Game.Prefabs;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

internal record struct LineDescriptor(
    Entity LineEntity,
    TransportType TransportType,
    bool IsCargo,
    bool IsPassenger,
    string LineNumber,
    UnityEngine.Color LineColor)
{
}