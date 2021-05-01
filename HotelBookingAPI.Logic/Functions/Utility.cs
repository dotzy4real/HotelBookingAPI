using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HotelBookingAPI.Logic.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using HotelBookingAPI.Business.Objects;
using System.Text.RegularExpressions;

namespace HotelBookingAPI.Logic.Functions
{
    public class Utility
    {

        public static void WriteToFile(string text)
        {
            Console.WriteLine(text);
            try
            {
                string path = string.Format(@"{0}\Logs\{1}HotelBookingAPILog.txt", AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("ddd(dd)-MM-yyyy-"));
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.Write(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt      "));
                    writer.WriteLine(text);
                    writer.Close();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
            }
        }

        public static object DeserializeToDictionaryOrList(string hotelBookings, bool isArray = false)
        {
            if (!isArray)
            {
                isArray = hotelBookings.Substring(0, 1) == "[";
            }
            if (!isArray)
            {
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(hotelBookings);
                var values2 = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> d in values)
                {
                    if (d.Value is JObject)
                    {
                        values2.Add(d.Key, DeserializeToDictionary(d.Value.ToString()));
                    }
                    else if (d.Value is JArray)
                    {
                        values2.Add(d.Key, DeserializeToDictionaryOrList(d.Value.ToString(), true));
                    }
                    else
                    {
                        values2.Add(d.Key, d.Value);
                    }
                }
                return values2;
            }
            else
            {
                var values = JsonConvert.DeserializeObject<List<object>>(hotelBookings);
                var values2 = new List<object>();
                foreach (var d in values)
                {
                    if (d is JObject)
                    {
                        values2.Add(DeserializeToDictionary(d.ToString()));
                    }
                    else if (d is JArray)
                    {
                        values2.Add(DeserializeToDictionaryOrList(d.ToString(), true));
                    }
                    else
                    {
                        values2.Add(d);
                    }
                }
                return values2;
            }
        }

