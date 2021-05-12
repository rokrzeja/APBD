using cwiczenia4.Models.Dto.request;
using cwiczenia4.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cwiczenia4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {

        private IProduct_WarehouseService _service;

        public WarehousesController(IProduct_WarehouseService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task <IActionResult> AddNewProduct_Warehouse(Product_Warehouse product_warehouse)
        {
            if (!await _service.IsProductExists(product_warehouse.IdProduct))
            {
                return NotFound("product id doesn't exists");
            }
            if(!await _service.IsWarehouseExists(product_warehouse.IdWarehouse))
            {
                return NotFound("id warehouse doesn't exists");
            }

            if(!await _service.IsOrderExists(product_warehouse))
            {
                return NotFound("there is no order with the id");
            }

            if (await _service.IsAlreadyDone())
            {
                return BadRequest("Order already done");
            }

            return Ok(await _service.AddProduct_Warehouse(product_warehouse));
        }



    }
}
