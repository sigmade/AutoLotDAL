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
            using (SqlCommand command = new SqlCommand(sql, _sqlConnection))
            {
                SqlParameter parameter = new SqlParameter
                {
                    ParameterName = "@Make",
                    Value = car.Make,
                    SqlDbType = SqlDbType.Char,
                    Size = 10
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@Color",
                    Value = car.Color,
                    SqlDbType = SqlDbType.Char,
                    Size = 10
                };
                command.Parameters.Add(parameter);
                parameter = new SqlParameter
                {
                    ParameterName = "@PetName",
                    Value = car.PetName,
                    SqlDbType = SqlDbType.Char,
                    Size = 10
                };
                command.Parameters.Add(parameter);
                command.ExecuteNonQuery();
                CloseConnection();
            }

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
        public string LookUpPetName(int carId)
        {
            OpenConnection();
            string carPetName;

            using (SqlCommand command = new SqlCommand("GetPetName", _sqlConnection))
            {
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter param = new SqlParameter
                {
                    ParameterName = "@carId",
                    SqlDbType = SqlDbType.Int,
                    Value = carId,
                    Direction = ParameterDirection.Input
                };
                command.Parameters.Add(param);

                param = new SqlParameter
                {
                    ParameterName = "@petName",
                    SqlDbType = SqlDbType.Char,
                    Size = 10,
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(param);

                command.ExecuteNonQuery();

                // Return output param.
                carPetName = (string)command.Parameters["@petName"].Value;
                CloseConnection();
            }
            return carPetName;
        }
        public void ProcessCreditRisk(bool throwEx, int custId)
        {
            OpenConnection();
            string fname;
            string lname;
            var cmdSelect = new SqlCommand($"SELECT * FROM Customers WHERE CustId = {custId}", _sqlConnection);
            using (var dataReader = cmdSelect.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    fname = (string)dataReader["FirstName"];
                    lname = (string)dataReader["LastName"];
                }
                else
                {
                    CloseConnection();
                    return;
                }
            }
            var cmdRemove = new SqlCommand($"DELETE FROM Customers WHERE CustId = {custId}", _sqlConnection);
            var cmdInsert = new SqlCommand($"INSERT INTO CreditRisks (FirstName, LastName) VALUES ('{fname}', '{lname}')", _sqlConnection);
            SqlTransaction tx = null;
            try
            {
                tx = _sqlConnection.BeginTransaction();
                cmdInsert.Transaction = tx;
                cmdRemove.Transaction = tx;
                cmdInsert.ExecuteNonQuery();
                cmdRemove.ExecuteNonQuery();

                if (throwEx)
                {
                    throw new Exception("Sorry! Database error! Tx failed....");
                }
                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tx?.Rollback();
            }
            finally
            {
                CloseConnection();
            }
        }

    }
}
