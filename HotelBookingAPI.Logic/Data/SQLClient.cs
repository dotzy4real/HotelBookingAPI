using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using HotelBookingAPI.Business.Objects;
using HotelBookingAPI.Logic.Functions;

namespace HotelBookingAPI.Logic.Data
{
    public class SQLClient
    {
        private static string _connString;
        public SQLClient()
        {
            _connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        }


        public List<HotelBookingObject> FetchHotelBooking(string query)
        {
            List<HotelBookingObject> hObjects = new List<HotelBookingObject>();
            using (SqlConnection sqlCon = new SqlConnection(_connString))
            {

                SqlCommand sqlCmd = new SqlCommand(query, sqlCon);

                // Be sure the sqlconnection is open
                if (sqlCon.State == ConnectionState.Closed)
                {
                    sqlCon.Open();
                }


                using (var reader = sqlCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            HotelBookingObject hObject = new HotelBookingObject();
                            hObject.ReservationId = reader[0].ToString();
                            hObject.RoomType = reader[1].ToString();
                            hObject.CustomerId = reader[2].ToString();
                            hObject.HourlyRate = reader[3].ToString();
                            hObject.Status = reader[4].ToString();
                            hObject.ExpectedCheckInTime = reader[5].ToString();
                            hObject.ExpectedCheckOutTime = reader[6].ToString();
                            hObjects.Add(hObject);
                        }
                    }
                }
            }
            return hObjects;
        }

        public string ExecuteStringColumnReader(string query)
        {
            string columnValue = "";
            using (SqlConnection sqlConId = new SqlConnection(_connString))
            {
                SqlCommand sqlCmd = new SqlCommand(query, sqlConId);

                // Be sure the sqlconnection is open
                if (sqlConId.State == ConnectionState.Closed)
                {
                    sqlConId.Open();
                }

                using (var reader = sqlCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            columnValue = reader[0].ToString();
                        }
                    }
                }

            }
            return columnValue;
        }

        public List<HotelBookingObject> SaveHotelBooking(string query, HotelBookingSave msg)
        {
            List<HotelBookingObject> hObjects = new List<HotelBookingObject>();
            Utility.WriteToFile("Got to SQL Client Write To File Path");
            Utility.WriteToFile("Request in Payload: " + JsonConvert.SerializeObject(msg.Entity));
            string roomTypeQuery = string.Format("select RoomTypeID from RoomTypes where RoomTypeName = '{0}'", msg.Entity.RoomType);
            string roomTypeID = ExecuteStringColumnReader(roomTypeQuery);
            if (string.IsNullOrEmpty(roomTypeID))
            {
                HotelBookingObject hObject = new HotelBookingObject();
                hObject.Error = "Either Room Type Does Not Exist in this Hotel or ReservationID does not exists. Available Room Types are: deluxe, regular, palatial";
                hObjects.Add(hObject);
                return hObjects;
            }
            else
            {
                Utility.WriteToFile("Able to check Room Type");
                using (SqlConnection sqlCon = new SqlConnection(_connString))
                {

                    using (var command = new SqlCommand(query, sqlCon))
                    {
                        command.Parameters.AddWithValue("RoomTypeID", roomTypeID);
                        command.Parameters.AddWithValue("CustomerID", Convert.ToInt64(msg.Entity.CustomerId));
                        command.Parameters.AddWithValue("Status", msg.Entity.Status);
                        command.Parameters.AddWithValue("ExpectedCheckIn", msg.Entity.ExpectedCheckInTime);
                        command.Parameters.AddWithValue("ExpectedCheckOut", msg.Entity.ExpectedCheckOutTime);
                        if (sqlCon.State == ConnectionState.Closed)
                        {
                            sqlCon.Open();
                        }
                        Utility.WriteToFile("Execute My Query Here");
                        command.ExecuteNonQuery();
                    }
                }
                Utility.WriteToFile("Could Insert Hotel Booking Successfully");
                string lastQuery = string.Format("select a.ReservationID, b.RoomTypeName, a.CustomerID, b.HourlyRate, a.Status, a.ExpectedCheckIn, a.ExpectedCheckOut from CheckTime as a, RoomTypes as b where a.RoomTypeID = b.RoomTypeID and a.CustomerID = {0} order by a.ReservationID desc", msg.Entity.CustomerId);
                Utility.WriteToFile("Able to launch Last Query");
                hObjects = FetchHotelBooking(lastQuery);
                Utility.WriteToFile("Able to Execute Last Query Successfully");
                return hObjects;
            }
        }

        public List<HotelBookingObject> UpdateHotelBooking(string query, HotelBookingUpdate msg)
        {
            using (SqlConnection sqlCon = new SqlConnection(_connString))
            {
                using (var command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("Status", msg.Patch.Status);
                    command.Parameters.AddWithValue("ExpectedCheckIn", msg.Patch.ExpectedCheckInTime);
                    command.Parameters.AddWithValue("ExpectedCheckOut", msg.Patch.ExpectedCheckOutTime);
                    if (sqlCon.State == ConnectionState.Closed)
                    {
                        sqlCon.Open();
                    }
                    command.ExecuteNonQuery();
                }
            }
            string lastQuery = string.Format("select a.ReservationID, b.RoomTypeName, a.CustomerID, b.HourlyRate, a.Status, a.ExpectedCheckIn, a.ExpectedCheckOut from CheckTime as a, RoomTypes as b where a.RoomTypeID = b.RoomTypeID and a.ReservationID = {0}", msg.Patch.ReservationId);
            List<HotelBookingObject> hObjects = FetchHotelBooking(lastQuery);
            return hObjects;
        }

        public double GetOverDueRate(string query, long ReservationID)
        {
            double rate = 0;
            using (SqlConnection sqlCon = new SqlConnection(_connString))
            {
                using (var command = new SqlCommand(query, sqlCon))
                {
                    command.Parameters.AddWithValue("ReservationID", ReservationID);

                    // Be sure the sqlconnection is open
                    if (sqlCon.State == ConnectionState.Closed)
                    {
                        sqlCon.Open();
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                rate = Convert.ToDouble(reader[0].ToString());
                            }
                        }
                    }
                }
            }
            return rate;
        }
    }
}
