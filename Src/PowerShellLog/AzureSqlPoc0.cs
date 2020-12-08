using System.Data.SqlClient;
using System.Text;

namespace PowerShellLog
{
  public class AzureSqlPoc0
  {
    public static string QueryUsingTSqlExample()
    {
      var rv = "Old .net 4.8 ";

      //try
      //{
      //  using (var connection = new SqlConnection(new SqlConnectionStringBuilder { DataSource = "sqs.database.windows.net", UserID = "azuresqluser", Password = ";lkj;lkj99", InitialCatalog = "PowerShellLog" }.ConnectionString))
      //  {
      //    rv += ($"\nQuery data example: \n\t{connection.ConnectionString}\n\n");

      //    connection.Open();
      //    using (var command = new SqlCommand("SELECT        TOP (20) Id, CommandText, Note, AddedAt FROM            Cmd WHERE        (CommandText LIKE '%latest%')", connection))
      //    {
      //      using (var reader = command.ExecuteReader())
      //      {
      //        while (reader.Read())
      //        {
      //          rv += ($"  {reader.GetString(0)},   {reader.GetString(1)}\n");
      //        }
      //      }
      //    }
      //  }
      //}
      //catch (SqlException e) { rv += (e.ToString()); }

      return rv;
    }
  }
}