        public static Dictionary<string, object> DeserializeToDictionary(string hotelBookings)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(hotelBookings);
            var values2 = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> d in values)
            {
                if (d.Value is JObject)
                {
                    values2.Add(d.Key, DeserializeToDictionary(d.Value.ToString()));
                }
                else
                {
                    values2.Add(d.Key, d.Value);
                }
            }
            return values2;
        }

        public static object ListHotelBooking()
        {
            try
            {
                // Fetch all Bookings in the Hotel
                SQLClient sqlClient = new SQLClient();
                string query = "select a.ReservationID, b.RoomTypeName, a.CustomerID, b.HourlyRate, a.Status, a.ExpectedCheckIn, a.ExpectedCheckOut from CheckTime as a, RoomTypes as b where a.RoomTypeID = b.RoomTypeID";
                List<HotelBookingObject> hObjects = sqlClient.FetchHotelBooking(query);
                WriteToFile("List of Fetched Bookings: " + JsonConvert.SerializeObject(hObjects));
                return DeserializeToDictionaryOrList(JsonConvert.SerializeObject(hObjects));
            }
            catch (Exception ex)
            {
                WriteToFile(string.Format("{0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace));
                return DeserializeToDictionaryOrList(string.Format(ex.ToString()));
            }
        }

        public static Dictionary<string, object> GetHotelBooking(long ReservationID)
        {
            try
            {
                // Fetch a Booking by Reservation ID
                HotelBookingObject hObject = ExecuteHotelBookFetch(ReservationID);
                if (string.IsNullOrEmpty(hObject.Error))
                    return DeserializeToDictionary(JsonConvert.SerializeObject(hObject));
                else
                {
                    Error err = new Error();
                    err.ErrorMessage = hObject.Error;
                    return DeserializeToDictionary(JsonConvert.SerializeObject(err));
                }
            }
            catch (Exception ex)
            {
                WriteToFile(string.Format("{0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace));
                return DeserializeToDictionary(string.Format(ex.ToString()));
            }

        }

        public static HotelBookingObject ExecuteHotelBookFetch(long ReservationID)
        {
            SQLClient sqlClient = new SQLClient();
            HotelBookingObject hObject = new HotelBookingObject();
            string query = string.Format("select a.ReservationID, b.RoomTypeName, a.CustomerID, b.HourlyRate, a.Status, a.ExpectedCheckIn, a.ExpectedCheckOut from CheckTime as a, RoomTypes as b where a.RoomTypeID = b.RoomTypeID and a.ReservationID = {0}", ReservationID);
            WriteToFile(string.Format("Booking Fetch Query: {0}", query));
            List<HotelBookingObject> hObjects = sqlClient.FetchHotelBooking(query);
            if (hObjects.Count < 1)
            {
                hObject.Error = "Either Room Type Does Not Exist in this Hotel or ReservationID does not exists. Available Room Types are: deluxe, regular, palatial";
            }
            else hObject = hObjects[0];
            return hObject;
        }

        public static Dictionary<string, object> UpdateHotelBooking(long ReservationID, HotelBookingUpdate msg)
        {
            try
            {
                // Fetch Details of the Booking to update by ReservationID
                HotelBookingObject hObject = ExecuteHotelBookFetch(ReservationID);
                if (string.IsNullOrEmpty(hObject.Error))
                {
                    HotelBookingUpdate updateMsg = new HotelBookingUpdate();
                    updateMsg.Patch = msg.Patch;
                    WriteToFile(string.Format("Update Msg Details: {0}", JsonConvert.SerializeObject(updateMsg)));

                    // Assign update msg with field values from the API Call if they exist
                    updateMsg.Patch.ReservationId = hObject.ReservationId;
                    updateMsg.Patch.Status = string.IsNullOrEmpty(msg.Patch.Status) ? hObject.Status : msg.Patch.Status;
                    updateMsg.Patch.ExpectedCheckInTime = string.IsNullOrEmpty(msg.Patch.ExpectedCheckInTime) ? hObject.ExpectedCheckInTime : msg.Patch.ExpectedCheckInTime;
                    updateMsg.Patch.ExpectedCheckOutTime = string.IsNullOrEmpty(msg.Patch.ExpectedCheckOutTime) ? hObject.ExpectedCheckOutTime : msg.Patch.ExpectedCheckOutTime;
                    SQLClient sqlClient = new SQLClient();
                    string query = string.Format("update CheckTime set [Status] = @Status, [ExpectedCheckIn] = @ExpectedCheckIn, [ExpectedCheckOut] = @ExpectedCheckOut where ReservationID = {0}", hObject.ReservationId);

                    // Update Hotel Booking by the ReservationID
                    List<HotelBookingObject> hObjects = sqlClient.UpdateHotelBooking(query, updateMsg);
                    hObject = hObjects[0];
                    return DeserializeToDictionary(JsonConvert.SerializeObject(hObject));
                }
                else
                {
                    Error err = new Error();
                    err.ErrorMessage = hObject.Error;
                    return DeserializeToDictionary(JsonConvert.SerializeObject(err));
                }
            }
            catch (Exception ex)
            {
                WriteToFile(string.Format("{0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace));
                return DeserializeToDictionary(string.Format(ex.ToString()));
            }

        }

        public static Dictionary<string, object> InsertHotelBooking(HotelBookingSave msg)
        {
            try
            {
                SQLClient sqlClient = new SQLClient();
                string query = "insert into CheckTime (RoomTypeID, CustomerID, [Status], ExpectedCheckIn, ExpectedCheckOut) values (@RoomTypeID, @CustomerID, @Status, @ExpectedCheckIn, @ExpectedCheckOut)";
                // Save the Booking in the Hotel
                List<HotelBookingObject> hObjects = sqlClient.SaveHotelBooking(query, msg);
                HotelBookingObject hObject = hObjects[0];
                if (string.IsNullOrEmpty(hObject.Error))
                    return DeserializeToDictionary(JsonConvert.SerializeObject(hObject));
                else
                {
                    Error err = new Error();
                    err.ErrorMessage = hObject.Error;
                    return DeserializeToDictionary(JsonConvert.SerializeObject(err));
                }
            }
            catch (Exception ex)
            {
                WriteToFile(string.Format("{0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace));
                return DeserializeToDictionary(string.Format(ex.ToString()));
            }

        }

        public static Dictionary<string, object> GetOverDueRate(long ReservationID, HotelBookingCalculate msg)
        {
            try
            {
                HotelBookingObject hObject = ExecuteHotelBookFetch(ReservationID);
                if (string.IsNullOrEmpty(hObject.Error))
                {
                    DateTime checkOutTime = Convert.ToDateTime(msg.CheckOutTime);
                    OverDue overDue = CalculateOverDueRate(checkOutTime, ReservationID, msg, hObject);
                    return DeserializeToDictionary(JsonConvert.SerializeObject(overDue));
                }
                else
                {
                    Error err = new Error();
                    err.ErrorMessage = hObject.Error;
                    return DeserializeToDictionary(JsonConvert.SerializeObject(err));
                }
            }
            catch (Exception ex)
            {
                WriteToFile(string.Format("{0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace));
                return DeserializeToDictionary(string.Format(ex.ToString()));
            }
        }

        public static OverDue CalculateOverDueRate(DateTime CheckOutTime, long ReservationID, HotelBookingCalculate msg, HotelBookingObject hObject)
        {
            bool isWeekend = DateIsWeekend(CheckOutTime);
            string query = string.Format("Select {0} from CheckTime as a, RoomOverDueRate as b where a.ReservationID = @ReservationID and a.RoomTypeID = b.RoomTypeID", isWeekend ? "b.WeekendRate" : "b.WeekDayRate");
            SQLClient sqlclient = new SQLClient();
            double overDueRate = sqlclient.GetOverDueRate(query, ReservationID);
            DateTime expectedCheckIn = Convert.ToDateTime(hObject.ExpectedCheckInTime);
            DateTime expectedCheckOut = Convert.ToDateTime(hObject.ExpectedCheckOutTime);
            double expectedHours = expectedCheckOut.Subtract(expectedCheckIn).TotalHours;
            bool hoursStretched = ExpectedHoursStretched(expectedHours);
            int calculatedHours = Convert.ToInt32(Math.Ceiling(expectedHours));
            WriteToFile(string.Format("Calculated Hours for ReservationID {0}: {1}", ReservationID, calculatedHours));
            double initialAmount = Convert.ToDouble(hObject.HourlyRate) * calculatedHours;
            double overDueAmount = GetOverDueAmount(CheckOutTime, expectedCheckOut, hoursStretched, overDueRate, Convert.ToDouble(hObject.HourlyRate));
            double totalAmount = initialAmount + overDueAmount;
            OverDue overdue = new OverDue
            {
                InitialAmount = initialAmount,
                OverDueAmount = overDueAmount,
                TotalAmount = totalAmount,
                WeekendRate = isWeekend
            };
            return overdue;
        }

        public static double GetOverDueAmount(DateTime CheckOutTime, DateTime ExpectedCheckOut, bool hoursStretched, double overDueRate, double hourlyRate)
        {
            double overDueAmount = 0;
            double diffHours = CheckOutTime.Subtract(ExpectedCheckOut).TotalHours;
            WriteToFile("Difference in Check out time by hours: " + diffHours);
            if (diffHours > 0)
            {
                int overDueHours = Convert.ToInt32(Math.Ceiling(diffHours));
                WriteToFile("Initial Overdue Hours: " + overDueHours);
                if (hoursStretched)
                    overDueHours = overDueHours - 1;
                WriteToFile("Final Overdu Hours: " + overDueHours);
                overDueAmount = (hourlyRate * (overDueRate / 100)) * overDueHours;
                return overDueAmount;
            }
            else
                return overDueAmount;
        }

        public static bool ExpectedHoursStretched(double Hours)
        {
            return Hours % 1 == 0 ? false : true;
        }

        public static bool DateIsWeekend(DateTime Date)
        {
            WriteToFile("Day of Week: " + Date.DayOfWeek);
            if (Date.DayOfWeek != DayOfWeek.Saturday && Date.DayOfWeek != DayOfWeek.Sunday)
                return false;
            else
                return true;
        }
    }
}
