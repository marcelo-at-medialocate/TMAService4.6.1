using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Data.SqlClient;
using Base;
using AppClass;
using Services;
using WebAppUtils;

using System.Net.Mail;
using System.Text;
using System.IO;

using System.Security.Principal;
using System.Runtime.InteropServices;

using System.Collections.Generic;


/// <summary>
/// Summary description for MiscTools
/// </summary>
public class MiscTools
{
    public const int LOGON32_LOGON_INTERACTIVE = 2;
    public const int LOGON32_PROVIDER_DEFAULT = 0;
    WindowsImpersonationContext impersonationContext;

    [DllImport("advapi32.dll")]
    public static extern int LogonUserA(String lpszUserName,
        String lpszDomain,
        String lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        ref IntPtr phToken);
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int DuplicateToken(IntPtr hToken,
        int impersonationLevel,
        ref IntPtr hNewToken);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool RevertToSelf();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool CloseHandle(IntPtr handle);

    public MiscTools()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static Dictionary<string, string> GetLanguages()
    {
        Dictionary<string, string> langs = new Dictionary<string, string>();

        foreach (Code c in CodeTranslator.GetCodeType("LOC"))
        {
            string lang = "";
            string description = "";
            char[] splitters = { '_', '(' };

            if (c.Alternate_Type != null && c.Alternate_Type.Length > 0)
            {
                string[] langsplit = c.Alternate_Type.Split(splitters);
                string[] descs = c.External_Description.Split(splitters);

                lang = langsplit[0];
                description = descs[0].TrimEnd();

                if (c.Alternate_Type == "en_ZW")
                {
                    description = "English";
                }

                if (langs.ContainsKey(lang) == false)
                {
                    langs.Add(lang, description);
                }
            }
        }

        return langs;
    }

    public static SortedList<string, Code> GetLocales(string lang_mnemonic)
    {
        SortedList<string, Code> locales = new SortedList<string, Code>();

        foreach (Code c in CodeTranslator.GetCodeType("LOC"))
        {
            string lang = "";
            char[] splitters = { '_', '(', ')' };

            if (c.Alternate_Type != null && c.Alternate_Type.Length > 0)
            {
                string[] langsplit = c.Alternate_Type.Split(splitters);
                string[] descs = c.External_Description.Split(splitters);
                string locale = "";

                lang = langsplit[0];
                if (lang == lang_mnemonic)
                {
                    if (descs.Length > 1)
                        locale = descs[1];
                    else
                        locale = descs[0].TrimEnd();

                    if (locales.IndexOfKey(locale) >= 0)
                        locales.Add(c.External_Description + " [" + c.Code_Value + "]", c);
                    else
                        locales.Add(locale, c);
                }
            }
        }

        return locales;
    }

    public static decimal GetProjectCostSummary(int entityIID)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        decimal amount = 0;

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Project_Job_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Entity_IID", entityIID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                amount = reader.GetDecimal(0);
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

