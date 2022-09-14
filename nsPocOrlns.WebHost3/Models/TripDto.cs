namespace nsPocOrlns.WebHost3.Models;

public class TripDto
{
    public long UnitId { get; set; }
    public Guid TripId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int NumHarschBreaks { get; set; }
    public int NumViolations { get; set; }
}
