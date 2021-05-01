using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HotelBookingAPI.Logic.Functions;
using HotelBookingAPI.Business.Objects;
using System.Web.Http;

namespace HotelBookingAPI.Controllers
{
    [RoutePrefix("api/Booking")]
    public class BookingController : ApiController
    {
        // GET: api/booking/1200
        [HttpGet]
        [Route("{reservationId}")]
        public Dictionary<string, object> Get(long reservationId)
        {
            Dictionary<string, object> bookingList = Utility.GetHotelBooking(reservationId);
            return bookingList;
        }


        // GET: api/booking
        [HttpGet]
        [Route("")]
        public object List()
        {
            object bookingLists = Utility.ListHotelBooking();
            return bookingLists;
        }


        // POST: api/booking
        [HttpPost]
        [Route("")]
        public Dictionary<string, object> Save([FromBody]HotelBookingSave msg)
        {
            Dictionary<string, object> bookingList = Utility.InsertHotelBooking(msg);
            return bookingList;
        }

        // PUT: api/booking/1200
        [HttpPut]
        [Route("{reservationId}")]
        public Dictionary<string, object> Update(long reservationId, [FromBody]HotelBookingUpdate msg)
        {

            Dictionary<string, object> bookingList = Utility.UpdateHotelBooking(reservationId, msg);
            return bookingList;
        }


        // POST: api/booking/overdue/1200
        [HttpPost]
        [Route("overdue/{reservationId}")]
        public object Calculate(long reservationId, [FromBody]HotelBookingCalculate msg)
        {
            object CheckOutDetails = Utility.GetOverDueRate(reservationId, msg);
            return CheckOutDetails;
        }



    }
}
