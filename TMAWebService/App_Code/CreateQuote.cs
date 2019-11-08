using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Base;
using Services;
using WebAppUtils;
using AppClass;

namespace TMAWebService
{
    public class CreateQuote
    {
        public BaseExtendable createMTDQuote(int contact_iid, string projDesc, TMAMessage msg, IMTDService svc, UserSession us, Guid mtdGUID, int srcLangCID, int[] targetLangCIDs)
        {
            
            BaseExtendable newQuote = new BaseExtendable(Code.Find(AppCodes.PROJECT_TARGET_TYPE).CID);
            if (msg.name != null )
            {
                newQuote.Description = msg.name;
            }
            else
            {
                newQuote.Description = projDesc;
            }
            newQuote.OID = svc.NextSequenceNumber(true);
            svc.Store(newQuote);
            ///// Add Attributes
            /////////////////////////////////////////////////////////////////////////////////////

 //           int contact_iid = Convert.ToInt32(ConfigurationSettings.AppSettings["CONTACT_IID"]);

            Contact contact = svc.LoadContact(contact_iid);
            CustomerSite site = svc.LoadCustomerSite(contact.Site_IID);
            int salesPersonIID = site.Sales_Employee_IID;
            if (salesPersonIID == -1)
            {
                salesPersonIID = us.Current_User.Employee.Employee_IID; // vilma
            }

            Base.Attribute salesAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "SALESPERSON"), salesPersonIID);
            salesAttr.SetParent(newQuote.Attributes);       

