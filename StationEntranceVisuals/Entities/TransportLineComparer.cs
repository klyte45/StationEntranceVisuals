using System.Collections.Generic;

namespace StationEntranceVisuals.Entities;

public class TransportLineComparer : IEqualityComparer<TransportLineModel>
{
    public bool Equals(TransportLineModel x, TransportLineModel y)
    {
        return
            x != null &&
            y != null &&
            x.Type == y.Type &&
            x.Name == y.Name &&
            x.Color == y.Color ;
    }

    public int GetHashCode(TransportLineModel obj)
    {
        return 
            obj.Type.GetHashCode() + 
            obj.Name.GetHashCode() + 
            obj.Color.GetHashCode();
    }
}