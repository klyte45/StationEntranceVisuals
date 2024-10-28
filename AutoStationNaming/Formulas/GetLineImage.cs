using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutoStationNaming.Utils;
using Colossal.Entities;
using Game.Common;
using Game.Routes;
using Game.UI;
using Unity.Entities;
using SubObject = Game.Objects.SubObject;

namespace AutoStationNaming.Formulas;

public class StationImages
{
    
    private static NameSystem _nameSystem;
    private const string SubwayEntityName = "SubwayLine";
    private const string TrainEntityName = "PassengerTrainLine";
    private const string TransparentImage = "Transparent";
    private const string ViaMobilidadeImage = "ViaMobilidadeOperator";
    private const string CptmImage = "CptmOperator";
    private const string MetroImage = "MetroOperator";
    private const string White = "White";
    private const string Black = "Black";

    private static readonly string[] ViaMobilidadeLines = ["4", "5", "6", "8", "9", "15"];
    
    private static void GetFirstLineInfo(EntityManager entityManager, Entity selectedEntity, ref List<Tuple<String, String>> lineNumberList)
    {
        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<ConnectedRoute> dynamicBuffer))
        {
            for (var i = 0; i < dynamicBuffer.Length; i++)
            {
                if (entityManager.TryGetComponent<Owner>(dynamicBuffer[i].m_Waypoint, out var owner))
                {
                    var lettersRegex = new Regex("[^a-zA-Z]+");
                    var entityDebugName = _nameSystem.GetDebugName(owner.m_Owner);
                    var entityName = lettersRegex.Replace(entityDebugName, "");
                    if (entityManager.TryGetComponent<RouteNumber>(owner.m_Owner, out var routeNumber))
                    {
                        var lineNumber = string.Join("", _nameSystem.GetRenderedLabelName(owner.m_Owner).Where(Char.IsDigit));
                        var routeString = lineNumber.Length > 0 ? lineNumber : routeNumber.m_Number.ToString();
                        lineNumberList.Add(Tuple.Create(entityName, routeString));
                    }
                }
            }
        }

        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<SubObject> subObjects))
        {
            for (var i = 0; i < subObjects.Length; i++)
            {
                GetFirstLineInfo(entityManager, subObjects[i].m_SubObject, ref lineNumberList);
            }
        }
    }

    private static List<Tuple<string, string>> GetFilteredLinesList(Entity buildingRef)
    {
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var lineNumberList = new List<Tuple<String, String>>();
        GetFirstLineInfo(entityManager, buildingRef, ref lineNumberList);
        
        var lineNumberOwnerList = new List<Tuple<String, String>>();
        entityManager.TryGetComponent<Owner>(buildingRef, out var owner);
        GetFirstLineInfo(entityManager, owner.m_Owner, ref lineNumberOwnerList);

        return lineNumberList
            .Concat(lineNumberOwnerList)
            .Distinct(new TupleComparer<string>())
            .ToList();
    }

    private static string GetLineOrTransparent(Entity buildingRef, int index, string lineType)
    {
        var lineNumber = GetFilteredLinesList(buildingRef)
            .OrderBy(t => int.Parse(t.Item2))
            .Where(x => x.Item1 == lineType)
            .ElementAtOrValue(index, Tuple.Create(TransparentImage, TransparentImage)).Item2;
        return lineNumber.Contains(TransparentImage) ? TransparentImage : "Line" + lineNumber;
    }
    
    public static readonly Func<Entity, string> GetFirstSubwayImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 0, SubwayEntityName);

    public static readonly Func<Entity, string> GetSecondSubwayImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 1, SubwayEntityName);
    
    public static readonly Func<Entity, string> GetThirdSubwayImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 2, SubwayEntityName);

    public static readonly Func<Entity, string> GetFourthSubwayImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 3, SubwayEntityName);
    
    public static readonly Func<Entity, string> GetFifthSubwayImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 4, SubwayEntityName);
    
    
    public static readonly Func<Entity, string> GetFirstTrainImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 0, TrainEntityName);

    public static readonly Func<Entity, string> GetSecondTrainImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 1, TrainEntityName);
    
    public static readonly Func<Entity, string> GetThirdTrainImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 2, TrainEntityName);

    public static readonly Func<Entity, string> GetFourthTrainImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 3, TrainEntityName);
    
    public static readonly Func<Entity, string> GetFifthTrainImageBinding = (buildingRef) => GetLineOrTransparent(buildingRef, 4, TrainEntityName);
    
    public static readonly Func<Entity, string> GetSubwayStationOperatorImageBinding = (buildingRef) =>
    {
        var subwayLines = GetFilteredLinesList(buildingRef)
            .Where(x => x.Item1 == SubwayEntityName).ToList();
        var hasViaMobilidadeLine = subwayLines.Where(x => ViaMobilidadeLines
            .Where(y => y == x.Item2).ToList().Count > 0).Count() > 0;
        return subwayLines.Count > 0 ? (hasViaMobilidadeLine ? ViaMobilidadeImage : MetroImage) + Black : TransparentImage;
    };
    
    public static readonly Func<Entity, string> GetTrainStationOperatorImageBinding = (buildingRef) =>
    {
        var trainLines = GetFilteredLinesList(buildingRef)
            .Where(x => x.Item1 == SubwayEntityName).ToList();
        return (trainLines.Count > 0) ? (CptmImage + Black) : TransparentImage;
    };

    public static string GetFirstSubwayLineImage(Entity buildingRef) => GetFirstSubwayImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetSecondSubwayLineImage(Entity buildingRef) => GetSecondSubwayImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetThirdSubwayLineImage(Entity buildingRef) => GetThirdSubwayImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetFourthSubwayLineImage(Entity buildingRef) => GetFourthSubwayImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetFifthSubwayLineImage(Entity buildingRef) => GetFifthSubwayImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetFirstTrainLineImage(Entity buildingRef) => GetFirstTrainImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetSecondTrainLineImage(Entity buildingRef) => GetSecondTrainImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetThirdTrainLineImage(Entity buildingRef) => GetThirdTrainImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetFourthTrainLineImage(Entity buildingRef) => GetFourthTrainImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetFifthTrainLineImage(Entity buildingRef) => GetFifthTrainImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetSubwayStationOperatorImage(Entity buildingRef) => GetSubwayStationOperatorImageBinding?.Invoke(buildingRef) ?? TransparentImage;
    
    public static string GetTrainStationOperatorImage(Entity buildingRef) => GetTrainStationOperatorImageBinding?.Invoke(buildingRef) ?? TransparentImage;
}