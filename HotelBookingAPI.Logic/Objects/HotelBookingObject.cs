using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingAPI.Business.Objects
{
    public class HotelBookingObject
    {
        public string ReservationId { get; set; }
        public string RoomType { get; set; }
        public string CustomerId { get; set; }
        public string HourlyRate { get; set; }
        public string Status { get; set; }
        public string ExpectedCheckInTime { get; set; }
        public string ExpectedCheckOutTime { get; set; }
        public string Error { get; set; }
    }

    public class HotelBookingSave
    {
        public Guid TransactionID { get; set; }
        public HotelBookingObject Entity { get; set; }
    }

    public class HotelBookingUpdate
    {
        public Guid TransactionID { get; set; }
        public HotelBookingObject Patch { get; set; }
    }

    public class HotelBookingCalculate
    {
        public string ReservationId { get; set; }
        public string RoomType { get; set; }
        public string CustomerId { get; set; }
        public string CheckOutTime { get; set; }
    }

    public class OverDue
    {
        public double InitialAmount { get; set; }
        public double OverDueAmount { get; set; }
        public double TotalAmount { get; set; }
        public bool WeekendRate { get; set; }
    }

    public class Error
    {
        public string ErrorMessage { get; set; }
    }
}
