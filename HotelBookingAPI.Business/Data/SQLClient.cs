using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace HotelBookingAPI.Logic.Data
{
    public class SQLClient
    {
        private static string _connString;
        public SQLClient()
        {
            _connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
        }
    }
}
