using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DigireadProject
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "HomePage", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Checkout",
                url: "Order/Checkout",
                defaults: new { controller = "Order", action = "Checkout" }
            );
            routes.MapRoute(
                name: "RemoveFromWaitList",
                url: "BookManagement/RemoveFromWaitList",
                defaults: new { controller = "BookManagement", action = "RemoveFromWaitList" }
            );
            routes.MapRoute(
                name: "Reviews",
                url: "Reviews/{action}/{id}",
                defaults: new { controller = "Reviews", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Payment",
                url: "Order/PaymentForm",
                defaults: new { controller = "Order", action = "PaymentForm" }
            );
            routes.MapRoute(
                name: "ManageRentals",
                url: "BookManagement/ManageRentals",
                defaults: new { controller = "BookManagement", action = "ManageRentals" }
            );

            routes.MapRoute(
                name: "ManageWaitList", 
                url: "BookManagement/ManageWaitList",
                defaults: new { controller = "BookManagement", action = "ManageWaitList" }
            );
        }
    }
}
