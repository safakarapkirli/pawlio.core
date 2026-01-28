using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Pawlio.Models;

namespace Pawlio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbUtilsController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly PostgreSqlDbContext _context;

        public DbUtilsController(IConfiguration config, PostgreSqlDbContext context)
        {
            _configuration = config;
            _context = context;
        }

        [HttpGet("update")]
        public string Update()
        {
            var path = Path.GetFullPath("AppData");
            string jsonFilePath = path + "/postgresql.sql";
            var sql = System.IO.File.ReadAllText(jsonFilePath);

            try
            {
                _context.Database.ExecuteSqlRaw(sql);
                sql = "/* OK */" + sql;
            }
            catch (Exception e)
            {
                sql = "/* ERRROR: " + e.Message + "*/" + sql;
            }

            return sql;
        }
    }
}
