using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient; // ✅ Use this (no need to install extra package)

namespace InfacApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RackController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RackController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ✅ 1. Get Active Racks
        [HttpGet("active")]
        public IActionResult GetActiveRacks()
        {
            try
            {
                var data = new List<RackDto>();

                using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
                using var cmd = new SqlCommand("pr_fetch_Active_Racks", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                using var dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    data.Add(new RackDto
                    {
                        RackId = dr["RackId"] != DBNull.Value ? Convert.ToInt32(dr["RackId"]) : 0,
                        RackName = dr["RackName"]?.ToString() ?? ""
                    });
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ✅ 2. Get Barcode Details
        [HttpGet("barcode")]
        public IActionResult GetBarcodeDetails(string barcode)
        {
            try
            {
                BarcodeResponse result = new BarcodeResponse();

                using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
                using var cmd = new SqlCommand("API_pr_fetch_barcode_details", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@vBarcode", SqlDbType.VarChar, 50).Value = barcode ?? "";

                con.Open();
                using var dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    result = new BarcodeResponse
                    {
                        Status = dr["Status"] != DBNull.Value ? Convert.ToInt32(dr["Status"]) : 0,
                        Message = dr["Message"]?.ToString() ?? "",

                        Barcode = dr["Barcode"]?.ToString(),
                        PartNo = dr["PartNo"]?.ToString(),
                        PartName = dr["PartName"]?.ToString(),
                        Model = dr["Model"]?.ToString(),

                        Qty = dr["Qty"] != DBNull.Value ? Convert.ToDecimal(dr["Qty"]) : null,
                        InwardDate = dr["InwardDate"] != DBNull.Value ? Convert.ToDateTime(dr["InwardDate"]) : null,

                        Rack = dr["Rack"]?.ToString(),
                        RackId = dr["RackId"] != DBNull.Value ? Convert.ToInt32(dr["RackId"]) : null
                    };
                }

                return result.Status == 1 ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ✅ 3. Update Rack + Insert Audit
        [HttpPost("updaterack")]
        public IActionResult UpdateRack([FromBody] UpdateRackRequest request)
        {
            try
            {
                ApiResponse result = new ApiResponse();

                using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
                using var cmd = new SqlCommand("API_pr_insert_barcode_rack_audit", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@vBarcode", SqlDbType.VarChar, 50).Value = request.Barcode ?? "";
                cmd.Parameters.Add("@iRackId", SqlDbType.Int).Value = request.RackId;
                cmd.Parameters.Add("@iUpdatedBy", SqlDbType.Int).Value = request.UpdatedBy;

                con.Open();
                using var dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    result = new ApiResponse
                    {
                        Status = dr["Status"] != DBNull.Value ? Convert.ToInt32(dr["Status"]) : 0,
                        Message = dr["Message"]?.ToString() ?? ""
                    };
                }

                return result.Status == 1 ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // ✅ DTO 1
        public class RackDto
        {
            public int RackId { get; set; }
            public string RackName { get; set; } = "";
        }

        // ✅ DTO 2
        public class BarcodeResponse
        {
            public int Status { get; set; }
            public string Message { get; set; } = "";

            public string? Barcode { get; set; }
            public string? PartNo { get; set; }
            public string? PartName { get; set; }
            public string? Model { get; set; }

            public decimal? Qty { get; set; }
            public DateTime? InwardDate { get; set; }

            public string? Rack { get; set; }
            public int? RackId { get; set; }
        }

        // ✅ DTO 3
        public class UpdateRackRequest
        {
            public string Barcode { get; set; } = "";
            public int RackId { get; set; }
            public int UpdatedBy { get; set; }
        }

        // ✅ DTO 4
        public class ApiResponse
        {
            public int Status { get; set; }
            public string Message { get; set; } = "";
        }
    }
}