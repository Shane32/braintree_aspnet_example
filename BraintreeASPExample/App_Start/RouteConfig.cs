﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BraintreeASPExample
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Checkouts-New",
                url: "checkouts/new",
                defaults: new { controller = "Checkouts", action = "New" },
                constraints: new { httpMethod = new HttpMethodConstraint(new string[]{"GET"}) }
            );

             routes.MapRoute(
                name: "Checkouts",
                url: "checkouts",
                defaults: new { controller = "Checkouts", action = "New" },
                constraints: new { httpMethod = new HttpMethodConstraint(new string[]{"GET"}) }
            );

            routes.MapRoute(
                name: "Checkouts-Create",
                url: "checkouts",
                defaults: new { controller = "Checkouts", action = "Create" },
                constraints: new { httpMethod = new HttpMethodConstraint(new string[] { "POST" }) }
            );

            routes.MapRoute(
                name: "Checkouts-Show",
                url: "checkouts/Show",
                defaults: new { controller = "Checkouts", action = "Show" }
            );

            routes.MapRoute(
                name: "Checkouts-VerifyCVV",
                url: "checkouts/VerifyCVV",
                defaults: new { controller = "Checkouts", action = "VerifyCVV" }
            );

            routes.MapRoute(
                name: "Checkouts-RetryPaypal",
                url: "checkouts/RetryPaypal",
                defaults: new { controller = "Checkouts", action = "RetryPaypal" }
            );

            routes.MapRoute(
                name: "Checkouts-RetryGooglePay",
                url: "checkouts/RetryGooglePay",
                defaults: new { controller = "Checkouts", action = "RetryGooglePay" }
            );

            routes.MapRoute(
                name: "Home",
                url: "",
                defaults: new { controller = "Checkouts", action = "New" }
            );
        }
    }
}