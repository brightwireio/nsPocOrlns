namespace nsPocOrlns.Infrastructure.Entities;

public class Trip
{
    public long UnitId { get; set; }
    public Guid TripId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int NumHarshBreaks { get; set; }
    public int NumViolations { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
}
