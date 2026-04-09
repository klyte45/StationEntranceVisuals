using StationEntranceVisuals.Components.Shareable;
using StationEntranceVisuals.Utils;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace StationEntranceVisuals.WE_TFMBridge
{
    public static class WE_TFMBuildingLineCacheBridge
    {
        [PatchGenericMethod(typeof(LineDescriptor))]
        public unsafe static List<LineDescriptor> GetLines(Entity selectedEntity, bool iterateToOwner) => throw new NotImplementedException("Stub only!");
        [PatchGenericMethod(typeof(LineDescriptor))]
        public unsafe static List<LineDescriptor> GetFilteredLines(Entity buildingEntity, string lineType, bool iterateToOwner, bool sortDescending) => throw new NotImplementedException("Stub only!");
        [PatchGenericMethod(typeof(LineDescriptor))]
        public unsafe static LineDescriptor? GetLineByIndex(Entity buildingEntity, string lineType, int index, bool iterateToOwner, bool sortDescending) => throw new NotImplementedException("Stub only!");
    }
}
