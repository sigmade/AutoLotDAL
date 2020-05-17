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

        public Car GetCar(int id) // return one row for CarId
        {
            OpenConnection();
            Car car = null;
            string sql = $"SELECT * FROM Inventory WHERE CarId = {id}";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dataReader.Read())
                {
                    car = new Car
                    {
                        CarId = (int)dataReader["CarId"],
                        Color = (string)dataReader["Color"],
                        Make = (string)dataReader["Make"],
                        PetName = (string)dataReader["Petname"]
                    };                    
                }
                dataReader.Close();
            }
            return car;
        }
        public void InsertAuto(string color, string make, string petName)
        {
            OpenConnection();
            // Format and execute SQL statement.
            string sql = $"Insert Into Inventory (Make, Color, PetName) Values ('{make}', '{color}', '{petName}')";

            // Execute using our connection.
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }
        //public void InsertAuto(Car car) // type method
        //{
        //    OpenConnection();
        //    string sql = "INSERT INTO Inventory (Make, Color, Petname) VALUES ('{car.Make}', '{car.Color}', '{car.PetNmae}')";

        //    using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
        //    {
        //        command.CommandType = CommandType.Text;
        //        command.ExecuteNonQuery();
        //    }
        //    CloseConnection();
        //}

        public void InsertAuto(Car car)
        {
            OpenConnection();
            string sql = "INSERT INTO Inventory (Make, Color, PetName) VALUES (@Make, @Color, @PetName)";

        }
        public void DeleteCar(int id)
        {
            OpenConnection();
            string sql = $"DELETE FROM Inventory WHERE CarId = '{id}'";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                try
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Exception error = new Exception("Sorry! That car is on order!", ex);

                    throw error;
                }
            }
            CloseConnection();
        }
        public void UpdateCarPetName(int id, string newPetName)
        {
            OpenConnection();
            string sql = $"UPDATE Inventory SET PetName = '{newPetName}' WHERE CarId = '{id}'";
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                command.ExecuteNonQuery();
            }
            CloseConnection();
        }



    }
}
