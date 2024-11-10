using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StationEntranceVisuals.Entities;
using StationEntranceVisuals.Utils;
using Colossal.Entities;
using Game.Common;
using Game.Routes;
using Game.UI;
using Game.UI.InGame;
using Unity.Entities;
using Color = Game.Routes.Color;
using SubObject = Game.Objects.SubObject;

namespace StationEntranceVisuals.Formulas;

public static class LinesUtils
{
    
    private static NameSystem _nameSystem;
    public const string SubwayEntityName = "SubwayLine";
    public const string TrainEntityName = "PassengerTrainLine";
    public const string TransparentImage = "Transparent";
    public const string Empty = "";
    public const string ViaMobilidadeImage = "ViaMobilidadeOperator";
    public const string CptmImage = "CptmOperator";
    public const string MetroImage = "MetroOperator";
    public const string White = "White";
    public const string Black = "Black";

    public static readonly string[] GetViaMobilidadeLines = ["4", "5", "6", "8", "9", "15"];
    
    private static void GetLines(EntityManager entityManager, Entity selectedEntity, ref List<TransportLineModel> lineNumberList)
    {
        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<ConnectedRoute> routes))
        {
            foreach (var route in routes) {
                if (entityManager.TryGetComponent<Owner>(route.m_Waypoint, out var owner))
                {
                    if (entityManager.TryGetComponent<RouteNumber>(owner.m_Owner, out var routeNumber))
                    {
                        var lettersRegex = new Regex("[^a-zA-Z]+");
                        var entityDebugName = _nameSystem.GetDebugName(owner.m_Owner);
                        var entityName = lettersRegex.Replace(entityDebugName, "");
                        entityManager.TryGetComponent<Color>(owner.m_Owner, out var routeColor);
                        var lineNumber = string.Join("", _nameSystem.GetRenderedLabelName(owner.m_Owner).Where(Char.IsDigit));
                        var routeString = lineNumber.Length > 0 ? lineNumber : routeNumber.m_Number.ToString();
                        lineNumberList.Add(new TransportLineModel(entityName, routeString, routeColor.m_Color));
                    }
                }
            }
        }
        
        if (entityManager.TryGetBuffer(selectedEntity, true, out DynamicBuffer<SubObject> subObjects))
        {
            foreach (var subObject in subObjects)
            {
                GetLines(entityManager, subObject.m_SubObject, ref lineNumberList);
            }
        }
    }

    internal static List<TransportLineModel> GetFilteredLinesList(Entity buildingRef)
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
        _nameSystem ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NameSystem>();

        return GetFilteredLinesList(buildingRef)
            .OrderBy(t => int.Parse(t.Name))
            .Where(x => x.Type == lineType)
            .ElementAtOrValue(index, new TransportLineModel(Empty, Empty, UnityEngine.Color.clear));
    }

    private static List<TransportLineModel> GetAllLinesList()
    {
        return null;
    }
    
    public static string GetLineName(Entity buildingRef, int index, string lineType)
    {
        return GetLine(buildingRef, index, lineType).Name;
    } 
    
    public static UnityEngine.Color GetLineColor(Entity buildingRef, int index, string lineType)
    {
        return GetLine(buildingRef, index, lineType).Color;
    }
}