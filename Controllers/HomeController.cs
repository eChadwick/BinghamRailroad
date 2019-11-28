﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BinghamRailroad.Models;
using BinghamRailroad.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BinghamRailroad.Controllers
{
    public class HomeController : Controller
    {
        private readonly BingRailContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, BingRailContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var allStations = (from Station in _context.Set<Station>()
            select Station).ToList();
            ViewData["Stations"] = allStations;
            return View();
        }
        [HttpGet]
        public IActionResult Routes()
        {
            var OriginStationId = 141;
            var DestinationStationId = 88;
            
            // Get matching routes
            var routes = 
            (from route in _context.Set<Route>()
            where route.OriginStation == OriginStationId
            && route.DestinationStation == DestinationStationId
            select new RouteInfoViewModel {
                RouteId = route.Id,
                DepartureTime = route.DepartureTime,
                ArrivalTime = route.ArrivalTime,
                TrainId = route.TrainId,
            }).ToList();

            // Get train amenities for each route
            foreach(var r in routes)
            {
                r.Amenities =
                (from trainAmenity in _context.Set<TrainAmenity>()
                join train in _context.Set<Train>() on trainAmenity.TrainId equals train.Id
                join amenity in _context.Set<Amenity>() on trainAmenity.AmenityId equals amenity.Id
                where train.Id == r.TrainId
                select amenity.Name).ToList();
            }

            // Get remaining seats for each route
            foreach(var r in routes)
            {
                r.OpenSeats =
                (from train in _context.Set<Train>()
                join route in _context.Set<Route>() on train.Id equals route.TrainId
                where route.Id == r.RouteId
                select train.NumSeats).First()
                -
                (from train in _context.Set<Train>()
                join route in _context.Set<Route>() on train.Id equals route.TrainId
                join ticket in _context.Set<Ticket>() on route.Id equals ticket.RouteId
                where route.Id == r.RouteId
                select ticket).Count();
            }
            Console.Write("Number of Routes: " + routes.Count());
            return View("Privacy", routes);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult SignIn()
        {
            return View();
        }
        public IActionResult CreateAccount()
        {
            return View();
        }
        public IActionResult AccountInfo()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