        return amount;

    }

    public static decimal GetQuoteSectionWordCount(int quoteIID, int languageCID)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        decimal amount = 0;

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Quote_Section_Language_Word_Count", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Quote_IID", quoteIID);
            cmd.Parameters.Add("@Language_CID", languageCID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    amount = reader.GetDecimal(0);
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

        return amount;

    }

    public static void ProgressClone(int progressIID, int quoteIID, int languageCID, int userIID)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);


        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Progress_Clone", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Progress_IID", progressIID);
            cmd.Parameters.Add("@Quote_IID", quoteIID);
            cmd.Parameters.Add("@Language_CID", languageCID);
            cmd.Parameters.Add("@Modified_By_User_IID", userIID);

            cmd.ExecuteNonQuery();

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

    }

    public static decimal GetProjectTotalBilling(int projectIID)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        decimal amount = 0;


        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Project_Invoice_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Project_IID", projectIID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    amount = reader.GetDecimal(0);
                else
                    amount = 0;
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }


        return amount;

    }

    public static decimal GetYTDInvoiceAmount(int year)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        decimal amount = 0;


        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_YTD_Invoice_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Year", year);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    amount = reader.GetDecimal(0);
                else
                    amount = 0;
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }


        return amount;

    }
    public static decimal GetProjectAdjustAmount(int projectIID)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        decimal amount = 0;


        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Project_Adjust_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Project_IID", projectIID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    amount = reader.GetDecimal(0);
                else
                    amount = 0;
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }


        return amount;

    }
    public static decimal GetInvoiceAmount(int invoiceIID, bool includeBillingAmount)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        decimal amount = 0;


        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Invoice_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Invoice_IID", invoiceIID);
            cmd.Parameters.Add("@Include_Bill_Percentage", includeBillingAmount);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    amount = reader.GetDecimal(0);
                else
                    amount = 0;
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

        return amount;
    }

    public static void CanDeleteLanguage(int entityIID, int languageCID, ref int memoryCount, ref int assesssCount, ref int quoteCount, ref int poCount)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Language_Can_Delete", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Entity_IID", entityIID);
            cmd.Parameters.Add("Language_CID", languageCID);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                memoryCount = reader.GetInt32(0);
            }
            reader.NextResult();

            while (reader.Read())
            {
                assesssCount = reader.GetInt32(0);
            }
            reader.NextResult();

            while (reader.Read())
            {
                quoteCount = reader.GetInt32(0);
            }
            reader.NextResult();

            while (reader.Read())
            {
                poCount = reader.GetInt32(0);
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }
        return;

    }

    public static decimal GetProjectQuoteSummary(int entityIID)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        decimal amount = 0;

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Project_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Entity_IID", entityIID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    amount = reader.GetDecimal(0);
                else
                    amount = 0;

            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

        return amount;
    }
    public static decimal GetPOAllocated(int poIID)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        decimal amount = 0;

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_PurchaseOrder_Allocated", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("Purchase_Order_IID", poIID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0) == false)
                    amount = reader.GetDecimal(0);
                else
                    amount = 0;

            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

        return amount;
    }
    public static void GetProjectTypeSummary(Employee currEmp, Code statusCode, ref int count, ref decimal amount, ref decimal invoiced)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Project_Status_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            if (currEmp != null)
                cmd.Parameters.Add("@Employee_IID", currEmp.Employee_IID);
            cmd.Parameters.Add("@Status_CID", statusCode.CID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                count = reader.GetInt32(0);
                if (reader.IsDBNull(1) == false)
                    amount = reader.GetDecimal(1);
                else
                    amount = 0;

                if (reader.IsDBNull(2) == false)
                    invoiced = reader.GetDecimal(2);
                else
                    invoiced = 0;

            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

    }

    public static void GetProjectTypeSummary(Employee currEmp, ref int count, ref decimal amount, ref decimal invoiced)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Project_YTD_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            if (currEmp != null)
                cmd.Parameters.Add("@Employee_IID", currEmp.Employee_IID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                count = reader.GetInt32(0);
                if (reader.IsDBNull(1) == false)
                    amount = reader.GetDecimal(1);
                else
                    amount = 0;

                if (reader.IsDBNull(2) == false)
                    invoiced = reader.GetDecimal(2);
                else
                    invoiced = 0;
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

    }

    public static void GetProjectTypeSummary(Employee currEmp, Code typeAttrCID, Code statusCode, ref int count, ref decimal amount, ref decimal invoiced)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Project_Type_Status_Amount", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            if (currEmp != null)
                cmd.Parameters.Add("@Employee_IID", currEmp.Employee_IID);
            cmd.Parameters.Add("@Type_Attr_CID", typeAttrCID.CID);
            cmd.Parameters.Add("@Status_CID", statusCode.CID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                count = reader.GetInt32(0);
                if (reader.IsDBNull(1) == false)
                    amount = reader.GetDecimal(1);
                else
                    amount = 0;
                if (reader.IsDBNull(2) == false)
                    invoiced = reader.GetDecimal(2);
                else
                    invoiced = 0;

            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }

    }

    public static DataTable GetProspectSummary(int Sales_Employee_IID)
    {
        DataTable value;

        DataSet DS = new DataSet();
        string connStr = ConfigurationSettings.AppSettings["connStr"];

        SqlConnection MyConnection = new SqlConnection(connStr);
        SqlDataAdapter MyDataAdapter = new SqlDataAdapter("Base_Prospect_Stage_Summary", MyConnection);
        MyDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Sales_Employee_IID", SqlDbType.Int));
        MyDataAdapter.SelectCommand.Parameters["@Sales_Employee_IID"].Value = Sales_Employee_IID;
        MyDataAdapter.Fill(DS, "Stages");

        value = DS.Tables["Stages"];

        return value;
    }

    public static DataTable GetActivitiesSummary(int employeeIID, DateTime eventDT, int typeCID, int statusCID)
    {
        DataTable value;

        DataSet DS = new DataSet();
        string connStr = ConfigurationSettings.AppSettings["connStr"];

        SqlConnection MyConnection = new SqlConnection(connStr);
        SqlDataAdapter MyDataAdapter = new SqlDataAdapter("Base_ActivitiesCollection_Select_Detail", MyConnection);
        MyDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Employee_IID", employeeIID));
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Event_DT", eventDT));
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Type_CID", typeCID));
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Status_CID", statusCID));
        MyDataAdapter.Fill(DS, "Activities");

        value = DS.Tables["Activities"];

        return value;
    }
    public static DataTable GetActivitiesTarget(int Target_Type_CID, int Target_IID)
    {
        DataTable value;

        DataSet DS = new DataSet();
        string connStr = ConfigurationSettings.AppSettings["connStr"];

        SqlConnection MyConnection = new SqlConnection(connStr);
        SqlDataAdapter MyDataAdapter = new SqlDataAdapter("Base_ActivitiesCollection_Select_Target", MyConnection);
        MyDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Target_Type_CID", Target_Type_CID));
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Target_IID", Target_IID));
        MyDataAdapter.Fill(DS, "Activities");

        value = DS.Tables["Activities"];

        return value;
    }
    public static DataTable GetProspectsSummary(int stageCID, int employeeIID)
    {
        DataTable value;

        DataSet DS = new DataSet();
        string connStr = ConfigurationSettings.AppSettings["connStr"];

        SqlConnection MyConnection = new SqlConnection(connStr);
        SqlDataAdapter MyDataAdapter = new SqlDataAdapter("Base_ProspectCollection_Select_Detail", MyConnection);
        MyDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Stage_CID", stageCID));
        MyDataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@Employee_IID", employeeIID));
        MyDataAdapter.Fill(DS, "Prospects");

        value = DS.Tables["Prospects"];

        return value;
    }

    public static int MergeResources(int fromResourceIID, int toResourceIID)
    {
        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        int retVal = 0;

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Resource_Merge", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@From_Resource_IID", fromResourceIID);
            cmd.Parameters.Add("@To_Resource_IID", toResourceIID);

            cmd.ExecuteNonQuery();

        }
        catch (SqlException ex)
        {
            retVal = -1;
        }
        finally
        {
            conn.Close();
        }

        return retVal;

    }

    public static bool impersonateValidUser(ref WindowsImpersonationContext impersonationContext)
    {
        WindowsIdentity tempWindowsIdentity;

        string userName = ConfigurationSettings.AppSettings["IMPERS_USER"];
        string domain = ConfigurationSettings.AppSettings["IMPERS_DOM"];
        string password = ConfigurationSettings.AppSettings["IMPERS_PASS"];

        IntPtr token = IntPtr.Zero;
        IntPtr tokenDuplicate = IntPtr.Zero;

        if (RevertToSelf())
        {
            if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE,
                LOGON32_PROVIDER_DEFAULT, ref token) != 0)
            {
                if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                {
                    tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                    impersonationContext = tempWindowsIdentity.Impersonate();
                    if (impersonationContext != null)
                    {
                        CloseHandle(token);
                        CloseHandle(tokenDuplicate);
                        return true;
                    }
                }
            }
        }
        if (token != IntPtr.Zero)
            CloseHandle(token);
        if (tokenDuplicate != IntPtr.Zero)
            CloseHandle(tokenDuplicate);
        return false;
    }

    public static void undoImpersonation(WindowsImpersonationContext impersonationContext)
    {
        impersonationContext.Undo();
    }

    public static void EmailTask(Task emailTask, int userRoleIID, bool ccOriginator, IMTDService service)
    {

        // Build Email Body
        // Send Mail

        Table taskTable = new Table();
        service.LoadAttributes(emailTask);
        TaskState docState = service.LoadTaskState(emailTask.State_IID);

        Document targetDoc = service.LoadDocument(docState.Document_IID);
        FormRender formBuilder = new FormRender(targetDoc, service);
        formBuilder.populateTable(taskTable, emailTask.Attributes, null);

        // Load history information
        Table historyTable = new Table();
        TaskTransactionCollection history = service.LoadTaskTransactionCollection(emailTask);
        foreach (TaskTransaction histItem in history)
        {
            TableRow row = new TableRow();

            // Date
            TableCell cell = new TableCell();
            cell.Width = Unit.Percentage(20);
            cell.Text = histItem.Log_DT.ToShortDateString() + " " + histItem.Log_DT.ToShortTimeString();
            cell.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            row.Cells.Add(cell);
            // Type
            cell = new TableCell();
            cell.Width = Unit.Percentage(20);
            cell.Text = histItem.Transition_Type_CID.GetExternalText();
            cell.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            row.Cells.Add(cell);
            // Employee
            cell = new TableCell();
            cell.Width = Unit.Percentage(20);
            Employee logEmployee = service.LoadEmployee(histItem.Employee_IID);
            cell.Text = logEmployee.Windows_Login_ID;
            cell.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            row.Cells.Add(cell);
            // Pending Role
            cell = new TableCell();
            cell.Width = Unit.Percentage(20);
            if (histItem.Pending_User_Role_IID != -1)
            {
                UserRole logRole = service.LoadUserRole(histItem.Pending_User_Role_IID);
                cell.Text = logRole.ToString();
            }
            cell.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            row.Cells.Add(cell);
            // Pending Role
            cell = new TableCell();
            cell.Width = Unit.Percentage(20);
            if (histItem.Current_User_Role_IID != -1)
            {
                UserRole logRole = service.LoadUserRole(histItem.Current_User_Role_IID);
                cell.Text = logRole.ToString();
            }
            cell.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            row.Cells.Add(cell);

            historyTable.Rows.Add(row);

            // Add comment row
            row = new TableRow();

            cell = new TableCell();
            cell.ColumnSpan = 5;
            cell.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Left;
            cell.Text = histItem.Transition_Note.Replace("\r\n", "<br>");

            row.Cells.Add(cell);
            historyTable.Rows.Add(row);
        }

        StringBuilder SB = new StringBuilder();
        StringWriter SW = new StringWriter(SB);
        HtmlTextWriter htmlTW = new HtmlTextWriter(SW);
        taskTable.RenderControl(htmlTW);
        if (historyTable.Rows.Count > 0)
        {
            TableRow row = new TableRow();

            TableCell cell = new TableCell();
            cell.ColumnSpan = 5;
            cell.Text = "<hr>";

            row.Cells.Add(cell);
            historyTable.Rows.AddAt(0, row);

            historyTable.RenderControl(htmlTW);
        }

        UserRole uRole = service.LoadUserRole(userRoleIID);

        string mailFrom = "autoresponse@medialocate.com";
        if (ConfigurationSettings.AppSettings["FromEmail"] != null)
        {
            mailFrom = ConfigurationSettings.AppSettings["FromEmail"];
        }
        string mailTo = "@medialocate.com";
        if (ConfigurationSettings.AppSettings["ToEmail"] != null)
        {
            mailTo = ConfigurationSettings.AppSettings["ToEmail"];
        }

        // Determine email address... try employee email, otherwise build it manually
        if (uRole.User.IsContact() == true)
        {
            //mailTo = uRole.User.Login_ID; 
            return;
        }
        else if (uRole.User.IsResource() == true && uRole.User.Resource.Email_Address != null && uRole.User.Resource.Email_Address.Length > 0)
        {
            mailTo = uRole.User.Resource.Email_Address;
        }
        else if (uRole.User.IsEmployee() == true && uRole.User.Employee.Email_Address != null && uRole.User.Employee.Email_Address.Length > 0)
        {
            mailTo = uRole.User.Employee.Email_Address;
        }
        else
        {
            mailTo = uRole.User.Login_ID + mailTo;
        }

        MailMessage mail = new MailMessage(mailFrom, mailTo);
        //MailMessage mail = new MailMessage(mailFrom, "dneff@value.net");
        mail.Subject = BuildTitle(emailTask, service);
        mail.IsBodyHtml = true;
        mail.Body = BuildHeader(emailTask, service) + SB.ToString();

        if (ccOriginator == true)
        {
            string originator;

            UserRole origRole = service.LoadUserRole(emailTask.Created_User_Role_IID);
            if (origRole.User.Employee != null && origRole.User.Employee.Email_Address != null && origRole.User.Employee.Email_Address.Length > 0)
            {
                originator = origRole.User.Employee.Email_Address;
            }
            else
            {
                originator = origRole.User.Login_ID + "@medialocate.com";
            }

            mail.CC.Add(originator);
        }

        if (ConfigurationSettings.AppSettings["BCCEmail"] != null)
        {
            mail.Bcc.Add(ConfigurationSettings.AppSettings["BCCEmail"]);
        }

        try
        {
            AppClass.MTDEmail.SendSmtpMail(mail);
        }
        catch (Exception ex)
        {
            Exception ex2 = new Exception("Error Sending Mail: " + mailTo, ex);
            ErrorHandler.ErrorLog.log(ex2, "");
        }
    }

    private static string BuildTitle(Task emailTask, IMTDService service)
    {
        string retVal = "";

        retVal = emailTask.Task_CID.GetExternalText() + "; ";

        Base.Attribute projectAttr = emailTask.Attributes.Find(CodeTranslator.Find("TASK_ATTR", "PROJECT"));
        if (projectAttr != null)
        {
            BaseExtendable project = new BaseExtendable(CodeTranslator.Find("ENTITY_TYPE", "PROJECT").Code_IID);
            project.Entity_IID = (int)projectAttr.Value;
            project = (BaseExtendable)service.Load(project);
            service.LoadAttributes(project);

            Base.Attribute contactAttr = project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "CONTACT"));
            if (contactAttr != null)
            {
                Contact targetContact = new Contact((int)contactAttr.Value);
                targetContact = (Contact)service.Load(targetContact);
                retVal += targetContact.Customer_Name + "; ";

                /*
                CustomerSite targetSite = new CustomerSite(targetContact.Site_IID);
                targetSite = (CustomerSite)service.Load(targetSite);
                Customer targetCustomer = new Customer(targetSite.Customer_IID);
                targetCustomer = (Customer)service.Load(targetCustomer);
                retVal += targetCustomer.Name + "; ";
                */

            }

            retVal += project.Description + "; ";

            Base.Attribute projectNumAttr = project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "PROJECTNUM"));
            if (projectNumAttr != null)
            {
                retVal += "P#:" + (string)projectNumAttr.Value;
            }
            else
            {
                retVal += "Q#:" + project.OID;
            }

        }
        else
        {
            Base.Attribute prospectAttr = emailTask.Attributes.Find(CodeTranslator.Find("TASK_ATTR", "PROSPECT"));
            if (prospectAttr != null)
            {
                int prospectIID = (int)prospectAttr.Value;
                Prospect propect = service.LoadProspect(prospectIID);
                CustomerSite site = service.LoadCustomerSite(propect.Site_IID);
                Customer cust = service.LoadCustomer(site.Customer_IID);
                retVal += " " + propect.First_Name + " " + propect.Last_Name + " (" + cust.Name + ")";
            }
        }

        return (retVal);

    }

    private static string BuildHeader(Task emailTask, IMTDService service)
    {
        string retVal = "<strong>Task Information</strong><br>";

        Base.Attribute projectAttr = emailTask.Attributes.Find(CodeTranslator.Find("TASK_ATTR", "PROJECT"));
        if (projectAttr != null)
        {
            BaseExtendable project = new BaseExtendable(CodeTranslator.Find("ENTITY_TYPE", "PROJECT").Code_IID);
            project.Entity_IID = (int)projectAttr.Value;
            project = (BaseExtendable)service.Load(project);
            service.LoadAttributes(project);

            retVal += "Quote Number: " + project.OID + "<br>";

            Base.Attribute projectNumAttr = project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "PROJECTNUM"));
            if (projectNumAttr != null)
            {
                retVal += "Project Number: " + (string)projectNumAttr.Value + "<br>";
            }

            retVal += "Description: " + project.Description + "<br>";

            Base.Attribute contactAttr = project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "CONTACT"));
            if (contactAttr != null)
            {
                Contact targetContact = new Contact((int)contactAttr.Value);
                targetContact = (Contact)service.Load(targetContact);
                retVal += "Company: " + targetContact.Customer_Name + "<br>";

                /*
                CustomerSite targetSite = new CustomerSite(targetContact.Site_IID);
                targetSite = (CustomerSite)service.Load(targetSite);
                Customer targetCustomer = new Customer(targetSite.Customer_IID);
                targetCustomer = (Customer)service.Load(targetCustomer);
                retVal += "Company: " + targetCustomer.Name + "<br>";
                */

            }
        }

        retVal += "Task Status: " + emailTask.Task_Status_CID.GetExternalText() + "<br>";


        UserRole createRole = service.LoadUserRole(emailTask.Created_User_Role_IID);
        retVal += "Task Created By: " + createRole.User.DisplayName + "<br>";

        if (emailTask.Pending_User_Role_IID != -1)
        {
            UserRole pendingRole = service.LoadUserRole(emailTask.Pending_User_Role_IID);
            retVal += "Task Pending Acceptance: " + pendingRole.User.DisplayName + "<br>";
        }
        if (emailTask.Current_User_Role_IID != -1)
        {
            UserRole currentRole = service.LoadUserRole(emailTask.Current_User_Role_IID);
            retVal += "Task Owned By: " + currentRole.User.DisplayName + "<br>";
        }

        retVal += "<hr><br>";

        return retVal;
    }

    public static decimal GetBaseInternalRate()
    {
        decimal retVal = 0;
        string internalRate = ConfigurationSettings.AppSettings["Base_Internal_Rate"];
        if (internalRate != null && internalRate.Length > 0)
        {
            try
            {
                retVal = Convert.ToDecimal(internalRate);
            }
            catch (Exception ex)
            {
            }

        }

        return retVal;
    }

    public static Guid GetEntityGUID(int entityIID)
    {
        Byte[] bytes = new Byte[16];
        Guid retVal = new Guid(bytes); // empty

        string connStr = ConfigurationSettings.AppSettings["connStr"];
        SqlConnection conn = new SqlConnection(connStr);

        try
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("Base_Entity_Select_IID_GUID", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("@Entity_IID", entityIID);

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                retVal = reader.GetGuid(0);
            }

        }
        catch (SqlException ex)
        {
        }
        finally
        {
            conn.Close();
        }
        return retVal;
    }
}

public class EvalScore
{
    private int m_count = 0;
    private int m_scores = 0;
    private bool m_processed = false;

    public bool Processed_SW
    {
        get { return m_processed; }
    }

    public EvalScore(int score, bool Reassess_SW)
    {
        if (Reassess_SW == false)
        {
            m_scores = score;
            m_count = 1;
        }
    }

    public void addScore(int score, bool Reassess_SW)
    {
        if (Reassess_SW == false)
        {
            m_scores += score;
            m_count += 1;
        }
    }

    public int getAverage()
    {
        m_processed = true;
        if (m_count == 0)
            return 0;

        return (int)Math.Round(Convert.ToDouble(m_scores) / Convert.ToDouble(m_count));
        //return m_scores / m_count;
    }
}
