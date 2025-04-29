using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EmployeeService.Models;
using Newtonsoft.Json;

namespace EmployeeService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IEmployeeService
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public string GetEmployeeById(int id)
        {

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = new SqlCommand("SELECT * FROM Employees WHERE ID = @ID", connection);
                cmd.Parameters.AddWithValue("@ID", id);
                var reader = cmd.ExecuteReader();

                Employee employee = null;

                if (reader.Read())
                {
                    employee = new Employee
                    {
                        Id = (int)reader["ID"],
                        Name = reader["Name"].ToString(),
                        ManagerId = reader["ManagerID"] as int?,
                        Enable = (bool)reader["Enable"]
                    };
                }
                reader.Close();

                if (employee != null)
                {
                    employee.Subordinates = GetSubordinates(employee.Id, connection);
                }

                return JsonConvert.SerializeObject(employee, Formatting.Indented);
            }
        }

      

        public void EnableEmployee(int id, int enable)
        {
            if (enable != 0 && enable != 1)
                throw new ArgumentException("Adrgument \'enable\' must be 0 or 1");
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("UPDATE Employees SET Enable = @Enable WHERE ID = @ID", connection);
                cmd.Parameters.AddWithValue("@Enable", enable);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }


        private List<Employee> GetSubordinates(int managerId, SqlConnection connection)
        {
            var subordinates = new List<Employee>();

            var cmd = new SqlCommand("SELECT * FROM Employees WHERE ManagerID = @ManagerID", connection);
            cmd.Parameters.AddWithValue("@ManagerID", managerId);
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var subordinate = new Employee
                {
                    Id = (int)reader["ID"],
                    Name = reader["Name"].ToString(),
                    ManagerId = reader["ManagerID"] as int?,
                    Enable = (bool)reader["Enable"]
                };

                subordinates.Add(subordinate);
            }
            reader.Close();

            return subordinates;
        }

    }

      
}