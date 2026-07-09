using System;
using GeoCoordinatePortable;

namespace RX.Nyss.ReportApi.Features.Alerts;

public class ReportPoint
{
    public Guid? Label { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int ReportId { get; set; }

    public bool GetPointsAreInRange(ReportPoint secondPoint, double distanceThreshold)
    {
        var firstCoordinate = new GeoCoordinate(Latitude, Longitude);
        var secondCoordinate = new GeoCoordinate(secondPoint.Latitude, secondPoint.Longitude);
        var distance = firstCoordinate.GetDistanceTo(secondCoordinate);
        return distance <= distanceThreshold;
    }
}
