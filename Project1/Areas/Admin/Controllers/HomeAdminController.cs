﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project1.Models;
using Project1.Models.Authentication;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Azure;
using System.Diagnostics.Eventing.Reader;
using Project1.Areas.Admin.ViewModels;
using NuGet.Protocol;
using Microsoft.AspNetCore.Authorization;
using System.Drawing.Printing;
using System.Collections.Generic;
using X.PagedList.Extensions;

namespace Project1.Areas.Admin.Controllers
{
	
	[Area("admin")]
	[Route("admin")]
	[Route("admin/homeadmin")]
	public class HomeAdminController : Controller
	{
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeAdminController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        PizzaOnlineContext db= new PizzaOnlineContext();
        //từ đây trở xuống là các action phục vụ hiển thị dashboard
        [Route("")]
		[Route("index")]
		public IActionResult Index()
		{
           
            return View("Dashboard/Index");
		}
        //kết thúc dashboard

        //từ đây trở xuống là các action xử lý combo
        //kết thúc các action xử lý combo

        //từ đây trở xuống là các action xử lý user
        //kết thúc các action xử lý user
    }
}
