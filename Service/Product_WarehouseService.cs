using cwiczenia4.Models.Dto.request;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cwiczenia4.Service
{

    public interface IProduct_WarehouseService
    {
        Task <int> AddProduct_Warehouse(Product_Warehouse product_warehouse);
        Task<bool> IsProductExists(int id);
        Task<bool> IsWarehouseExists(int id);
        Task<bool> IsOrderExists(Product_Warehouse product_warehouse);
        Task<bool> IsAlreadyDone();


        Task<int> AddProduct_WarehouseByProcedure(Product_Warehouse product_warehouse);
    }
    public class Product_WarehouseService : IProduct_WarehouseService
    {

        private IConfiguration _configuration;
        private int _orderId;

        public Product_WarehouseService(IConfiguration config)
        {
            _configuration = config;
            _orderId = -1;
        }


        public async Task <bool> IsProductExists(int id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("SELECT * FROM Product WHERE IdProduct=@id", connection);
            command.Parameters.AddWithValue("@id", id);

            bool isProductExists = false;
            var resultList = new List<int>();

            await connection.OpenAsync();
            try
            {
                var sqlDataReader = await command.ExecuteReaderAsync();
                while (await sqlDataReader.ReadAsync())
                {
                    resultList.Add((int)sqlDataReader["IdProduct"]);
                }
            }
            catch (SqlException exc)
            {
                isProductExists = false;
            }
            catch (Exception exc)
            {
                isProductExists = false;
            }

            if (resultList.Count > 0) isProductExists = true;

            return isProductExists;
        }

        public async Task <bool> IsWarehouseExists(int id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("SELECT * FROM Warehouse WHERE IdWarehouse=@id ", connection);
            command.Parameters.AddWithValue("@id", id);

            bool isWarehouseExists = false;
            var resultList = new List<int>();

            await connection.OpenAsync();

            try
            {
                var sqlDataReader = await command.ExecuteReaderAsync();
                while (await sqlDataReader.ReadAsync())
                {
                    resultList.Add((int)sqlDataReader["IdWarehouse"]);
                }
            }
            catch (SqlException exc)
            {
                isWarehouseExists = false;
            }
            catch (Exception exc)
            {
                isWarehouseExists = false;
            }

            if (resultList.Count > 0) isWarehouseExists = true;

            return isWarehouseExists;
        }

        public async Task<bool> IsOrderExists(Product_Warehouse product_warehouse)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("SELECT * FROM " + (char)34 + "Order" + (char)34 + " WHERE IdProduct=@id AND Amount=@amount", connection);
            command.Parameters.AddWithValue("@id", product_warehouse.IdProduct);
            command.Parameters.AddWithValue("@amount", product_warehouse.Amount);

            var resultList = new List<int>();

            await connection.OpenAsync();

            try
            {
                var sqlDataReader = await command.ExecuteReaderAsync();
                while (await sqlDataReader.ReadAsync())
                {
                    if (DateTime.Compare((DateTime)sqlDataReader["CreatedAt"], product_warehouse.CreatedAt) < 0)
                    {
                        resultList.Add((int)sqlDataReader["IdOrder"]);
                    }
                }
            }
            catch (SqlException exc)
            {
                return false;
            }
            catch (Exception exc)
            {
                return false;
            }

            if (resultList.Count > 0)
            {
                _orderId = resultList[0];
                return true;
            }

            return false;
        }

        public async Task<bool> IsAlreadyDone()
        {
            //if (id < 0) return true;
            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("SELECT * FROM Product_Warehouse WHERE IdOrder=@id", connection);
            command.Parameters.AddWithValue("@id", _orderId);

            var resultList = new List<int>();
            bool isAlreadyDone = true;

            await connection.OpenAsync();

            try
            {
                var sqlDataReader = await command.ExecuteReaderAsync();
                while (await sqlDataReader.ReadAsync())
                {
                    resultList.Add((int)sqlDataReader["IdProductWarehouse"]);
                }
            }
            catch (SqlException exc)
            {
                isAlreadyDone = true;
            }
            catch (Exception exc)
            {
                isAlreadyDone = true;
            }

            if (resultList.Count < 1) isAlreadyDone = false;

            return isAlreadyDone;
        }

        public async Task<int> AddProduct_Warehouse(Product_Warehouse product_warehouse)
        {
            await UpdateFullfilledAt();
            await Insert(product_warehouse);
            return await FindLastIndex();
        }
        public async Task  UpdateFullfilledAt()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("UPDATE " + (char)34 + "Order" + (char)34 + " SET FulfilledAt = @at WHERE IdOrder=@id", connection);
            command.Parameters.AddWithValue("@at", DateTime.Now);
            command.Parameters.AddWithValue("@id", _orderId);

            await connection.OpenAsync();

            try
            {
               await command.ExecuteNonQueryAsync();

            }
            catch (SqlException exc)
            {
                Console.WriteLine(exc);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }
        public async Task Insert(Product_Warehouse product_warehouse)
        {
            double productPrice = await FindProductPrice(product_warehouse.IdProduct);

            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) VALUES (@idw, @idp, @ido, @amount, @price, @ca)", connection);
            command.Parameters.AddWithValue("@idw", product_warehouse.IdWarehouse);
            command.Parameters.AddWithValue("@idp", product_warehouse.IdProduct);
            command.Parameters.AddWithValue("@ido", _orderId);
            command.Parameters.AddWithValue("@amount", product_warehouse.Amount);
            command.Parameters.AddWithValue("@price", productPrice * product_warehouse.Amount);
            command.Parameters.AddWithValue("@ca", DateTime.Now);

            await connection.OpenAsync();

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException exc)
            {
                Console.WriteLine(exc);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }
        public async Task<double> FindProductPrice(int id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("SELECT Price FROM Product WHERE IdProduct=@id", connection);
            command.Parameters.AddWithValue("@id", id);

            double result = 0;

            await connection.OpenAsync();

            try
            {
                var sqlDataReader = await command.ExecuteReaderAsync();
                while (await sqlDataReader.ReadAsync())
                {
                    result = (double)sqlDataReader.GetDecimal(0);
                }
            }
            catch (SqlException exc)
            {
                Console.WriteLine(exc);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            return result;
        }
        public async Task<int> FindLastIndex()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("SELECT MAX(IdProductWarehouse) FROM Product_Warehouse", connection);

            int result = 0;

            await connection.OpenAsync();

            try
            {
                var sqlDataReader = await command.ExecuteReaderAsync();
                while (await sqlDataReader.ReadAsync())
                {
                    result = sqlDataReader.GetInt32(0);
                }
            }
            catch (SqlException exc)
            {
                Console.WriteLine(exc);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }

            return result;
        }



        public async Task<int> AddProduct_WarehouseByProcedure(Product_Warehouse product_warehouse)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("ProductionDb"));
            using var command = new SqlCommand("AddProductToWarehouse", connection);
            command.CommandType = System.Data.CommandType.StoredProcedure;

            SqlParameter idProduct = new SqlParameter();
            idProduct.ParameterName = "@IdProduct";
            idProduct.SqlDbType = System.Data.SqlDbType.Int;
            idProduct.Value = product_warehouse.IdProduct;

            SqlParameter idWarehouse = new SqlParameter();
            idWarehouse.ParameterName = "@IdWarehouse";
            idWarehouse.SqlDbType = System.Data.SqlDbType.Int;
            idWarehouse.Value = product_warehouse.IdWarehouse;

            SqlParameter amount = new SqlParameter();
            amount.ParameterName = "@Amount";
            amount.SqlDbType = System.Data.SqlDbType.Int;
            amount.Value = product_warehouse.Amount;

            SqlParameter createdAt = new SqlParameter();
            createdAt.ParameterName = "@CreatedAt";
            createdAt.SqlDbType = System.Data.SqlDbType.DateTime;
            createdAt.Value = product_warehouse.CreatedAt;

            command.Parameters.Add(idProduct);
            command.Parameters.Add(idWarehouse);
            command.Parameters.Add(amount);
            command.Parameters.Add(createdAt);

            await connection.OpenAsync();

            int index = 0;
            using(var sqlDataReader = await command.ExecuteReaderAsync())
            {
                while(await sqlDataReader.ReadAsync())
                {
                    index = (int)sqlDataReader.GetDecimal(0); 
                }
            }


            return index;
        }
    }
}
