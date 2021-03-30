using Microsoft.SqlServer.Server;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace ExtractDllFromSql
{
    class Program
    {
        [SqlProcedure]
        static void Main()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["database"].ConnectionString;
            string dllName = ConfigurationManager.AppSettings["DllName"]; 
            string outputPath = ConfigurationManager.AppSettings["OutputPath"];
            string suffix = ConfigurationManager.AppSettings["Suffix"];
           
            try 
            {
                Console.WriteLine("Starting...");
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = CreateQuery(dllName);

                    Console.WriteLine("Connecting");
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var dr = cmd.ExecuteReader())
                    {
                        dr.Read();

                        var bytes = dr.GetSqlBytes(0);
                        var dllFullName = $"{outputPath}{dllName}{suffix}.dll";

                        Console.WriteLine("Write DLL");
                        WriteDll(bytes, dllFullName);

                        Console.WriteLine($"{dllFullName} has been writen successfully");
                    }
                }

                Console.WriteLine("End of process");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();
        }

        private static void WriteDll(System.Data.SqlTypes.SqlBytes bytes, string dllFullName)
        {
            using (var bytestream = new FileStream(dllFullName, FileMode.CreateNew))
            {
                bytestream.Write(bytes.Value, 0, (int)bytes.Length);
            }
        }

        private static string CreateQuery(string dllName)
        {
            return "SELECT AF.content " +
                      "FROM sys.assembly_files AF " +
                      "JOIN sys.assemblies A ON AF.assembly_id = A.assembly_id " +
                      $"WHERE AF.file_id = 1 AND A.name = '{dllName}'";
        }
    }
}


