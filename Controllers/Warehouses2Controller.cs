using cwiczenia4.Models.Dto.request;
using cwiczenia4.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cwiczenia4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Warehouses2Controller : ControllerBase
    {
        private IProduct_WarehouseService _service;

        public Warehouses2Controller(IProduct_WarehouseService service)
        {
            _service = service;
        }
        
        [HttpPost]
        public async Task <IActionResult> AddNewProduct_WarehouseByProcedure(Product_Warehouse product_warehouse)
        {
            try
            {
                return Ok(await _service.AddProduct_WarehouseByProcedure(product_warehouse));
            }
            catch(SqlException exc)
            {
                if (exc.Message.Equals("Invalid parameter: There is no order to fullfill"))
                {
                    return BadRequest("Invalid parameter: There is no order to fullfill");
                }
                else if (exc.Message.Equals("Invalid parameter: Provided IdProduct does not exist"))
                {
                    return NotFound("Invalid parameter: Provided IdProduct does not exist");
                }
                else if (exc.Message.Equals("Invalid parameter: Provided IdWarehouse does not exist"))
                {
                    return NotFound("Invalid parameter: Provided IdWarehouse does not exist");
                }
                else
                {
                    return NotFound("sth invalid");
                }
            }
            
        }
    }
}
