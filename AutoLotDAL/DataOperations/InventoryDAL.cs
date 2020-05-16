using AutoLotDAL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AutoLotDAL.DataOperations
{
    public class InventoryDAL
    {
        private SqlConnection _sqlConnection = null;
        private void OpenConnection()
        {
            _sqlConnection = new SqlConnection { ConnectionString = _connectionString };
            _sqlConnection.Open();
        }

        private void CloseConnection()
        {
            if(_sqlConnection?.State != ConnectionState.Closed)
            {
                _sqlConnection?.Close();
            }
        }

        private readonly string _connectionString;

        public InventoryDAL() : this(@"Data Source = .\SQLEXPRESS; Integrated Security=true; Initial Catalog=AutoLot")
        { 
        }

        public InventoryDAL(string connectionString) => _connectionString = connectionString;

        public List<Car> GetAllInventory() // return List<Car>,  all data in table Inventory
        {
            OpenConnection();
            List<Car> inventory = new List<Car>();

            //Prepeare object
            string sql = "SELECT * FROM Inventory";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dataReader.Read())
                {
                    inventory.Add(new Car
                    {
                        CarId = (int)dataReader["CarId"],
                        Color = (string)dataReader["Color"],
                        Make = (string)dataReader["Make"],
                        PetName = (string)dataReader["Petname"]
                    });
                }
                dataReader.Close();
            }
            return inventory;
        }


    }
}
