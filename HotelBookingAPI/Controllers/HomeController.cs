using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HotelBookingAPI.Logic.Functions;
using HotelBookingAPI.Business.Objects;
using System.Web.Http;
using System.Web.Mvc;

namespace HotelBookingAPI.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
