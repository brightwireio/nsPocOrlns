namespace nsPocOrlns.Grains.Interfaces;

public class TripState
{
    public double StartLatitide { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitide { get; set; }
    public double EndLongitude { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public int NumViolations { get; set; }
    public int NumHarshBreaks { get; set; }

}