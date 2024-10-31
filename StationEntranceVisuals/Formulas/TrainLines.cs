using System;
using System.Linq;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

public static class TrainLines
{
    private static readonly Func<Entity, string> FirstLineNameBinding = (buildingRef) => 
        LinesUtils.GetLineName(buildingRef, 0, LinesUtils.TrainEntityName);

    private static readonly Func<Entity, string> SecondLineNameBinding = (buildingRef) => 
        LinesUtils.GetLineName(buildingRef, 1, LinesUtils.TrainEntityName);

    private static readonly Func<Entity, string> ThirdLineNameBinding = (buildingRef) => 
        LinesUtils.GetLineName(buildingRef, 2, LinesUtils.TrainEntityName);

    private static readonly Func<Entity, string> FourthLineNameBinding = (buildingRef) => 
        LinesUtils.GetLineName(buildingRef, 3, LinesUtils.TrainEntityName);
    
    
    private static readonly Func<Entity, UnityEngine.Color> FirstLineColorBinding = (buildingRef) => 
        LinesUtils.GetLineColor(buildingRef, 0, LinesUtils.TrainEntityName);

    private static readonly Func<Entity, UnityEngine.Color> SecondLineColorBinding = (buildingRef) => 
        LinesUtils.GetLineColor(buildingRef, 1, LinesUtils.TrainEntityName);

    private static readonly Func<Entity, UnityEngine.Color> ThirdLineColorBinding = (buildingRef) => 
        LinesUtils.GetLineColor(buildingRef, 2, LinesUtils.TrainEntityName);

    private static readonly Func<Entity, UnityEngine.Color> FourthLineColorBinding = (buildingRef) => 
        LinesUtils.GetLineColor(buildingRef, 3, LinesUtils.TrainEntityName);


    private static readonly Func<Entity, string> TrainStationOperatorImageBinding = (buildingRef) =>
    {
        var trainLines = LinesUtils.GetFilteredLinesList(buildingRef)
            .Where(x => x.Type == LinesUtils.TrainEntityName).ToList();
        return (trainLines.Count > 0) ? (LinesUtils.CptmImage + LinesUtils.Black) : LinesUtils.TransparentImage;
    };
    
    public static string GetStationOperatorName(Entity buildingRef) => 
        TrainStationOperatorImageBinding?.Invoke(buildingRef) ?? LinesUtils.Empty;
    
    public static UnityEngine.Color GetFirstLineColor(Entity buildingRef) => 
        FirstLineColorBinding?.Invoke(buildingRef) ?? UnityEngine.Color.clear;
    
    public static UnityEngine.Color GetSecondLineColor(Entity buildingRef) => 
        SecondLineColorBinding?.Invoke(buildingRef) ?? UnityEngine.Color.clear;
    
    public static UnityEngine.Color GetThirdLineColor(Entity buildingRef) => 
        ThirdLineColorBinding?.Invoke(buildingRef) ?? UnityEngine.Color.clear;
    
    public static UnityEngine.Color GetFourthLineColor(Entity buildingRef) => 
        FourthLineColorBinding?.Invoke(buildingRef) ?? UnityEngine.Color.clear;
   
    
    public static string GetFirstLineName(Entity buildingRef) => 
        FirstLineNameBinding?.Invoke(buildingRef) ?? LinesUtils.Empty;
    
    public static string GetSecondLineName(Entity buildingRef) => 
        SecondLineNameBinding?.Invoke(buildingRef) ?? LinesUtils.Empty;
    
    public static string GetThirdLineName(Entity buildingRef) => 
        ThirdLineNameBinding?.Invoke(buildingRef) ?? LinesUtils.Empty;
    
    public static string GetFourthLineName(Entity buildingRef) => 
        FourthLineNameBinding?.Invoke(buildingRef) ?? LinesUtils.Empty;
}