using System;
using System.Collections.Generic;
using Game.Prefabs;
using StationEntranceVisuals.Formulas;
using Unity.Entities;
using UnityEngine;

namespace StationEntranceVisuals.Utils;

public static class MockLineUtils
{

    private static readonly Dictionary<int, Color> SubwayColors = new()
    {
        { 0, Color.blue },
        { 1, Color.green },
        { 2, Color.red },
        { 3, Color.yellow },
        { 4, new Color(231, 39, 245, 0.8f) },
        { 5, new Color(255, 116, 0, 0.99f) },
        { 6, new Color(83, 3, 79, 0.99f) },
        { 7, Color.gray },
    };
    
    private static readonly Dictionary<int, Color> TrainColors = new()
    {
        { 0, Color.gray },
        { 1, new Color(83, 3, 79, 0.99f) },
        { 2, new Color(255, 116, 0, 0.99f) },
        { 3, new Color(231, 39, 245, 0.8f) },
        { 4, Color.yellow },
        { 5, Color.red },
        { 6, Color.green },
        { 7, Color.blue },
    };
    

    public static HashSet<LineDescriptor> GetMockLineDescriptors(Entity buildingRef, string lineType)
    {
        var transportType = (TransportType)Enum.Parse(typeof(TransportType), lineType);
        return transportType == TransportType.Train ? MockTrainLines(buildingRef) : MockSubwayLines(buildingRef);
    }

    private static HashSet<LineDescriptor> MockTrainLines(Entity buildingRef)
    {
        var lineDescriptors = new HashSet<LineDescriptor>();
        for (var i = 0; i < Mod.m_Setting.TrainLines; i++)
        {
            var lineColor = TrainColors.TryGetValue(i, out var color) ? color : Color.gray;
            lineDescriptors.Add(
                new LineDescriptor(
                    buildingRef,
                    TransportType.Train,
                    false,
                    true,
                    "T" + (i + 1),
                    (i + 1),
                    "T" + (i + 1),
                    lineColor
                )
            );
        }
        return lineDescriptors;
    }
    
    private static HashSet<LineDescriptor> MockSubwayLines(Entity buildingRef)
    {
        var lineDescriptors = new HashSet<LineDescriptor>();
        for (var i = 0; i < Mod.m_Setting.SubwayLines; i++)
        {
            var lineColor = SubwayColors.TryGetValue(i, out var color) ? color : Color.red;
            lineDescriptors.Add(
                new LineDescriptor(
                    buildingRef,
                    TransportType.Subway,
                    false,
                    true,
                    "S" + (i + 1),
                    (i + 1),
                    "S" + (i + 1),
                    lineColor
                )
            );
        }
        return lineDescriptors;
    }
}