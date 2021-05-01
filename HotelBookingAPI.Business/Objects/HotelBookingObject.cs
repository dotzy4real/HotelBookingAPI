using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingAPI.Business.Objects
{
    public class HotelBookingObject
    {
        string ReservationId { get; set; }
        string RoomType { get; set; }
        string CustomerId { get; set; }
        string HourRate { get; set; }
        string Status { get; set; }
        string ExpectedCheckInTime { get; set; }
        string ExpectedCheckOutTime { get; set; }
    }
}
