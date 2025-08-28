using System;

namespace TakiWebApi.Models;

public class SharedRide
{
    public int SharedRideID { get; set; }
    public int TripID { get; set; }
    public int PassengerCount { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedDate { get; set; }
}
