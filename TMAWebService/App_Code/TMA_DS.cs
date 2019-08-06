using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Data.SqlClient;

namespace TMAWebService
{
    public class TMA_DS
    {
        public static string TMA_DSN()
        {

            return ConfigurationManager.ConnectionStrings["TMAConnectionString"].ConnectionString;

        }

        public static Int64 StoreMessage(TMAMessage msg)
        {
            int type_cid = 100310;     // TMA CQ Message
            int status_cid = 100202;   // Not approved
            DateTime processed_dt = DateTime.Now;  //current dt


            Int64 retVal = 0;
            SqlConnection MyConnection = null;

            try
            {
                MyConnection = new SqlConnection(TMA_DSN());
                MyConnection.Open();

                SqlCommand cmd = new SqlCommand("TMA_Message_Insert", MyConnection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Type", msg.MessageType));
                cmd.Parameters.Add(new SqlParameter("@seq_no", msg.sequenceNumber));
                cmd.Parameters.Add(new SqlParameter("@status_cid", status_cid));
                cmd.Parameters.Add(new SqlParameter("@name", msg.name));
                cmd.Parameters.Add(new SqlParameter("@language", msg.language));
                cmd.Parameters.Add(new SqlParameter("@Message", msg.ToXml()));
                cmd.Parameters.Add(new SqlParameter("@processed_dt", processed_dt));

                cmd.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (MyConnection != null)
                {
                    MyConnection.Close();
                }
            }
            return retVal;
        }

    }


}
