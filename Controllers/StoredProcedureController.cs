using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StoredProceduresWebApi.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace StoredProceduresWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoredProcedureController : ControllerBase
    {
        private const int DEFAULT_COMMAND_TIMEOUT = 180000; // 3 minutes
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public StoredProcedureController(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody] StoredProcedurePostInput input)
        {
            if (input == null)
            {
                throw new ArgumentException($"{nameof(input)} is null");
            }

            if (string.IsNullOrWhiteSpace(input.SPName))
            {
                throw new ArgumentException($"{nameof(input.SPName)} is null or white space");
            }

            string dbConnectionString = _configuration.GetConnectionString("db");
            int timeout = input.Timeout ?? DEFAULT_COMMAND_TIMEOUT;
            SqlConnection conn = new SqlConnection(dbConnectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeout;
                cmd.CommandText = input.SPName;
                if (input.Parameters != null)
                {
                    foreach (KeyValuePair<string, string> kvp in input.Parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                using (SqlDataAdapter da = new SqlDataAdapter
                {
                    SelectCommand = cmd
                })
                {
                    DataSet ds = new DataSet("ResultSet");
                    da.Fill(ds);
                    return Ok(JsonConvert.SerializeObject(ds, Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, JsonConvert.SerializeObject(new ErrorDetails { StatusCode = 500, Message = ex.Message }, Formatting.Indented));
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                conn.Dispose();
            }
        }
    }
}
