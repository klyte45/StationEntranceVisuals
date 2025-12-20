using Game.Prefabs;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace StationEntranceVisuals.Systems.LineData;

/// <summary>
/// Burst-compatible version of LineDescriptor
/// </summary>
public struct NativeLineDescriptor
{
    public Entity Entity;
    public TransportType TransportType;
    public bool IsCargo;
    public bool IsPassenger;
    public int Number;
    public float4 Color;

    public NativeLineDescriptor(Entity entity, TransportType transportType, bool isCargo, bool isPassenger, int number, Color color)
    {
        Entity = entity;
        TransportType = transportType;
        IsCargo = isCargo;
        IsPassenger = isPassenger;
        Number = number;
        Color = new float4(color.r, color.g, color.b, color.a);
    }

    public Color GetUnityColor() => new Color(Color.x, Color.y, Color.z, Color.w);
}
