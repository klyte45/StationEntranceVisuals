using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoStationNaming.Entities;
using AutoStationNaming.Utils;
using Colossal;
using Colossal.Entities;
using Game.Buildings;
using Game.Common;
using Game.Prefabs;
using Game.Routes;
using Game.UI;
using Unity.Entities;
using UnityEngine;
using Color = Game.Objects.Color;
using SubNet = Game.Net.SubNet;

namespace AutoStationNaming.Formulas;

public class StationUtils
{
    
    private static NameSystem _nameSystem;
    private const string SubwayEntityName = "SubwayLine";
    private const string TrainEntityName = "PassengerTrainLine";
    private const string TransparentImage = "Transparent";
    private const string Empty = "";
    private const string ViaMobilidadeImage = "ViaMobilidadeOperator";
    private const string CptmImage = "CptmOperator";
    private const string MetroImage = "MetroOperator";
    private const string White = "White";
    private const string Black = "Black";

    private static readonly string[] ViaMobilidadeLines = ["4", "5", "6", "8", "9", "15"];
    
    private static void GetLines(EntityManager entityManager, Entity selectedEntity, ref List<TransportLineModel> lineNumberList)
    {
        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<ConnectedRoute> routes))
        {
            foreach (var route in routes) {
                if (entityManager.TryGetComponent<Owner>(route.m_Waypoint, out var owner))
                {
                
                    if (entityManager.TryGetComponent<RouteNumber>(owner.m_Owner, out var routeNumber)) {
                        var lettersRegex = new Regex("[^a-zA-Z]+");
                        var entityDebugName = _nameSystem.GetDebugName(owner.m_Owner);
                        var entityName = lettersRegex.Replace(entityDebugName, "");
                        entityManager.TryGetComponent<Color>(owner.m_Owner, out var routeColor);
                        var lineNumber = string.Join("", _nameSystem.GetRenderedLabelName(owner.m_Owner).Where(Char.IsDigit));
                        var routeString = lineNumber.Length > 0 ? lineNumber : routeNumber.m_Number.ToString();
                        lineNumberList.Add(new TransportLineModel(entityName, routeString, ColorUtils.GetColor(routeColor.m_Value)));
                    }
                   
                }
            }
        }

        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<SubObject> subObjects))
        {
            foreach (var subObject in subObjects)
            {
                GetLines(entityManager, subObject.m_Prefab, ref lineNumberList);
            }
        }
    }

    private static List<TransportLineModel> GetFilteredLinesList(Entity buildingRef)
    {
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var lineNumberList = new List<TransportLineModel>();
        GetLines(entityManager, buildingRef, ref lineNumberList);
        
        var lineNumberOwnerList = new List<TransportLineModel>();
        entityManager.TryGetComponent<Owner>(buildingRef, out var owner);
        GetLines(entityManager, owner.m_Owner, ref lineNumberOwnerList);

        return lineNumberList
            .Concat(lineNumberOwnerList)
            .ToList()
            .Distinct(new TransportLineComparer())
            .ToList();
    }

    private static TransportLineModel GetLine(Entity buildingRef, int index, string lineType)
    {
        return GetFilteredLinesList(buildingRef)
            .OrderBy(t => int.Parse(t.Name))
            .Where(x => x.Type == lineType)
            .ElementAtOrValue(index, new TransportLineModel(Empty, Empty, UnityEngine.Color.clear));
    }
    
    private static string GetLineName(Entity buildingRef, int index, string lineType)
    {
        return GetLine(buildingRef, index, lineType).Name;
    } 
    
    private static UnityEngine.Color GetLineColor(Entity buildingRef, int index, string lineType)
    {
        return GetLine(buildingRef, index, lineType).Color;
    } 
    
    public static readonly Func<Entity, string> GetFirstSubwayNameBinding = (buildingRef) => GetLineName(buildingRef, 0, SubwayEntityName);

    public static readonly Func<Entity, string> GetSecondSubwayNameBinding = (buildingRef) => GetLineName(buildingRef, 1, SubwayEntityName);
    
    public static readonly Func<Entity, string> GetThirdSubwayNameBinding = (buildingRef) => GetLineName(buildingRef, 2, SubwayEntityName);

    public static readonly Func<Entity, string> GetFourthSubwayNameBinding = (buildingRef) => GetLineName(buildingRef, 3, SubwayEntityName);
    
    public static readonly Func<Entity, string> GetFifthSubwayNameBinding = (buildingRef) => GetLineName(buildingRef, 4, SubwayEntityName);
    
    
    public static readonly Func<Entity, string> GetFirstTrainNameBinding = (buildingRef) => GetLineName(buildingRef, 0, TrainEntityName);

    public static readonly Func<Entity, string> GetSecondTrainNameBinding = (buildingRef) => GetLineName(buildingRef, 1, TrainEntityName);
    
    public static readonly Func<Entity, string> GetThirdTrainNameBinding = (buildingRef) => GetLineName(buildingRef, 2, TrainEntityName);

    public static readonly Func<Entity, string> GetFourthTrainNameBinding = (buildingRef) => GetLineName(buildingRef, 3, TrainEntityName);
    
    public static readonly Func<Entity, string> GetFifthTrainNameBinding = (buildingRef) => GetLineName(buildingRef, 4, TrainEntityName);
    
    
    public static readonly Func<Entity, UnityEngine.Color> GetFirstSubwayColorBinding = (buildingRef) => GetLineColor(buildingRef, 0, SubwayEntityName);
    
    public static readonly Func<Entity, string> GetSubwayStationOperatorImageBinding = (buildingRef) =>
    {
        var subwayLines = GetFilteredLinesList(buildingRef)
            .Where(x => x.Type == SubwayEntityName).ToList();
        var hasViaMobilidadeLine = subwayLines.Where(x => ViaMobilidadeLines
            .Where(y => y == x.Name).ToList().Count > 0).Count() > 0;
        return subwayLines.Count > 0 ? (hasViaMobilidadeLine ? ViaMobilidadeImage : MetroImage) + Black : TransparentImage;
    };
    
    public static readonly Func<Entity, string> GetTrainStationOperatorImageBinding = (buildingRef) =>
    {
        var trainLines = GetFilteredLinesList(buildingRef)
            .Where(x => x.Type == SubwayEntityName).ToList();
        return (trainLines.Count > 0) ? (CptmImage + Black) : TransparentImage;
    };
    
    public static UnityEngine.Color GetFirstSubwayLineColor(Entity buildingRef) => GetFirstSubwayColorBinding?.Invoke(buildingRef) ?? UnityEngine.Color.clear;
    
    public static UnityEngine.Color GetSecondSubwayLineColor(Entity buildingRef) => GetFirstSubwayColorBinding?.Invoke(buildingRef) ?? UnityEngine.Color.clear;
    
    public static UnityEngine.Color GetThirdSubwayLineColor(Entity buildingRef) => GetFirstSubwayColorBinding?.Invoke(buildingRef) ?? UnityEngine.Color.clear;
    
    public static UnityEngine.Color GetFourthSubwayLineColor(Entity buildingRef) => GetFirstSubwayColorBinding?.Invoke(buildingRef) ?? UnityEngine.Color.clear;

    public static string GetFirstSubwayLineName(Entity buildingRef) => GetFirstSubwayNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetSecondSubwayLineName(Entity buildingRef) => GetSecondSubwayNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetThirdSubwayLineName(Entity buildingRef) => GetThirdSubwayNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetFourthSubwayLineName(Entity buildingRef) => GetFourthSubwayNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetFifthSubwayLineName(Entity buildingRef) => GetFifthSubwayNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetFirstTrainLineName(Entity buildingRef) => GetFirstTrainNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetSecondTrainLineName(Entity buildingRef) => GetSecondTrainNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetThirdTrainLineName(Entity buildingRef) => GetThirdTrainNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetFourthTrainLineName(Entity buildingRef) => GetFourthTrainNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetFifthTrainLineName(Entity buildingRef) => GetFifthTrainNameBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetSubwayStationOperatorName(Entity buildingRef) => GetSubwayStationOperatorImageBinding?.Invoke(buildingRef) ?? Empty;
    
    public static string GetTrainStationOperatorName(Entity buildingRef) => GetTrainStationOperatorImageBinding?.Invoke(buildingRef) ?? Empty;
}