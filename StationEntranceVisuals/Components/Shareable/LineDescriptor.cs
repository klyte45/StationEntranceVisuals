using Game.Prefabs;
using StationEntranceVisuals.Systems;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;

namespace StationEntranceVisuals.Components.Shareable
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LineDescriptor : System.IEquatable<LineDescriptor>
    {
        public Entity Entity;
        public TransportType TransportType;
        public bool IsCargo;
        public bool IsPassenger;
        public FixedString32Bytes Acronym;
        public int Number;
        public UnityEngine.Color Color;
        public FixedString32Bytes SmallName;

        public LineDescriptor(
            Entity entity,
            TransportType transportType,
            bool isCargo,
            bool isPassenger,
            FixedString32Bytes acronym,
            int number,
            UnityEngine.Color color,

    FixedString32Bytes smallName)
        {
            Entity = entity;
            TransportType = transportType;
            IsCargo = isCargo;
            IsPassenger = isPassenger;
            Acronym = acronym;
            Number = number;
            Color = color;
            SmallName = smallName;
        }

        public bool Equals(LineDescriptor other)
        {
            return Entity.Equals(other.Entity);
        }

        public override bool Equals(object obj)
        {
            return obj is LineDescriptor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Entity.GetHashCode();
        }
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
    }
}
