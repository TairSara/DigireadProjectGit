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

            // הוספת ניתוב ספציפי לדף ניהול המשתמשים
            routes.MapRoute(
                name: "AdminUsers",
                url: "Admin/ManageUsers",
                defaults: new { controller = "Admin", action = "ManageUsers" }
            );


            // ניתוב ברירת מחדל
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "HomePage", id = UrlParameter.Optional }
            );
        }
    }
}