            Base.Attribute contactAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "CONTACT"), contact_iid);
            contactAttr.SetParent(newQuote.Attributes);                                         
            // msg.status ='approved"
             Base.Attribute statusAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "STATUS"), Code.Find(10033));
             statusAttr.SetParent(newQuote.Attributes);
            // Quoted
            int perCt = 10;
            Base.Attribute pmPercentAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "PM_PERCENT"), perCt);
            pmPercentAttr.SetParent(newQuote.Attributes);

            decimal internalRate = 35;
            Base.Attribute internalRateAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "BASE_INTERNAL_RATE"), internalRate);
            internalRateAttr.SetParent(newQuote.Attributes); 

            Base.Attribute sourceAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "SOURCE_LANG"), Code.Find(srcLangCID));
            sourceAttr.SetParent(newQuote.Attributes);
            int targetCID;  
            int numLangs = targetLangCIDs.Length;                       
            for (int i = 0; i < numLangs; i++ )
            {
               targetCID = targetLangCIDs[i];
               if (targetCID > 0)
               {
                   Base.Attribute targetLang = newQuote.Attributes.Add(CodeTranslator.Find("CTYPE", "LOC"), Code.Find(targetCID), 0);
                   targetLang.SetParent(newQuote.Attributes);
               }                                         
           }
            /*
            Base.Attribute budgetAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "BUDGET"), this.BudgetCheckBox.Checked);
            budgetAttr.SetParent(newQuote.Attributes);
            Base.Attribute rushAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "RUSH_JOB"), this.RushCheckBox.Checked);
            rushAttr.SetParent(newQuote.Attributes);
            Base.Attribute reverseAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "LANGUAGES_REVERSED"), this.ReverseCheckBox.Checked);
            reverseAttr.SetParent(newQuote.Attributes);
            Base.Attribute cleanAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "PRE_TRANS_CLEANING"), this.CleanCheckBox.Checked);
            cleanAttr.SetParent(newQuote.Attributes);
            */

            //DateTime dTime = new DateTime(2012, 09, 01, 12, 00, 00);
            //String dTime;
            //dTime = defDTime.ToString("yyyy-MM-dd HH:mm tt");

            Base.Attribute dateQuoteStartAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "QUOTE_START_DT"), DateTime.Now);
            dateQuoteStartAttr.SetParent(newQuote.Attributes);

            Base.Attribute dateAssessStartAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "ASSESS_START_DT"), DateTime.Now);
            dateAssessStartAttr.SetParent(newQuote.Attributes);

            Base.Attribute dateQuoteDueeAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR","QUOTE_DUE_DT"), DateTime.Now.AddDays(1) );
            dateQuoteDueeAttr.SetParent(newQuote.Attributes);

            Base.Attribute dateAsessDueAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "ASSESS_DUE_DT"), DateTime.Now.AddDays(1) );
            dateAsessDueAttr.SetParent(newQuote.Attributes);

            /*
            Base.Attribute dateAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "PROJECT_DUE_DT"), dTime);
                dateAttr.SetParent(newQuote.Attributes);
            Base.Attribute dateAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "PROD_DUE_DT"), dTime);
                dateAttr.SetParent(newQuote.Attributes);
           */

            svc.Store(newQuote);

            
            msg.quoteOID = newQuote.OID;
            // Update Quote if first
            ProspectCollection prospects = svc.LoadProspectCollection(Convert.ToInt32("5157"));
            if (prospects.Count > 0)
            {
                Prospect updateProspect = (Prospect)prospects[0];
                if (updateProspect.Quote_IID == -1)
                {
                    updateProspect.Quote_IID = newQuote.Entity_IID;
                    svc.Store(updateProspect);
                }
            }

            BaseExtendable m_project = newQuote;
            return m_project;
        }

        public BaseExtendable createMTDProject(int contact_iid, string projDesc, TMAMessage msg, IMTDService svc, UserSession us, Guid mtdGUID, int srcLangCID,  int[] targetLangCIDs, bool monthlyFlag)
        {

            BaseExtendable newQuote = new BaseExtendable(Code.Find(AppCodes.PROJECT_TARGET_TYPE).CID);
            if (msg.name != null)
            {
                newQuote.Description = msg.name;
            }
            else
            {
                newQuote.Description = projDesc;
            }
            newQuote.OID = svc.NextSequenceNumber(true);
            svc.Store(newQuote);
            ///// Add Attributes
            /////////////////////////////////////////////////////////////////////////////////////

            //           int contact_iid = Convert.ToInt32(ConfigurationSettings.AppSettings["CONTACT_IID"]);

            Contact contact = svc.LoadContact(contact_iid);
            CustomerSite site = svc.LoadCustomerSite(contact.Site_IID);
            int salesPersonIID = site.Sales_Employee_IID;
            if (salesPersonIID == -1)
            {
                salesPersonIID = us.Current_User.Employee.Employee_IID; // vilma
            }

            Base.Attribute salesAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "SALESPERSON"), salesPersonIID);
            salesAttr.SetParent(newQuote.Attributes); 

            Base.Attribute contactAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "CONTACT"), contact_iid);
            contactAttr.SetParent(newQuote.Attributes);
            // msg.status ='approved"
            Base.Attribute statusAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "STATUS"), Code.Find(10033));
            statusAttr.SetParent(newQuote.Attributes);
            // Quoted
            decimal perCt = 10;
            Base.Attribute pmPercentAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "PM_PERCENT"), perCt);
            pmPercentAttr.SetParent(newQuote.Attributes);

            decimal internalRate = 35;
            Base.Attribute internalRateAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "BASE_INTERNAL_RATE"), internalRate);
            internalRateAttr.SetParent(newQuote.Attributes);                                            // use default from MTD web config

            Base.Attribute sourceAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "SOURCE_LANG"), Code.Find(srcLangCID));
            sourceAttr.SetParent(newQuote.Attributes);
            int targetCID;
            int numLangs = targetLangCIDs.Length;
            for (int i = 0; i < numLangs; i++)
            {
                targetCID = targetLangCIDs[i];
                if (targetCID > 0)
                {
                    Base.Attribute targetLang = newQuote.Attributes.Add(CodeTranslator.Find("CTYPE", "LOC"), Code.Find(targetCID), 0);
                    targetLang.SetParent(newQuote.Attributes);
                }
            }
            /*
            Base.Attribute budgetAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "BUDGET"), this.BudgetCheckBox.Checked);
            budgetAttr.SetParent(newQuote.Attributes);
            Base.Attribute rushAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "RUSH_JOB"), this.RushCheckBox.Checked);
            rushAttr.SetParent(newQuote.Attributes);
            Base.Attribute reverseAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "LANGUAGES_REVERSED"), this.ReverseCheckBox.Checked);
            reverseAttr.SetParent(newQuote.Attributes);
            Base.Attribute cleanAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "PRE_TRANS_CLEANING"), this.CleanCheckBox.Checked);
            cleanAttr.SetParent(newQuote.Attributes);
            */

            //DateTime dTime = new DateTime(2012, 09, 01, 12, 00, 00);
            //String dTime;
            //dTime = defDTime.ToString("yyyy-MM-dd HH:mm tt");

            Base.Attribute dateQuoteStartAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "QUOTE_START_DT"), DateTime.Now);
            dateQuoteStartAttr.SetParent(newQuote.Attributes);

            Base.Attribute dateAssessStartAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "ASSESS_START_DT"), DateTime.Now);
            dateAssessStartAttr.SetParent(newQuote.Attributes);

            Base.Attribute dateQuoteDueeAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "QUOTE_DUE_DT"), DateTime.Now.AddDays(1));
            dateQuoteDueeAttr.SetParent(newQuote.Attributes);

            Base.Attribute dateAsessDueAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "ASSESS_DUE_DT"), DateTime.Now.AddDays(1));
            dateAsessDueAttr.SetParent(newQuote.Attributes);
            /*
            Base.Attribute dateAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "ASSESS_DUE_DT"), dTime);
                dateAttr.SetParent(newQuote.Attributes);

            Base.Attribute dateAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "PROJECT_DUE_DT"), dTime);
                dateAttr.SetParent(newQuote.Attributes);
            Base.Attribute dateAttr = newQuote.Attributes.Add(Code.Find("PROJECT_ATTR", "PROD_DUE_DT"), dTime);
                dateAttr.SetParent(newQuote.Attributes);
           */

            svc.Store(newQuote);


            msg.quoteOID = newQuote.OID;



            //convert Quote to Project
            string projectNum = svc.NextSequenceNumber(false);

            BaseExtendable m_project = newQuote;

            Code statusCode = Code.Find("PROJECT_STATUS", "LINGUISTIC");
            statusAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "STATUS"));   
            if (statusAttr == null)
            {
 
                statusAttr = m_project.Attributes.Add(CodeTranslator.Find("PROJECT_ATTR", "STATUS"), statusCode);
                statusAttr.SetParent(m_project.Attributes);
            }
            else
            {
               statusAttr.Value = statusCode;
 
            }
            Base.Attribute  projStartDate = m_project.Attributes.Add(Code.Find("PROJECT_ATTR", "PROJECT_START_DT"), DateTime.Now);
            projStartDate.SetParent(m_project.Attributes);

            Base.Attribute prodStartDate =  m_project.Attributes.Add(Code.Find("PROJECT_ATTR", "PROD_START_DT"), DateTime.Now);
            prodStartDate.SetParent(m_project.Attributes);

            
            Base.Attribute  monthlyAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "MONTHLY"));
            if (monthlyAttr == null && monthlyFlag == true)
            {
                monthlyAttr = m_project.Attributes.Add(CodeTranslator.Find("PROJECT_ATTR", "MONTHLY"), monthlyFlag);
                monthlyAttr.SetParent(m_project.Attributes);
            }
        
            m_project.Attributes.Add(CodeTranslator.Find("PROJECT_ATTR", "PROJECTNUM"), projectNum);

            bool itar = false;
            Base.Attribute itarAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "ITAR"));
            if (itarAttr != null)
            {
                itar = ((bool)itarAttr.Value);
                itarAttr.SetParent(m_project.Attributes);
            }

            svc.Store(m_project);
            return m_project;
        }

        public void WriteDebugfile(string line)
        {
            //if debug, open local file for logging
            string debug = ConfigurationManager.AppSettings["MTD_DEBUG"];
            if (debug == "TRUE")
            {
                string debugFile = ConfigurationManager.AppSettings["MTD_DEBUGFILE"];
                if (!File.Exists(debugFile))
                {
                    using (StreamWriter sw = File.CreateText(debugFile))
                    { sw.WriteLine(line); }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(debugFile))
                    { sw.WriteLine(line); }
                }
            }


        }
    }
}