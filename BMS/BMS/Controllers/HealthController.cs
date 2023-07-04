﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public string Check()
        {
            Console.WriteLine($"BmsV1Service-Ok-【{DateTime.Now:yyyy/MM/dd HH:mm:ss}】");
            return $"BmsV1Service-Ok-【{DateTime.Now:yyyy/MM/dd HH:mm:ss}】";
        }
    }
}

