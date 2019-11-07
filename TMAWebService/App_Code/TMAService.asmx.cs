//TODO
//- Add return message when MTDGUID is incorrect

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.IO.Compression;
using System.Threading;
//using Ionic.Zip;
using AppClass;
using Base;
using Services;
using WebAppUtils;
using System.Security.Principal;
using Com.Idiominc.Webservices.Client;
using Com.Idiominc.Webservices.Client.Workflow;
using Com.Idiominc.Webservices.Client.User;

namespace TMAWebService
{
    [Serializable]
    public class TMAMsg
    {
        [XmlArrayItem("UID")]
        public List<string> IDs = new List<string>();
    }
    [WebService(Namespace = "http://tma.medialocate.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]


    public class TMAService : System.Web.Services.WebService
    {
        private BaseExtendable m_project = null;
 

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }


        [WebMethod]
        public string Add_Update_Content(
            Int64 seqNo,
            string MTD_GUID,
            string contentType,
//            string UID,
            string source_lang,
            string target_langs,
//            string client_id,
            string date,
            string projName,
            string projDescription,
            string header,
            string XMLMsg,
            string XMLFileName
            )
        {
            TMAMessage msg = new TMAMessage(TMAMessage.MSG_TYPE.Add_Update_Doc_Content);
            string retVal = "";
            string quoteDir = "";
            string transDir = "";
            string filetype = "xml";
            string fileDesc = "";  //quote description
//            string clientId = "";
            string quoteType = "";  //from msg parms
            string mtdProjectID = "";  //MTD project id
            string wsProjectID = ""; //WS project group id
            string projectTrackingCode = ""; //mtdProjectID_wsProjectGroupID
            int fileCount = 1;
            var XMLfiles = new List<string>();
            string XMLfilesNameDesc = "";
            DateTime dt = DateTime.Now;
            Boolean zipfileAttached = false;
            String projectFile = "";
            char[] sep = { ';' };
            string debugLine = DateTime.Now + " Start processing " + dt.ToString(); WriteDebugfile(debugLine);
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            debugLine = DateTime.Now + " Start processing1 " + dt.ToString(); WriteDebugfile(debugLine);
            if (MTD_GUID == "")
            {
                return ("GUID_NOT_FOUND");
            }
            debugLine = DateTime.Now + " Start processing1.1 " + dt.ToString(); WriteDebugfile(debugLine);
            if (XMLFileName == "" && XMLMsg == "")
            {
                return ("Error: No File or Content was Received");
            }
            debugLine = DateTime.Now + " Start processing1.2 " + dt.ToString(); WriteDebugfile(debugLine);
            string[] projAttribs = null;

            debugLine = DateTime.Now + " Start processing1.3  " + dt.ToString(); WriteDebugfile(debugLine);
            if (XMLMsg == null)
            {
                XMLMsg = "";
            }

            if (header == null)
            {
                header = "";
            }
            if (header != "")
            {
                projAttribs = header.Split(sep);
                debugLine = DateTime.Now + " Start processing1.4 " + dt.ToString(); WriteDebugfile(debugLine);
                foreach (string projAttrib in projAttribs)
                {
                    string prjAttrib = projAttrib.Trim().ToLower();

                    switch (prjAttrib)
                    {
                        case "update":
                            break;
                        case "review":
                            break;
                        case "client_review":
                            break;
                        case "":
                            break;
                        default:
                            return "Error: Header field has incorrect modifiers";
                    }
                }
                debugLine = DateTime.Now + " Start processing1.5 " + dt.ToString(); WriteDebugfile(debugLine);
            }
            

            debugLine = DateTime.Now + " Start processing 2 " + dt.ToString(); WriteDebugfile(debugLine);
            if (XMLFileName.Contains(".zip"))
            {
                debugLine = DateTime.Now + " Start processing 3 " + dt.ToString(); WriteDebugfile(debugLine);
                string XMLFileNames = "";
                debugLine = DateTime.Now + " Start processing 4" + dt.ToString(); WriteDebugfile(debugLine);
                XMLfiles = UnpackZipFile(XMLFileName);
                debugLine = DateTime.Now + " Start processing 5 " + dt.ToString(); WriteDebugfile(debugLine);
                fileCount = XMLfiles.Count();
                if (fileCount < 1)
                {
                    retVal = "Error unpacking or reading zip file " + XMLFileName;
                    return retVal;
                }
                debugLine = DateTime.Now + " Start processing 6 " + dt.ToString(); WriteDebugfile(debugLine);
                for (int j = 0; j < fileCount; j++)
                {


                    if (XMLfiles.Count > 0)
                    {
                        XMLMsg = System.IO.File.ReadAllText(XMLfiles[j]);

                        if (XMLMsg == "")
                        {
                            XMLFileNames = XMLFileNames + ", " + XMLfiles[j];
                        }
                       
                    }      
                 }
                if (XMLFileNames != "")
                {
                    retVal = "Error could not read file(s) " + XMLFileNames;
                    return retVal;
                }
                zipfileAttached = true;
                projectFile = XMLFileName;
                debugLine = DateTime.Now + " Start processing  7 " + dt.ToString(); WriteDebugfile(debugLine);
                XMLfilesNameDesc = XMLFileName.Substring(XMLFileName.LastIndexOf("\\") + 1);

            } 
            else 
            {
                string fileNamePart;
                if (XMLFileName == "" && !(XMLMsg == ""))
                {               
                    fileNamePart = DateTime.Now.ToString("yyyyMMddHHmmssff") + ".xml";
                    projectFile = ConfigurationSettings.AppSettings["TEMPFOLDER"] + "\\temp" + fileNamePart;
                    XMLMsg = XMLMsgDecode(XMLMsg);
                    System.IO.File.WriteAllText(projectFile, XMLMsg, Encoding.UTF8);
                }
                else
                {
                    fileNamePart = XMLFileName.Substring(XMLFileName.LastIndexOf("\\") + 1);
                    projectFile = ConfigurationSettings.AppSettings["TEMPFOLDER"] + "\\" + fileNamePart;
                    XMLMsg = System.IO.File.ReadAllText(XMLFileName);
                    System.IO.File.WriteAllText(projectFile, XMLMsg, Encoding.UTF8);
                }

                XMLfilesNameDesc = fileNamePart;
                XMLFileName = projectFile;
              //  if (XMLMsg.StartsWith(_byteOrderMarkUtf8))
              //  {
              //      XMLMsg = XMLMsg.Remove(0, _byteOrderMarkUtf8.Length);
              //  }

              //  Encoding outputEnc = new UTF8Encoding(false); // create encoding with no BOM
              //  TextWriter file = new StreamWriter(projectFile, false, outputEnc); // open file with encoding
                // write data here
              //  file.Write(XMLMsg);
              //  file.Close(); // save and close it
              //  projectFile = XMLFileName;
            }

            // ReadInputMsg(msg, XMLMsg, UID);
            EntityVersion newVersion = null;

            string userName = ConfigurationSettings.AppSettings["MTD_USR"];
            string pwd = ConfigurationSettings.AppSettings["MTD_PWD"];
            WSScopeMgr scpMgr = new WSScopeMgr();

            // the below locales should come from the XML message
            string srcLang = "en_US";  //""English (United States) - UTF8";  //XML msg
            string targLangs = "ar_EG;ru_RU";   // "Arabic (Egypt) - UTF8;Russian - UTF8";  //XML msg
            string srcMount = "backup/MDL";  //web.config
            string WSsrcLang = "";
            string WStargLangs = "";
            Code srcLangCode = null;
            Code targLangCode = null;

            srcMount = XMLFileName;
            string targetMount = "Client Files/Medialocate/Temp/test";  //web.config
            //check source and target languages
            source_lang = source_lang.Replace("-", "_");
            target_langs = target_langs.Replace("-", "_");
            if (source_lang != "")
            { srcLang = source_lang; }
            if (target_langs != "")
            { targLangs = target_langs; }
            if (projDescription != "")
            { fileDesc = projDescription; }
//                if (client_id != "")
//                { clientId = client_id; }
            UserSession us = new UserSession();
            User user = us.AuthenticateUser(userName, pwd);
            string transLangDesc = "";
            debugLine = DateTime.Now + " Authenticated user " + userName; WriteDebugfile(debugLine);
            IMTDService service = us.MTDService;

            if (srcLang == "en_EM" || srcLang == "en_AP" || srcLang == "en_AS" || srcLang == "en_EU")
            {
                srcLang = "en_US";
            }
            //need to change format to match WS language codes
            srcLangCode = Code.FindAltType("LOC", srcLang);
            if (srcLangCode == null)
            {
                retVal = "Source locale not found";
                return retVal;
            }

            int srcLangCID = srcLangCode.CID;  //used in CreateQuote

            string[] targetLanguages = new string[20];
            Code[] targetLangCodes = new Code[20];
            int[] targetLangCIDs = new int[20];
            targetLanguages[0] = targLangs;
            int targLangCount = 0;
            debugLine = DateTime.Now + " targLangs= " + targLangs; WriteDebugfile(debugLine);

            if (targLangs.Contains(";"))
            {
                targetLanguages = targLangs.Split(sep);
                int numLocs = targetLanguages.Length;
                if (numLocs > targetLangCIDs.Length)
                {
                    return "Too many Locales submitted";
                }
                debugLine = DateTime.Now + " numLocs= " + numLocs; WriteDebugfile(debugLine);
                for (int i = 0; i < numLocs; i++)
                {
                    targetLanguages[i] = targetLanguages[i].Replace("es_LA","es_EC");
                    targLangCode = Code.FindAltType("LOC", targetLanguages[i].Trim());

                    if (targLangCode == null)
                    {
                        retVal = "Target locale not found";
                        return retVal;
                    }
                    targetLangCodes[i] = targLangCode;
                    targLangCount++;
                    debugLine = DateTime.Now + " targLangCount= " + targLangCount + ", targLangCode= " + targLangCode; WriteDebugfile(debugLine);
                    targetLangCIDs[i] = targLangCode.CID;
                    if (i > 0 && i < numLocs) 
                    {
                        transLangDesc += ";";
                    }
                    transLangDesc += targetLangCodes[i].Internal_Description;
                }
            }
            else
            {
                targLangs = targLangs.Replace("es_LA", "es_EC");
                targLangCode = Code.FindAltType("LOC", targLangs);
                if (targLangCode == null)
                {
                    retVal = "Target locale not found";
                    return retVal;
                }
                targLangCount = 1;
                targetLangCodes[0] = targLangCode;
                targetLangCIDs[0] = targLangCode.CID;
                transLangDesc = targetLangCodes[0].Internal_Description;
            }
            Guid mtdGUID = new Guid(MTD_GUID);  //msg parms "1EA7EDE9-AA8E-4384-AB73-C1E8BF9DE9AA"
            AppClass.WSParamCollection parms = service.LoadWSParamCollection(mtdGUID);

            debugLine = DateTime.Now + " Loaded WS Params with MTD_GUID " + MTD_GUID; WriteDebugfile(debugLine);

            string WS_Client_Name = getWSClient(parms, mtdGUID);
            string Project_Type = getWSProjectType(parms, mtdGUID);
            string MTD_Mode = getModeCID(parms, mtdGUID);
            string WSurl = getWSurl(parms, mtdGUID);
            int ContactIID = getWSContactIID(parms, mtdGUID);
            debugLine = DateTime.Now + " MTD Mode " + MTD_Mode; WriteDebugfile(debugLine);

            CreateQuote cq;
            BuildDir bdr;
            newVersion = new EntityVersion();
            newVersion.Target_Type_CID = Code.Find(AppCodes.QUOTE_TARGET_TYPE);
            newVersion.Version = "1.0";
            newVersion.Released = true;

            //int len = parms.Count;
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            int day = DateTime.Today.Day;
            int hour = DateTime.Now.Hour;
            int minute = DateTime.Now.Minute;
            //create a quote and folder for today
            string dayPart = day.ToString();
            string monthPart = month.ToString();
            string yearPart = year.ToString();
            string hourPart = hour.ToString();
            string minPart = minute.ToString();
            if (month < 10) { monthPart = "0" + monthPart; }
            if (day < 10) { dayPart = "0" + dayPart; }
            if (hour < 10) { hourPart = "0" + hourPart; }
            if (minute < 10) { minPart = "0" + minPart; }
            string yyyyMMDDHHMM = yearPart + monthPart + dayPart + hourPart + minPart;
            string yyMM = yearPart.Substring(2, 2) + monthPart;
            string projPathPrefix = "Projects\\";
            string quotePathPrefix = "Quotes\\";
            string mtdQuoteNum;
            Boolean newProject = false;
            string [] entityIIDArray = new string[4];
            if (MTD_Mode.Contains("MONTHLY"))
            {
                DateTime now = DateTime.Now;
                string monthNow = now.ToString("MMMM");
                string monthYear = monthPart + yearPart.Substring(2,2);
                //see if there is already an MTD project ID for this client
                bool createdMonthlyProject = doesMonthlyProjExist(service, parms, mtdGUID, ContactIID, monthYear);
                debugLine = DateTime.Now + " Contact ID " + ContactIID; WriteDebugfile(debugLine);
                mtdProjectID = getMTDProjID(parms, mtdGUID);
                if (mtdProjectID == "" || !createdMonthlyProject)
                {
                    cq = new CreateQuote();
                    m_project = cq.createMTDProject(ContactIID, "CMS Monthly Project - " + monthNow, msg, service, us, mtdGUID, srcLangCID, targetLangCIDs, true);
                    mtdQuoteNum = m_project.OID;  //to be used for project tracking code
                    newProject = true;
                }
                else
                {
                    debugLine = DateTime.Now + " Getting Monthly Project Collection"; WriteDebugfile(debugLine);

                    string[] mtdQuoteNumArray;


                    if (mtdProjectID.Contains('_'))
                    {
                        mtdQuoteNumArray = mtdProjectID.Split('_');
                        mtdQuoteNum = mtdQuoteNumArray[0].Trim();
                    }
                    else
                    {
                        mtdQuoteNum = mtdProjectID;
                    }

                    ProjectCollection m_projects = service.LoadProjectCollection(mtdQuoteNum);

                    foreach (BaseExtendable project in m_projects)
                    {
                        if (project.OID == mtdQuoteNum)
                        {
                            int projectIID = Convert.ToInt32(project.Entity_IID);
                            m_project = new BaseExtendable(CodeTranslator.Find("ENTITY_TYPE", "PROJECT").Code_IID);
                            m_project.Entity_IID = projectIID;
                            m_project = (BaseExtendable)service.Load(m_project);
                            service.LoadAttributes(m_project);                               
                        }
                    }
                    // TODO: Test if all languages are aleady setup in the MTD project. Otherwise, add them to the MTD project.

                    debugLine = DateTime.Now + " Got Monthly Project Collection"; WriteDebugfile(debugLine);
                }

                // Check and add new locales to MTD
                int targetCID;
                int numLangs = targetLangCIDs.Length;
                ArrayList targetLoc = m_project.Attributes.GetAttributeList(CodeTranslator.Find("CTYPE", "LOC"));
                for (int i = 0; i < numLangs; i++)
                {
                    bool inMTD = false;
                    targetCID = targetLangCIDs[i];
                    if (targetCID > 0)
                    {
                        foreach (Base.Attribute targetLang in targetLoc)
                        {
                            Code lang = (Code)targetLang.Attribute_IID;
                            if (targetCID == lang.CID)
                            {
                                inMTD = true;
                            }

                        }
                        if (inMTD == false)
                        {
                            Base.Attribute targetLang = m_project.Attributes.Add(CodeTranslator.Find("CTYPE", "LOC"), Code.Find(targetCID), 0);
                            service.Store(targetLang);
                        }
                    }
                }


                Base.Attribute projectAttr = m_project.Attributes.Find(Code.Find("PROJECT_ATTR", "PROJECTNUM"));
                string mtdProjectNum = (string)projectAttr.Value;

                Base.Attribute salesAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "SALESPERSON"));
                AppClass.Employee sales = service.LoadEmployee((int)salesAttr.Value);
                int salesIID = sales.Employee_IID;
                debugLine = DateTime.Now + " Got Sales IID =  " + salesIID; WriteDebugfile(debugLine);
                string mtdQuoteMMYY = mtdQuoteNum.Substring(0, 4);
                string mtdQuoteSeqNum = mtdQuoteNum.Substring(4, mtdQuoteNum.Length - 4);
                quotePathPrefix = quotePathPrefix + mtdQuoteMMYY + "\\" + mtdQuoteSeqNum + "\\";
                string mtdProjectYYMM = mtdProjectNum.Substring(0, 4);
                string mtdProjectSeqNum = mtdProjectNum.Substring(4, mtdProjectNum.Length - 4);
                projPathPrefix = projPathPrefix + mtdProjectYYMM + "\\" + mtdProjectSeqNum + "\\";  //project OID = seq No
                //create Version
                service.Store(newVersion);


//                   EntityRelation newRelation = new EntityRelation(Code.Find(AppCodes.QUOTE_TARGET_TYPE));
//                   //newRelation.Entity_IID_1 = m_project.Entity_IID;
//                   newRelation.Entity_IID_2 = m_project.Entity_IID;
//                    newRelation.Entity_2_Version_IID = newVersion.Entity_Version_IID;
//                   newRelation.Entity_2.Description = projDescription;

//                    service.Store(newRelation);

//                if (WriteSendLog(service, user, m_project) == false)
//                {
//                    debugLine = DateTime.Now + " Error writing SendLog"; WriteDebugfile(debugLine); 
//                    retVal = "Error writing SendLog";
//                }
                ///// Build Project Directory


                //m_project.OID.Substring(4, m_project.OID.Length - 4));
                debugLine = DateTime.Now + " After writing SendLog"; WriteDebugfile(debugLine);
                entityIIDArray = scpMgr.getScopeInfo(srcMount, projName, projDescription, projectFile, srcLangCode, targetLangCodes, targLangCount, filetype, m_project, service, WS_Client_Name, Project_Type, MTD_Mode, WSurl, mtdProjectNum,projAttribs);
                projectTrackingCode = entityIIDArray[2] + "_" + entityIIDArray[3];
                wsProjectID = Convert.ToString(entityIIDArray[3]);

                //finally, create directory
                bdr = new BuildDir();
                if (newProject == true)
                {
                    bdr.buildProjDirStructure(mtdProjectYYMM, mtdProjectSeqNum, false);
                }
                quoteDir = bdr.buildQuoteDirectory(quotePathPrefix, yyyyMMDDHHMM, projName, XMLFileName);
                transDir = bdr.buildTransDirectory(projPathPrefix, yyyyMMDDHHMM, wsProjectID, projName, targetLanguages, XMLFileName);
                debugLine = DateTime.Now + " Built Monthly quoteDir " + quoteDir; WriteDebugfile(debugLine);
                debugLine = DateTime.Now + " Built Monthly trans directory " + transDir; WriteDebugfile(debugLine);

                if (mtdProjectID == "" || !createdMonthlyProject)
                {
                    create_m_Task(service, ContactIID, salesIID, m_project.Entity_IID, "Customer Additions", srcLangCode.Internal_Description, transLangDesc, XMLfilesNameDesc);
                }
                retVal = projectTrackingCode;
                msg.description = "Added Project " + projectTrackingCode;
                //send configured WS email
                NotifyWS nws = new NotifyWS();
                nws.SendMessage(msg.quoteOID, msg.description);
            }
            else if (MTD_Mode.Contains("ONE-TIME"))
            {
                //for one-off quotes, create a new project each time
                cq = new CreateQuote();
                m_project = cq.createMTDProject(ContactIID, fileDesc, msg, service, us, mtdGUID, srcLangCID, targetLangCIDs, false);
                mtdQuoteNum = m_project.OID;    //to be used for project tracking code

                Base.Attribute projectAttr = m_project.Attributes.Find(Code.Find("PROJECT_ATTR", "PROJECTNUM"));
                string mtdProjectNum = (string)projectAttr.Value;
                
                Base.Attribute salesAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "SALESPERSON"));
                AppClass.Employee sales = service.LoadEmployee((int)salesAttr.Value);
                int salesIID = sales.Employee_IID;

                string mtdQuoteMMYY = mtdQuoteNum.Substring(0, 4);
                string mtdQuoteSeqNum = mtdQuoteNum.Substring(4, mtdQuoteNum.Length - 4);
                quotePathPrefix = quotePathPrefix + mtdQuoteMMYY + "\\" + mtdQuoteSeqNum + "\\";

                string mtdProjectYYMM = mtdProjectNum.Substring(0, 4);
                string mtdProjectSeqNum = mtdProjectNum.Substring(4, mtdProjectNum.Length - 4);
                projPathPrefix = projPathPrefix + mtdProjectYYMM + "\\" + mtdProjectSeqNum + "\\";  //project OID = seq No

                //create Version
                service.Store(newVersion);


                //                   EntityRelation newRelation = new EntityRelation(Code.Find(AppCodes.QUOTE_TARGET_TYPE));
                //                   //newRelation.Entity_IID_1 = m_project.Entity_IID;
                //                   newRelation.Entity_IID_2 = m_project.Entity_IID;
                //                    newRelation.Entity_2_Version_IID = newVersion.Entity_Version_IID;
                //                   newRelation.Entity_2.Description = projDescription;

                //                    service.Store(newRelation);

 //               if (WriteSendLog(service, user, m_project) == false)
 //               {
 //                   debugLine = DateTime.Now + " Error writing SendLog"; WriteDebugfile(debugLine);
 //                   retVal = "Error writing SendLog";
 //               }
                ///// Build Project Directory


                //m_project.OID.Substring(4, m_project.OID.Length - 4));
                debugLine = DateTime.Now + " MTD Mode2 " + MTD_Mode; WriteDebugfile(debugLine);
                entityIIDArray = scpMgr.getScopeInfo(srcMount, projName, projDescription, XMLMsg, srcLangCode, targetLangCodes, targLangCount, filetype, m_project, service, WS_Client_Name, Project_Type, MTD_Mode, WSurl, mtdProjectNum, projAttribs);
                projectTrackingCode = entityIIDArray[2] + "_" + entityIIDArray[3];
                wsProjectID = Convert.ToString(entityIIDArray[3]);

                //finally, create directory
               debugLine = DateTime.Now + " MTD Mode3 " + MTD_Mode; WriteDebugfile(debugLine);
                bdr = new BuildDir();
                bdr.buildProjDirStructure(mtdProjectYYMM, mtdProjectSeqNum, false);
                quoteDir = bdr.buildQuoteDirectory(quotePathPrefix, yyyyMMDDHHMM, projName, XMLFileName);
                transDir = bdr.buildTransDirectory(projPathPrefix, yyyyMMDDHHMM, wsProjectID, projName, targetLanguages,XMLFileName);
                debugLine = DateTime.Now + " Built Monthly quoteDir " + quoteDir; WriteDebugfile(debugLine);
                debugLine = DateTime.Now + " Built Monthly trans directory " + transDir; WriteDebugfile(debugLine);

                create_m_Task(service,ContactIID,salesIID,m_project.Entity_IID,"Start project without quote",srcLangCode.Internal_Description,transLangDesc,XMLfilesNameDesc);
                retVal = projectTrackingCode;
                msg.description = "Added Project " + projectTrackingCode;
                //send configured WS email
                NotifyWS nws = new NotifyWS();
                nws.SendMessage(msg.quoteOID, msg.description);

            }
            else
            { 
                cq = new CreateQuote();
                m_project = cq.createMTDQuote(ContactIID, fileDesc, msg, service, us, mtdGUID, srcLangCID, targetLangCIDs);
                
                Base.Attribute salesAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "SALESPERSON"));
                AppClass.Employee sales = service.LoadEmployee((int)salesAttr.Value);
                int salesIID = sales.Employee_IID;

                //create Version
                service.Store(newVersion);
                mtdQuoteNum = m_project.OID;


                EntityRelation newRelation = new EntityRelation(Code.Find(AppCodes.QUOTE_TARGET_TYPE));
                //newRelation.Entity_IID_1 = m_project.Entity_IID;
                newRelation.Entity_IID_2 = m_project.Entity_IID;
                newRelation.Entity_2_Version_IID = newVersion.Entity_Version_IID;
                service.Store(newRelation);

                debugLine = DateTime.Now + " Created quote " + m_project.Entity_IID.ToString(); WriteDebugfile(debugLine);
                ///// Build Quote Directory

                //return value should be assessmnetIID, quoteItemIID,  m_project.oid & WSProjectGroupID
                entityIIDArray = scpMgr.getScopeInfo(srcMount, projName, projDescription, XMLMsg, srcLangCode, targetLangCodes, targLangCount, filetype, m_project, service, WS_Client_Name, Project_Type, quoteType, WSurl, mtdQuoteNum, projAttribs);
                projectTrackingCode = entityIIDArray[2] + "_" + entityIIDArray[3];
                wsProjectID = Convert.ToString(entityIIDArray[3]);

                // Call WSScopeMonitor
                // WSScopeMonitor(entityIIDArray, WSUrl);

                string mtdQuoteMMYY = mtdQuoteNum.Substring(0, 4);
                string mtdQuoteSeqNum = mtdQuoteNum.Substring(4, mtdQuoteNum.Length - 4);
                quotePathPrefix = quotePathPrefix + mtdQuoteMMYY + "\\" + mtdQuoteSeqNum + "\\";

                //finally, build folder directory
                bdr = new BuildDir();
                quoteDir = bdr.buildQuoteDirectory(quotePathPrefix, yyyyMMDDHHMM, projName,XMLFileName);
                debugLine = DateTime.Now + " Built quote directory " + quoteDir; WriteDebugfile(debugLine);


//                WriteSendLog(service, user, m_project);
                create_m_Task(service,ContactIID,salesIID,m_project.Entity_IID,"Quote Request",srcLangCode.Internal_Description,transLangDesc,XMLfilesNameDesc);

                retVal = projectTrackingCode;
                msg.description = retVal;
                //send configured WS email
                NotifyWS nws = new NotifyWS();
                nws.SendMessage(msg.quoteOID, msg.description);
            }
            //single message

            //TODO The MTD project tracking code should also include the Quote and assessment section reference.
            debugLine = DateTime.Now + " Done - Tracking Code= " + projectTrackingCode; WriteDebugfile(debugLine);
            return retVal;
        } //AddUpdate

        [WebMethod]
        public string getStatus(string projectTrackerID, string locale, string MTD_GUID)
        {
            string userName = ConfigurationSettings.AppSettings["MTD_USR"];
            string pwd = ConfigurationSettings.AppSettings["MTD_PWD"];
            string WSuserName = ConfigurationSettings.AppSettings["WS_USR"];
            string WSpwd = ConfigurationSettings.AppSettings["WS_PWD"];

            if (MTD_GUID == "")
            {
                return ("GUID_NOT_FOUND");
            }

            if (projectTrackerID == "")
            {
                return ("TRACKER_NOT_FOUND");
            }

            UserSession us = new UserSession();
            User user = us.AuthenticateUser(userName, pwd);
            IMTDService service = us.MTDService;

            if (locale == "es_LA")
            {
                locale = "es_EC";
            }

            locale = locale.Replace('-', '_');

            Code mtdLocaleID = Code.FindAltType("LOC", locale);
            string debugLine = DateTime.Now + " Received getStatus call with : Tracker ID = " + projectTrackerID + ", Locale = " + locale + ", mtdLocaleID =  " + mtdLocaleID.External_Description + ", MTD_GUID = " + MTD_GUID; WriteDebugfile(debugLine);
            if (locale.Length > 5)
            {
                return "LOCALE_NOT_VALID";
            }

            locale = locale.Replace('_', '-');
            string localeName = getWSLangauge(locale);
            Guid mtdGUID = new Guid(MTD_GUID);  //msg parms "1EA7EDE9-AA8E-4384-AB73-C1E8BF9DE9AA"
            AppClass.WSParamCollection parms = service.LoadWSParamCollection(mtdGUID);
            string WSurl = getWSurl(parms, mtdGUID);
            string WS_Client_Name = getWSClient(parms, mtdGUID);
            Base.Text dTID = mtdLocaleID.Description_TID;
            int wslocaleID = Convert.ToInt32(dTID.Text_IID);
           // Guid APIKey = new Guid(API_KEY);  //msg parms "1EA7EDE9-AA8E-4384-AB73-C1E8BF9DE9AA"
            //find API_Key in WS_Track_Assets
            //AppClass.WSAssetTrackCollection wsTrackColl = service.LoadWSAssetTrackCollection(APIKey, projectTrackerID);
//            if (wsTrackColl == null)
//            { return "Error in API KEY or  ProjectTrackerID"; }

            string locales = null;
            //Get WS project id, right of '_' separator
            string[] ProjectID = projectTrackerID.Split('_');

            //Get WS project id, right of '_' separator
            int wsProjID = Convert.ToInt32(ProjectID[1]);

            WSContext ctx = new WSContext(WSuserName, WSpwd,WSurl); 
            WSWorkflowManager workflowMgr = ctx.getWorkflowManager();
            WSProjectGroup wsProjectGroup = workflowMgr.getProjectGroup(wsProjID);
            WSUserManager userManager = ctx.getUserManager();
            
            WSLocale wsloc = null;

           if (WS_Client_Name == "Seagate")
            {
                wsloc = userManager.getLocale2(GetSeagateLocID(wslocaleID));
            }
            else
            {
                wsloc = userManager.getLocale2(wslocaleID);
            }

            if (wsloc == null)
            {

                return "LOCALE_NOT_VALID";

            }
            else
            {
                int projStatus = 0;
                //Find the status of the project based on the locale
                WSProject[] wsProjects = null;
                if (wsProjectGroup == null){
                    return "Project does not Exist";
                } else {
                    wsProjects = wsProjectGroup.getProjects;
                }
                foreach (WSProject wsp in wsProjects) {
                    if ( wsp.getTargetLocale.getDisplayString.Equals(wsloc.getDisplayString)) {
                        projStatus = wsp.getStatus;
                        if (projStatus == 2)
                        {
                            int curstepcount = 0;
                            WSTask[] tasks = wsp.getActiveTasks();
                            foreach (WSTask task in tasks)
                            {
                                WSTaskStep currTaskStep = task.getCurrentTaskStep;
                                string curstep = currTaskStep.getDisplayString;
                                if (curstep.Contains("Review"))
                                {
                                    curstepcount++;
                                }
                            }
                            if (curstepcount == tasks.Length)
                            {
                                projStatus = 6;
                            }
                        }
                        break;
                    }
                }
                
                debugLine = DateTime.Now + " projStatus =" + projStatus; WriteDebugfile(debugLine);
                switch (projStatus)
                {
                    case 0:
                        {
                            return "STATUS_ANY";
                        }
                    case 1:
                        {
                            return "STATUS_NOT_STARTED";
                        }
                    case 2:
                        {
                            return "STATUS_ACTIVE";
                        }
                    case 3:
                        {
                            return "STATUS_COMPLETED";
                        }
                    case 4:
                        {
                            return "STATUS_PARTIALLY_COMPLETED";
                        }
                    case 5:
                        {
                            return "STATUS_CANCELED";
                        }
                    case 6:
                        {
                            return "STATUS_REVIEW";
                        }
                    default:
                        {
                            return "STATUS_INVALID";
                        }
                }
            }
            return locales;
        }

        [WebMethod]
        public string getAssets(string MTD_GUID, string projectTrackerID, string locale)
        {
            string tempDir = ConfigurationSettings.AppSettings["WEB_TEMP_PATH"];
            string webURL = ConfigurationSettings.AppSettings["WEB_URL"];
            string userName = ConfigurationSettings.AppSettings["MTD_USR"];
            string pwd = ConfigurationSettings.AppSettings["MTD_PWD"];
            string rootDir = ConfigurationSettings.AppSettings["MTD_PATH"];
            string wsRootDir = ConfigurationSettings.AppSettings["WSROOTPATH"];
            string bccAddress = "";
            string emailAddress = "";
            string salesPersonFirstName = "";
            string salesPersonLastName = "";
            string debugLine = DateTime.Now + " Received get asset request = MTD_GUID " + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale " + locale; WriteDebugfile(debugLine);

            if (MTD_GUID == "")
            {
                return ("GUID_NOT_FOUND");
            }

            if (projectTrackerID == "")
            {
                return ("TRACKER_NOT_FOUND");
            }

             
            WSScopeMgr scpMgr = new WSScopeMgr();
            UserSession us = new UserSession();
            User user = us.AuthenticateUser(userName, pwd);
            IMTDService service = us.MTDService;
            //int wsProjID = Convert.ToInt32(ProjectID[1]);

            Guid APIKey = new Guid(MTD_GUID);  //msg parms "1EA7EDE9-AA8E-4384-AB73-C1E8BF9DE9AA"
            //find API_Key in WS_Track_Assets
            AppClass.WSAssetTrackCollection wsTrackColl = service.LoadWSAssetTrackCollection(APIKey, projectTrackerID);
            if (wsTrackColl == null )
            { return "Error in ProjectTrackID"; }

            AppClass.WSParamCollection parms = service.LoadWSParamCollection(APIKey);
            string[] projectTrackerIDArray = projectTrackerID.Trim().Split('_');
            string mtdQuoteNum =  projectTrackerIDArray[0];
            string wsPrjGroupNum = projectTrackerIDArray[1];
            string wsClientName = getWSClient(parms, APIKey);
            if (wsClientName == "Seagate")
            {
                wsRootDir = ConfigurationSettings.AppSettings["WSROOTPATH2"];
            }
            string wsURL = getWSurl(parms, APIKey);
            string projectName = scpMgr.getWSProjName(wsURL, wsPrjGroupNum);
            string locale2 = "";
            if (projectName == "Project does not exist")
            {
                return "Project does not exist";
            }

            if (locale == "es_LA")
            {
                locale2 = "es_EC";
            } else
            {
                locale2 = locale;
            }
            debugLine = DateTime.Now + " Got Locale 2 = MTD_GUID " + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale2 " + locale2 + ", locale " + locale; WriteDebugfile(debugLine);

            locale = locale.Replace('-', '_');
            string wsLocale = getWSLocale(locale2);
            if (wsLocale == "No Locale found")
            {
                return "Locale was not found";
            }
            ProjectCollection m_projects = service.LoadProjectCollection(mtdQuoteNum);
            
            debugLine = DateTime.Now + " Got Project Collection = MTD_GUID " + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale " + locale; WriteDebugfile(debugLine);
            
            foreach (BaseExtendable project in m_projects)
            {
                string projOID = project.OID;

                if (projOID.Equals(mtdQuoteNum))
                {
                    int projectIID = Convert.ToInt32(project.Entity_IID);
                    m_project = new BaseExtendable(CodeTranslator.Find("ENTITY_TYPE", "PROJECT").Code_IID);
                    m_project.Entity_IID = projectIID;
                    m_project = (BaseExtendable)service.Load(m_project);
                    service.LoadAttributes(m_project);
                }
            }
            Base.Attribute projectAttr = m_project.Attributes.Find(Code.Find("PROJECT_ATTR", "PROJECTNUM"));
            string mtdProjectNum = (string)projectAttr.Value;

            string retValue = null;
            string wsCopyPath = wsRootDir + wsClientName + "\\Projects\\" + wsPrjGroupNum + "_" + projectName + "\\Target-" + wsLocale + " - UTF8\\";

            debugLine = DateTime.Now + " Got WS Path = MTD_GUID " + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale " + locale + "\n" + wsCopyPath; WriteDebugfile(debugLine);

            //Set up myML project path
            string mtdProjectYYMM = mtdProjectNum.Substring(0, 4);
            string mtdProjectSeqNum = mtdProjectNum.Substring(4, mtdProjectNum.Length - 4);
            string projRoot = rootDir + "Projects\\" + mtdProjectYYMM + "\\" + mtdProjectSeqNum + "\\";
            string myMLPath = projRoot + "4-MyMedialocate";

            WindowsImpersonationContext impersonationContext = null;
            if (MiscTools.impersonateValidUser(ref impersonationContext))
            {
                // Get WS file Array
                string myMLWSFilesPath = null;
                string[] wsFileEntries = Directory.GetFiles(wsCopyPath);
                debugLine = DateTime.Now + " Got WS files = MTD_GUID " + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale " + locale; WriteDebugfile(debugLine);

                // Recurse into subdirectories of this directory. 
                string[] subdirectoryEntries = Directory.GetDirectories(myMLPath);
                Boolean returnPackageExist = false;
                foreach (string subdirectory in subdirectoryEntries)
                {
                    if (subdirectory.Contains(wsPrjGroupNum))
                    {
                        string[] subPathArray = subdirectory.Split('\\');
                        string mySubdirectory = subPathArray[subPathArray.Length - 1];
                        myMLWSFilesPath = myMLPath + "\\" + mySubdirectory + "\\";
                        debugLine = DateTime.Now + " Got myMLWSFilesPath = " + myMLWSFilesPath + ", MTD_GUID " + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale " + locale; WriteDebugfile(debugLine);
                        returnPackageExist = true;
                    }
                }

                if (returnPackageExist == false)
                {
                    return "Tracking_Code does not match available projects";
                }

                //Copy files to shorter path
                string myLocalePath = myMLWSFilesPath + locale.Trim() + "\\";
//               debugLine = DateTime.Now + " Got myML Path = " + myLocalePath + ", MTD_GUID " + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale " + locale; WriteDebugfile(debugLine);

                //Copy all the files & Replaces any files with the same name
                string zipName = null;
                string dateTimeNow = DateTime.Now.ToString("MMddyyyyHHmmss");
                zipName = projectTrackerID + "_" + locale + "_" + dateTimeNow + ".zip";
                string shortZipDir = "c:\\z\\" + dateTimeNow + "\\";

                if (!Directory.Exists(shortZipDir))
                {
                    Directory.CreateDirectory(shortZipDir);
                }

                string[] fileEntries = Delimon.Win32.IO.Directory.GetFiles(wsCopyPath);
                string[] directoryEntries = Delimon.Win32.IO.Directory.GetDirectories(wsCopyPath);

                if (fileEntries == null || directoryEntries == null)
                { return "No assets to return"; }

                if (fileEntries.Length > 0 || directoryEntries.Length > 0)
                {
                    retValue = webURL + zipName;
//                    debugLine = DateTime.Now + " Zipped the files = " + retValue + ", MTD_GUID" + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale" + locale; WriteDebugfile(debugLine);
                }
                else
                { return "No files found"; }

                foreach (string newPath in Delimon.Win32.IO.Directory.GetFiles(wsCopyPath, "*.*", Delimon.Win32.IO.SearchOption.AllDirectories))
                {
                    string fileName = Delimon.Win32.IO.Path.GetFileName(newPath);
                    string sourceDir = newPath.Substring(0, newPath.Length - fileName.Length).Replace(wsCopyPath, shortZipDir);
                    if (!Directory.Exists(sourceDir))
                    {
                        Directory.CreateDirectory(sourceDir);
                    }
                    Delimon.Win32.IO.File.Copy(newPath, newPath.Replace(wsCopyPath, shortZipDir), true);
                }

                while (!File.Exists(tempDir + "\\" + zipName))
                {
                    ZipFile.CreateFromDirectory(shortZipDir, tempDir + "\\" + zipName, CompressionLevel.Fastest, false, new MyEncoder());
                    if (File.Exists(tempDir + "\\" + zipName))
                    {
                        File.Copy(tempDir + "\\" + zipName, myLocalePath + "\\" + zipName);
                        Directory.Delete(shortZipDir, true);
                    }
                }

                debugLine = DateTime.Now + " Copied files over to P: drive =" + myLocalePath + ", MTD_GUID " + MTD_GUID + ", projectTrackerID " + projectTrackerID + ", locale " + locale; WriteDebugfile(debugLine);

                MiscTools.undoImpersonation(impersonationContext);
            }
            else
            { return "Could not access directory";}

            Code quoteType = Code.Find("ENTITY_TYPE","QUOTE");
            EntityRelationCollection quoteRelations = service.LoadRelatedQuotes(m_project);
            string m_quoteDesc = "";
            foreach (EntityRelation relation in quoteRelations)
            {
                BaseExtendable quote = relation.Entity_2;
                service.LoadAttributes(quote);
                m_quoteDesc = quote.Description;
            }
            Base.Attribute salesAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "SALESPERSON"));
            if (salesAttr != null && salesAttr.Value != null)
            {
                AppClass.Employee sales = service.LoadEmployee((int)salesAttr.Value);
                salesPersonFirstName = sales.First_Name;
                salesPersonLastName = sales.Last_Name;
                bccAddress = sales.Email_Address;
            }
            Base.Attribute contactAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "CONTACT"));
            if (contactAttr != null)
            {
                Contact targetContact = new Contact((int)contactAttr.Value);
                targetContact = (Contact)service.Load(targetContact);
                CustomerSite targetSite = service.LoadCustomerSite(targetContact.Site_IID);
                emailAddress = targetContact.Email_Address;
            }
            string mailSubject = "Files Succesfully Uploaded";
            string mailMessage = " The Project named " + m_quoteDesc + " has been completed and the files uploded to your CMS. If if you need further assistance, please contact your Medialocate Sales Representative, " + salesPersonFirstName + " " + salesPersonLastName + ", or call our Toll-free line: 1-800-776-0857. <br /> The Medialocate Team";
            EmailUser(mailSubject, mailMessage, emailAddress, bccAddress);
            return retValue;
        }


        [WebMethod]
        public string cancelProjRequest(string MTD_GUID, string projectTrackerID)
        {
            Boolean wsRetValue = false;
            Boolean mtdRetValue = false;
            string userName = ConfigurationSettings.AppSettings["MTD_USR"];
            string pwd = ConfigurationSettings.AppSettings["MTD_PWD"];
            string WSuserName = ConfigurationSettings.AppSettings["WS_USR"];
            string WSpwd = ConfigurationSettings.AppSettings["WS_PWD"];
            string bccAddress = "";
            string emailAddress = "";
            string salesPersonFirstName = "";
            string salesPersonLastName = "";
            string debugLine = DateTime.Now + " Recieved Cancel Request for Project " + projectTrackerID; WriteDebugfile(debugLine);

            UserSession us = new UserSession();
            User user = us.AuthenticateUser(userName, pwd);
            IMTDService service = us.MTDService;

            if (MTD_GUID == "")
            {
                return ("GUID_NOT_FOUND");
            }

            if (projectTrackerID == "")
            { 
                return ("TRACKER_NOT_FOUND");
            }
            
            Guid mtdGUID = new Guid(MTD_GUID);  //msg parms "1EA7EDE9-AA8E-4384-AB73-C1E8BF9DE9AA"
            AppClass.WSParamCollection parms = service.LoadWSParamCollection(mtdGUID);
            string WSurl = getWSurl(parms, mtdGUID);

            AppClass.WSAssetTrackCollection wsTrackColl = service.LoadWSAssetTrackCollection(mtdGUID, projectTrackerID);
            if (wsTrackColl == null)
            { 
                return "TRACKER_NOT_FOUND"; 
            }
            
            string[] ProjectID = projectTrackerID.Split('_');

            //Get WS project id, right of '_' separator
            string mtdQuoteNum = ProjectID[0].Trim();
            int wsProjGroupNum = Convert.ToInt32(ProjectID[1].Trim());

            WSContext ctx = new WSContext(WSuserName, WSpwd, WSurl);
            WSWorkflowManager workflowMgr = ctx.getWorkflowManager();
            WSProjectGroup wsProjectGroup = workflowMgr.getProjectGroup(wsProjGroupNum);
            string projDesc = "";
            ProjectCollection m_projects = service.LoadProjectCollection(mtdQuoteNum);
            foreach (BaseExtendable mtd_project in m_projects)
            {
                if (mtd_project.OID == mtdQuoteNum)
                {
                    int projectIID = Convert.ToInt32(mtd_project.Entity_IID);
                    m_project = new BaseExtendable(CodeTranslator.Find("ENTITY_TYPE", "PROJECT").Code_IID);
                    m_project.Entity_IID = projectIID;
                    m_project = (BaseExtendable)service.Load(m_project);
                    service.LoadAttributes(m_project);
                }
            }

            if (wsProjectGroup != null && m_project !=null)
            {
                WSProject[] projects = wsProjectGroup.getProjects;
                projDesc = wsProjectGroup.getDisplayString;

                //Get teh lsit of Projects or Languages in this Group
                foreach (WSProject project in projects)
                {
                    project.cancel("Cancelled by MTD");
                    wsRetValue = true;
                }

                debugLine = DateTime.Now + " Canceled WS Project " + wsProjGroupNum + " " + wsRetValue; WriteDebugfile(debugLine);
/*                
                ProjectCollection m_projects = service.LoadProjectCollection(mtdQuoteNum);
                foreach (BaseExtendable mtd_project in m_projects)
                {
                    if (mtd_project.OID == mtdQuoteNum)
                    {
                        int projectIID = Convert.ToInt32(mtd_project.Entity_IID);
                        m_project = new BaseExtendable(CodeTranslator.Find("ENTITY_TYPE", "PROJECT").Code_IID);
                        m_project.Entity_IID = projectIID;
                        m_project = (BaseExtendable)service.Load(m_project);
                        service.LoadAttributes(m_project);
                    }
                }
*/
                Base.Attribute isProjectApproved = m_project.Attributes.Find(CodeTranslator.Find("", ""));

                Base.Attribute salesAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "SALESPERSON"));
                if (salesAttr != null && salesAttr.Value != null)
                {
                    AppClass.Employee sales = service.LoadEmployee((int)salesAttr.Value);
                    salesPersonFirstName = sales.First_Name;
                    salesPersonLastName = sales.Last_Name;
                    bccAddress = sales.Email_Address;
                }
                Base.Attribute contactAttr = m_project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "CONTACT"));
                if (contactAttr != null)
                {
                    Contact targetContact = new Contact((int)contactAttr.Value);
                    targetContact = (Contact)service.Load(targetContact);
                    CustomerSite targetSite = service.LoadCustomerSite(targetContact.Site_IID);

                    emailAddress = targetContact.Email_Address;
                }
                Code quoteType = Code.Find("ENTITY_TYPE", "QUOTE");
                EntityRelationCollection quoteRelations = service.LoadRelatedQuotes(m_project);
                string m_quoteDesc = "";
                foreach (EntityRelation relation in quoteRelations)
                {
                    BaseExtendable quote = relation.Entity_2;
                    service.LoadAttributes(quote);

                    m_quoteDesc = quote.Description;
                    if (m_quoteDesc.Contains(projDesc))
                    {
                        //mark quote item rejected.
                        quote.Inactive = true;
                        mtdRetValue = true;
                    }
                }
                debugLine = DateTime.Now + " Canceled MTD Project " + mtdQuoteNum + " " + mtdRetValue; WriteDebugfile(debugLine);
                if (wsRetValue && mtdRetValue)
                {

                    string mailSubject = "Estimate Succesfully Cancelled";
                    string mailMessage = " The Estimate named " + m_quoteDesc + " has been successfully cancelled. If you need further assistance, please contact your Medialocate Sales Representative, " + salesPersonFirstName + " " + salesPersonLastName + ", or call our Toll-free line: 1-800-776-0857. <br /> The Medialocate Team";
                    EmailUser(mailSubject, mailMessage, emailAddress, bccAddress);
                    debugLine = DateTime.Now + " Cancel Request for Project " + m_quoteDesc + " (" + projectTrackerID + ") succeded."; WriteDebugfile(debugLine);
                    return "SUCCESS";

                }
                else if ((wsRetValue && !mtdRetValue) || (!wsRetValue && mtdRetValue))
                {
                    string mailSubject = "Estimate Failed to Cancel";
                    string mailMessage = " The Estimate " + m_quoteDesc + " with number " + projectTrackerID + " could not be completely cancelled. Your Medialocate Sales Representative, " + salesPersonFirstName + " " + salesPersonLastName + ", has been contacted. If if you need further assistance, please contact your Medialocate Sales Representative or call our Toll-free line: 1-800-776-0857. <br />The Medialocate Team";
                    EmailUser(mailSubject, mailMessage, emailAddress, bccAddress);
                    debugLine = DateTime.Now + " Cancel Request for Project " + m_quoteDesc + " (" + projectTrackerID + ") failed"; WriteDebugfile(debugLine);
                    return "PARTIAL_FAILED";
                }
            }
            else
            {
                string mailSubject = "Estimate Failed to Cancel";
                string mailMessage = " The Estimate number " + projectTrackerID + " could not be cancelled. Your Medialocate Sales Representative, " + salesPersonFirstName + " " + salesPersonLastName + ", has been contacted. If if you need further assistance, please contact your Medialocate Sales Representative or call our Toll-free line: 1-800-776-0857. <br />The Medialocate Team";
                EmailUser(mailSubject, mailMessage, emailAddress, bccAddress);
                debugLine = DateTime.Now + " Could not Cancel Request for Project " + projectTrackerID + " failed to find project in WS"; WriteDebugfile(debugLine);
                return "FAILED";

            }
            return "FAILED";
        }

        private void create_m_Task(IMTDService service, int ContactIID, int EmployeeIID, int quote_Entity_IID, string requestType, string sourceLang, string targetLangs, string filesToTranslate)
        {
            User contactUser = service.LoadUser(Code.Find("USER_TYPE", "CONTACT"), ContactIID);
            User employeeUser = service.LoadUser(Code.Find("USER_TYPE", "EMPLOYEE"), EmployeeIID);

            Code qType = Code.Find("TASK_TYPE", "CUST_QUOTE_REQ");
            int Current_User_Role_IID = -1;
            int Pending_User_Role_IID = -1;

            Task newTask = new Task();
            newTask.Task_CID = qType;

            UserRoleCollection roles = service.LoadUserRoles(contactUser);
            if (roles.Count > 0)
            {
                Current_User_Role_IID = ((UserRole)roles[0]).User_Role_IID;
            }
            UserRoleCollection rolesPending = service.LoadUserRoles(employeeUser);
            if (rolesPending.Count > 0)
            {
                Pending_User_Role_IID = ((UserRole)rolesPending[0]).User_Role_IID;
            }
            

            newTask.Created_User_Role_IID = Current_User_Role_IID;
            newTask.When_Required_DT = (DateTime)DateTime.Now;

            newTask.Pending_User_Role_IID = Pending_User_Role_IID;
            newTask.Current_User_Role_IID = Current_User_Role_IID;
            service.Store(newTask);


            Base.Attribute projectAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "PROJECT"), quote_Entity_IID);
            projectAttr.SetParent(newTask.Attributes);

            Base.Attribute priorityAttr = newTask.Attributes.Add(Code.Find("CTYPE", "TASK_PRIORITY"), Code.Find("TASK_PRIORITY","NORMAL"));
            priorityAttr.SetParent(newTask.Attributes);

            Base.Attribute reqTypeAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "REQ_TYPE"), requestType);
            reqTypeAttr.SetParent(newTask.Attributes);

            Base.Attribute poAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "PO_NUMBER"), "");
            poAttr.SetParent(newTask.Attributes);

            Base.Attribute srcAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "SOURCE_LANG"), sourceLang);
            srcAttr.SetParent(newTask.Attributes);

            Base.Attribute targetAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "TARGET_LANGS"), targetLangs);
            targetAttr.SetParent(newTask.Attributes);

            Base.Attribute filesAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "FILES_PROCESSED_LIST"), filesToTranslate);
            filesAttr.SetParent(newTask.Attributes);

            Base.Attribute otherLangAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "OTHER_LANGUAGE"), "");
            otherLangAttr.SetParent(newTask.Attributes);

            Base.Attribute reqServeLangAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "REQ_SERVICES"), "Translation Request from CMS" );
            reqServeLangAttr.SetParent(newTask.Attributes);
            //----------------

//            Base.Attribute taskmonthlyAttr = newTask.Attributes.Add(Code.Find("TASK_ATTR", "MONTHLY"), Convert.ToBoolean(Convert.ToInt32(MonthlyButtonList.SelectedValue)));
//            taskmonthlyAttr.SetParent(newTask.Attributes);

            Base.Attribute attr = newTask.Attributes.Add(CodeTranslator.Find("TASK_ATTR", "SPEC_INST"), "");
            attr.SetParent(newTask.Attributes);
            

            service.Store(newTask);
            newTask = service.LoadTask(newTask.Task_IID); // Load to get latest state!

            MiscTools.EmailTask(newTask, newTask.Pending_User_Role_IID, false, service);


        }
        public void EmailUser(string mailSubject, string mailMessage, string emailAddress,string bccAddress)
        {
            string mailBCC = null;

            string mailFrom = "autoresponse@medialocate.com";
            if (ConfigurationSettings.AppSettings["FromEmail"] != null)
            {
                mailFrom = ConfigurationSettings.AppSettings["FromEmail"];
            }

            if (ConfigurationSettings.AppSettings["BCCEmail"] != null || bccAddress.Equals(""))
            {
                mailBCC = ConfigurationSettings.AppSettings["BCCEmail"];
            }

            string mailTo = emailAddress;

            try
            {
                // Use MailMessage to build the outgoing message
                MailMessage message = new MailMessage(mailFrom, mailTo, mailSubject, mailMessage);
                {
                    message.IsBodyHtml = true;

                    if (mailBCC != null)
                        message.Bcc.Add(mailBCC);

                    // Send delivers the message to the mail server
                    AppClass.MTDEmail.SendSmtpMail(message);
                }
            }
            catch (Exception)
            {
            }
        }

        [WebMethod]
        public string Add_Update_ContentZip(
            Int64 seqNo,
            string MTD_GUID,
            string contentType,
            //            string UID,
            string source_lang,
            string target_langs,
            //            string client_id,
            string date,
            string projName,
            string projDescription,
            string header,
            string XMLMsg,
            byte[] zipFile
            )
        {
            string zipfilePath = GetZipFilePath(zipFile);
            string debugLine = DateTime.Now + " MTD_GUID: " + MTD_GUID + ", contentType: " + contentType + ", source_lang: " + source_lang + ", target_langs: " + target_langs + ", Date: " + date + ", projName: " + projName + ", projDescription: " + projDescription + ", header: " + header + ", XMLMsg: " + XMLMsg + ", zipfilePath = " + zipfilePath; WriteDebugfile(debugLine);
            debugLine = DateTime.Now + "  "; WriteDebugfile(debugLine);
            return Add_Update_Content(seqNo, MTD_GUID, contentType, source_lang, target_langs,
                                        date, projName, projDescription, header, XMLMsg, zipfilePath);

        }


        private string GetZipFilePath(byte[] zipFile)
        {
            string debugLine = DateTime.Now + " GetZipFilePath processing  "; WriteDebugfile(debugLine);
            try
            {
                string DateTimeNow =  DateTime.Now.ToString("yyyyMMddHHmmssff");
                string path = ConfigurationSettings.AppSettings["TEMPFOLDER"] + "\\Zip\\" + DateTimeNow + "\\"; 
                string zipFileName = DateTimeNow + ".zip";
                string filePath = path + zipFileName;
                debugLine = DateTime.Now + " filePath = " + filePath; WriteDebugfile(debugLine);
                debugLine = DateTime.Now + " zipFile.Length  " + zipFile.Length; WriteDebugfile(debugLine);
                //using (
                WindowsImpersonationContext impersonationContext = null;
                if (MiscTools.impersonateValidUser(ref impersonationContext))
                {
                    if (!Directory.Exists(path)) {
                        Directory.CreateDirectory(path);
                    }

                    using (Stream file = File.OpenWrite(filePath))
                    {
                                file.Write(zipFile, 0, zipFile.Length);
                    }    
                }
                debugLine = DateTime.Now + " Zip Path = " + filePath; WriteDebugfile(debugLine);
                return filePath;
            }
            catch (Exception)
            {
                return null;
            }
            
        }
/*
        private string createZipFile(string path, string fileName, string WSProjectID, string locale)
        {
            ZipFile zip = new ZipFile();
            string[] subdirs = Directory.GetDirectories(path);
            foreach (string sub in subdirs)
            {
                string[] sub2 = Directory.GetDirectories(sub);
                foreach (string sub3 in sub2)
                {

                    string proj = sub3.Substring( sub3.Length - 4);
                    if (proj == WSProjectID)
                    {
                        string[] sub4 = Directory.GetDirectories(sub3);
                        foreach (string sub5 in sub4)
                        {
                            string loc = sub5.Substring(sub5.Length - locale.Length);
                            if (loc == locale)
                            {
                                string[] files = Directory.GetFiles(sub5);
                                foreach (string file in files)
                                {
                                    //check for ProjectID and locale in filepath
                                    string[] folders = file.Split('\\');
                                    zip.AddFile(file);
                                }
                            }
                        }
                    }//endif
                }
            }
            zip.Save(fileName);

            return fileName;
        }
  */
        private List<string> UnpackZipFile(string fileName)
        {
            //string[] XMLfiles = new string[10];
            String debugLine = DateTime.Now + " UnpackZipFile processing "; WriteDebugfile(debugLine);
            var ListOfFiles = new List<string>();
            string extractPath = ConfigurationSettings.AppSettings["TEMPFOLDER"] + "\\Zip\\" +  DateTime.Now.ToString("yyyyMMddHHmmssff");
            string unpackPath = extractPath + "\\Extract" ;
            debugLine = DateTime.Now + " UnpackZipFile processing 2 "; WriteDebugfile(debugLine);
            using (Ionic.Zip.ZipFile zip1 = Ionic.Zip.ZipFile.Read(fileName))
            {
                int fileCount = zip1.Count;
                int idx = 0;
                foreach (Ionic.Zip.ZipEntry entry in zip1)
                {
                    entry.Extract(unpackPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                    //XMLfiles[idx] = unpackPath + "\\" + entry.FileName;
                    ListOfFiles.Add( unpackPath + "\\" + entry.FileName);
                    idx ++;
                }
            }
            //XMLfiles = listOfFiles.ToArray();
            return ListOfFiles;
        }

        private string getWSLangauge(string ISOlocale)
        {
        
            switch(ISOlocale)
            {
                case "ar-SA":
                    {
                        return "Arabic (Saudi Arabia)";
                    }

                case "khm-KH":
                    {
                        return "Khmer";
                    }

                case "zh-CN":
                    {
                        return "Chinese (PRC)";
                    }

                case "zh-TW":
                    {
                        return "Chinese (Taiwan)";
                    }

                case "cs-CZ":
                    {
                        return "Czech";
                    }

                case "da-DK":
                    {
                        return "Danish";
                    }

                case "nl-NL":
                    {
                        return "Dutch (Netherlands)";
                    }

                case "nl-BE":
                    {
                        return "Dutch (Belgium)";
                    }

                case "en-AU":
                    {
                        return "English (Australia)";
                    }

                case "en-GB":
                    {
                        return "English (United Kingdom)";
                    }

                case "en-US":
                    {
                        return "English (United States)";
                    }

                case "en-CA":
                    {
                        return "English (Canada)";
                    }

                case "fi-FI":
                    {
                        return "Finnish";
                    }

                case "fr-BE":
                    {
                        return "French (Belgium)";
                    }

                case "fr-CA":
                    {
                        return "French (Canada)";
                    }

                case "fr-FR":
                    {
                        return "French (France)";
                    }

                case "de-DE":
                    {
                        return "German (Germany)";
                    }

                case "el-GR":
                    {
                        return "Greek";
                    }

                case "id-ID":
                    {
                        return "Indonesian";
                    }

                case "it-IT":
                    {
                        return "Italian (Italy)";
                    }

                case "ja-JP":
                    {
                        return "Japanese";
                    }

                case "ko-KR":
                    {
                        return "Korean";
                    }

                case "nb-NO":
                    {
                        return "Norwegian";
                    }

                case "no-NB":
                    {
                        return "Norwegian";
                    }

                case "pl-PL":
                    {
                        return "Polish";
                    }

                case "pt-BR":
                    {
                        return "Portuguese (Brazil)";
                    }

                case "pt-PT":
                    {
                        return "Portuguese (Portugal)";
                    }

                case "ro-RO":
                    {
                        return "Romanian";
                    }

                case "ru-RU":
                    {
                        return "Russian";
                    }

                case "es-EC":
                    {
                        return "Spanish (Latin America)";
                    }

                case "es-ES":
                    {
                        return "Spanish (Spain)";
                    }

                case "sv-SE":
                    {
                        return "Swedish";
                    }

                case "th-TH":
                    {
                        return "Thai";
                    }

                case "tr-TR":
                    {
                        return "Turkish";
                    }

                case "vi-VN":
                    {
                        return "Vietnamese";
                    }

                default:
                    {
                        return "No Language found";
                    }

            }

        }

        private string getWSLocale(string ISOlocale)
        {
            ISOlocale = ISOlocale.Replace("_", "-");
            switch (ISOlocale)
            {
                case "ar-SA":
                    {
                        return "Arabic (Saudi Arabia)";
                    }

                case "khm-KH":
                    {
                        return "Khmer";
                    }

                case "zh-CN":
                    {
                        return "Chinese (Simplified)";
                    }

                case "zh-TW":
                    {
                        return "Chinese (Traditional)";
                    }

                case "cs-CZ":
                    {
                        return "Czech";
                    }

                case "da-DK":
                    {
                        return "Danish";
                    }

                case "nl-NL":
                    {
                        return "Dutch (Netherlands)";
                    }

                case "nl-BE":
                    {
                        return "Dutch (Belgium)";
                    }

                case "en-AU":
                    {
                        return "English (Australia)";
                    }

                case "en-GB":
                    {
                        return "English (United Kingdom)";
                    }

                case "en-US":
                    {
                        return "English (United States)";
                    }

                case "en-CA":
                    {
                        return "English (Canada)";
                    }

                case "fi-FI":
                    {
                        return "Finnish";
                    }

                case "fr-BE":
                    {
                        return "French (Belgium)";
                    }

                case "fr-CA":
                    {
                        return "French (Canada)";
                    }

                case "fr-FR":
                    {
                        return "French (France)";
                    }

                case "de-DE":
                    {
                        return "German (Germany)";
                    }

                case "el-GR":
                    {
                        return "Greek";
                    }

                case "id-ID":
                    {
                        return "Indonesian";
                    }

                case "it-IT":
                    {
                        return "Italian (Italy)";
                    }

                case "ja-JP":
                    {
                        return "Japanese";
                    }

                case "ko-KR":
                    {
                        return "Korean";
                    }

                case "nb-NO":
                    {
                        return "Norwegian";
                    }

                case "no-NB":
                    {
                        return "Norwegian";
                    }

                case "pl-PL":
                    {
                        return "Polish";
                    }

                case "pt-BR":
                    {
                        return "Portuguese (Brazil)";
                    }

                case "pt-PT":
                    {
                        return "Portuguese (Portugal)";
                    }

                case "ro-RO":
                    {
                        return "Romanian";
                    }

                case "ru-RU":
                    {
                        return "Russian";
                    }

                case "es-EC":
                    {
                        return "Spanish (Latin America)";
                    }

                case "es-ES":
                    {
                        return "Spanish (Spain)";
                    }

                case "sv-SE":
                    {
                        return "Swedish";
                    }

                case "th-TH":
                    {
                        return "Thai";
                    }

                case "tr-TR":
                    {
                        return "Turkish";
                    }

                case "vi-VN":
                    {
                        return "Vietnamese";
                    }

                default:
                    {
                        return "No Locale found";
                    }

            }

        }

        private int GetSeagateLocID(int ID)
        {
            switch (ID)
            {
                //Arabic (Egypt) - UTF8
                case 1918:
                    {
                        return 1033;
                    }
                //Arabic (Saudi Arabia) - UTF8
                case 1871:
                    {
                        return 1129;
                    }
                //Arabic (U.A.E) - UTF8
                case 1921:
                    {
                        return 1273;
                    }
                //Chinese (Hong Kong) - UTF8
                case 1865:
                    {
                        return 1286;
                    }
                //Chinese (Simplified) - UTF8
                case 1289:
                    {
                        return 1051;
                    }
                //Chinese (Traditional) - UTF8
                case 1338:
                    {
                        return 1071;
                    }
                //Czech - UTF8
                case 1925:
                    {
                        return 1034;
                    }
                //Danish - UTF8
                case 1926:
                    {
                        return 1274;
                    }
                //Dutch (Belgium) - UTF8
                case 1928:
                    {
                        return 1241;
                    }
                //Dutch (Netherlands) - UTF8
                case 1769:
                    {
                        return 1083;
                    }
                //English (Asia) - UTF8
                case 5898:
                    {
                        return 1337;
                    }
                //English (Australia) - UTF8
                case 2169:
                    {
                        return 1080;
                    }
                //English (Belize) - UTF8
                case 5899:
                    {
                        return 1325;
                    }
                //English (Canada) - UTF8
                case 2364:
                    {
                        return 1193;
                    }
                //English (Europe) - UTF8
                case 5897:
                    {
                        return 1324;
                    }
                //English (Indonesia) - UTF8
                case 5900:
                    {
                        return 1321;
                    }
                //English (Singapore) - UTF8
                case 2850:
                    {
                        return 1322;
                    }
                //English (United Kingdom) - UTF8
                case 1360:
                    {
                        return 1066;
                    }
                //English (United States) - UTF8
                case 1165:
                    {
                        return 1018;
                    }
                //Estonian - UTF8
                case 1929:
                    {
                        return 1276;
                    }
                //Finnish - UTF8
                case 1931:
                    {
                        return 1277;
                    }
                //French (Belgium) - UTF8
                case 2458:
                    {
                        return 1323;
                    }
                //French (Canada) - UTF8
                case 1370:
                    {
                        return 1078;
                    }
                //French (France) - UTF8
                case 1361:
                    {
                        return 1019;
                    }
                //German (Germany) - UTF8
                case 1362:
                    {
                        return 1020;
                    }
                //Greek - UTF8
                case 1933:
                    {
                        return 1097;
                    }
                //Hindi - UTF8
                case 1945:
                    {
                        return 1305;
                    }
                //Hungarian - UTF8
                case 1946:
                    {
                        return 1279;
                    }
                //Indonesian - UTF8
                case 1948:
                    {
                        return 1161;
                    }
                //Italian (Italy) - UTF8
                case 1363:
                    {
                        return 1067;
                    }
                //Japanese - UTF8
                case 1340:
                    {
                        return 1073;
                    }
                //Korean - UTF8
                case 1339:
                    {
                        return 1072;
                    }
                //Latvian - UTF8
                case 1949:
                    {
                        return 1281;
                    }
                //Lithuanian - UTF8
                case 1950:
                    {
                        return 1280;
                    }
                //Malay (Malaysia) - UTF8
                case 5385:
                    {
                        return 1177;
                    }
                //Norwegian - UTF8
                case 1952:
                    {
                        return 1282;
                    }
                //Polish - UTF8
                case 1364:
                    {
                        return 1035;
                    }
                //Portuguese - UTF8
                case 5901:
                    {
                        return 1283;
                    }
                //Portuguese (Brazil) - UTF8
                case 1365:
                    {
                        return 1075;
                    }
                //Portuguese (Portugal) - UTF8
                case 1771:
                    {
                        return 1084;
                    }
                //Romanian - UTF8
                case 1953:
                    {
                        return 1036;
                    }
                //Russian - UTF8
                case 1366:
                    {
                        return 1068;
                    }
                //Slovak - UTF8
                case 1954:
                    {
                        return 1284;
                    }
                //Slovenian - UTF8
                case 1955:
                    {
                        return 1285;
                    }
                //Spanish (Latin America) - UTF8
                case 1852:
                    {
                        return 1074;
                    }
                //Spanish (Mexico) - UTF8
                case 1772:
                    {
                        return 1275;
                    }
                //Spanish (Spain) - UTF8
                case 1368:
                    {
                        return 1039;
                    }
                //Swedish - UTF8
                case 1625:
                    {
                        return 1145;
                    }
                //Thai - UTF8
                case 1770:
                    {
                        return 1085;
                    }
                //Turkish - UTF8
                case 1369:
                    {
                        return 1069;
                    }
                //Ukrainian - UTF8
                case 1957:
                    {
                        return 1369;
                    }
                //Vietnamese - UTF8
                case 1707:
                    {
                        return 1086;
                    }

                default:
                    {
                        return 0;
                    }
            }
        }



        private void ReadInputMsg(TMAMessage msg, string XMLMsg, string UID)
        {
            XmlReader reader = XmlReader.Create(new StringReader(XMLMsg));
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;

            while (reader.Read())
            {
                // 
                if (reader.NodeType == XmlNodeType.Element)
                    if (reader.Name == "VignVCMId")
                    {
                        UID = reader.ReadString().ToString();
                    }
                if (reader.Name == "VignStatus")
                {
                    msg.status = reader.ReadString().ToString();
                }
                if (reader.Name == "VignLogicalPath")
                {
                    msg.logicalPath = reader.ReadString().ToString();
                }

                if (reader.Name == "VignName")
                {
                    msg.vgnName = reader.ReadString().ToString();
                }
                if (reader.Name == "GUID")
                {
                    msg.GUID = reader.ReadString().ToString();
                }
                if (reader.Name == "LANGUAGE")
                {
                    msg.language = reader.ReadString().ToString();
                }
                if (reader.Name == "GEOGRAPHY")
                {
                    msg.geography = reader.ReadString().ToString();
                }
                if (reader.Name == "NAME")
                {
                    msg.name = reader.ReadString().ToString();
                }
                if (reader.Name == "PARENT-UID")
                {
                    msg.parentGUID = reader.ReadString().ToString();
                }
                if (reader.Name == "MASTER-UID")
                {
                    msg.masterGUID = reader.ReadString().ToString();
                }
                if (reader.Name == "TEASER")
                {
                    msg.teaser = reader.ReadString().ToString();
                }

                if (reader.Name == "INTERNAL-NAME")
                {
                    msg.internalName = reader.ReadString().ToString();
                }

                if (reader.Name == "EXTERNAL-URL")
                {
                    msg.externalURL = reader.ReadString().ToString();
                }
                if (reader.Name == "DESCRIPTION-SHORT")
                {
                    msg.description = reader.ReadString().ToString();
                }

            }//while
         return;
        
        }

        // get WS Client name from MDL database
        private string getWSClient(WSParamCollection parms, Guid mtdGUID)
        {
            string WSClient = "";
            foreach (WSParam parm in parms)
            {
                if (parm.MTD_GUID == mtdGUID)
                {
                    WSClient = parm.WS_Client_Name;
                }
            }

            return WSClient;

        }
        // get WS Project type from MDL database
        private string getWSProjectType(WSParamCollection parms, Guid mtdGUID)
        {
            string ProjType = "";
            foreach (WSParam parm in parms)
            {
                if (parm.MTD_GUID == mtdGUID)
                {
                    ProjType = parm.Project_Type;
                }
            }
            return ProjType;
        }

        private string getModeCID(WSParamCollection parms, Guid mtdGUID)
        {
            Base.Code modeCID;
            string ModeType = "";
            foreach (WSParam parm in parms)
            {
                if (parm.MTD_GUID == mtdGUID)
                {
                    modeCID = parm.Mode_CID;
                    ModeType = modeCID.Code_Value;
                }
            }
            return ModeType;
        }

        private string getMTDProjID(WSParamCollection parms, Guid mtdGUID)
        {
            string monthlyProjID = "";
            foreach (WSParam parm in parms)
            {
                if (parm.MTD_GUID == mtdGUID)
                {
                    monthlyProjID = parm.MTD_ProjectID;
                }
            }
            return monthlyProjID;
        
        }

        private string getMTDProjNum(String monthlyProjID)
        {
            string monthlyProjNum = "";
            string[] projectID = monthlyProjID.Split('_');
            monthlyProjNum = projectID[1].Substring(0, 4);
            return monthlyProjNum;

        }

        private string getWSurl(WSParamCollection parms, Guid mtdGUID)
        {
            string WSurl= "";
            foreach (WSParam parm in parms)
            {
                if (parm.MTD_GUID == mtdGUID)
                {
                    WSurl = parm.WS_URL;
                }
            }
            return WSurl;

        }

        private string XMLMsgDecode(string XMLMsg)
        {
            string XMLheader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
            string pattern = @"\?xml version=";
            Regex XMLMsgMatch = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = XMLMsgMatch.Match(XMLMsg);
            if (!match.Success)
            {
                XMLMsg = XMLheader + XMLMsg;
            }
            XMLMsg = XMLMsg.Replace("&lt;","<");
            XMLMsg = XMLMsg.Replace("&gt;",">");
            XMLMsg = XMLMsg.Replace("&quot;","\"");
            return XMLMsg;
        }
        private bool doesMonthlyProjExist(IMTDService service, WSParamCollection parms, Guid mtdGUID, int ContactIID, string monthYear)
        {
            // Making sure the stored Project # is for the current month
            bool returnValue = false;
            ProjectCollection projects = service.LoadProjectCollection(ContactIID);
            foreach (BaseExtendable project in projects)
            {
                service.LoadAttributes(project);
                Base.Attribute monthlyAttr = project.Attributes.Find(CodeTranslator.Find("PROJECT_ATTR", "MONTHLY"));
                if (monthlyAttr != null)
                    {
                    bool monthlySW = Convert.ToBoolean(monthlyAttr.Value);
                    string projMonthYear = project.OID.Substring(0, 4);
                    if (monthlySW == true && monthYear == projMonthYear)
                        {
                        foreach (WSParam parm in parms)
                            {
                            if (parm.MTD_GUID == mtdGUID)
                                {
                                    string mtdProjectID = project.OID;
                                    parm.MTD_ProjectID = mtdProjectID;
                                    service.Store(parm);
                                    returnValue = true;
                                }
                            }
                        }
                   }
            }
            return returnValue;
        }

        private int getWSContactIID(WSParamCollection parms, Guid mtdGUID)
        {
            int ContactIID = 0;
            foreach (WSParam parm in parms)
            {
                if (parm.MTD_GUID == mtdGUID)
                {
                    ContactIID = parm.Target_IID;
                }
            }
            return ContactIID;

        }

/*        private void putProjectNum(string mtdProjectID, WSParamCollection parms, Guid mtdGUID)
        {

            foreach (WSParam parm in parms)
            {
                if (parm.MTD_GUID == mtdGUID)
                {
                    parm.MTD_ProjectID = mtdProjectID;
                    service.Store(parm);
                }
            }
        }
 */

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

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            Console.WriteLine("Processed file '{0}'.", path);
        }

/*        private bool WriteSendLog(IMTDService service, User user, BaseExtendable project)
        {
            WSSendLog wslog = new WSSendLog();

            wslog.User_IID = user.User_IID;
            string debugLine = DateTime.Now + " wslog.User_IID " + user.User_IID; WriteDebugfile(debugLine);  
            wslog.Status_CID = Code.Find(102);
            wslog.Description = "Test description";
            wslog.Quote_IID = project.Entity_IID;
            wslog.Send_ID = user.User_IID;
            string seqNo = service.NextSequenceNumber(true);
            wslog.Seq_Number = 1;
            try
            {
                service.Store(wslog);
                return true;
            }
            catch (Exception err) { return false; }
        }
*/

        private bool WriteSendLog(IMTDService service, User user, BaseExtendable project)
        {
            WSSendLog wslog = new WSSendLog();
            wslog.User_IID = user.User_IID;
            string debugLine = DateTime.Now + " wslog.User_IID " + user.User_IID; WriteDebugfile(debugLine);
            wslog.Status_CID = Code.Find(102);
            wslog.Description = "Test description";
            wslog.Quote_IID = project.Entity_IID;
            wslog.Send_ID = user.User_IID;
            wslog.Seq_Number = 1;

/*
            try
            {
                service.Store(wslog);
                return true;
            }
            catch (Exception err) { return false; }
*/
            int tries = 3;
            while (true)
            {
                try
                {
                    service.Store(wslog);
                    debugLine = DateTime.Now + " WriteSendLog.service.Store (SUCCESS)"; WriteDebugfile(debugLine);
                    string seqNo = service.NextSequenceNumber(true);
                    return true; // success!
                }
                catch
                {
                    debugLine = DateTime.Now + " WriteSendLog.service.Store (FAILURE)"; WriteDebugfile(debugLine);
                    if (--tries == 0)
                        return false;
                    debugLine = DateTime.Now + " WriteSendLog.service.Store (RETRYING)"; WriteDebugfile(debugLine);
                    Thread.Sleep(1000); // 1 second
                }
            }

        }
    }
        [XmlRootAttribute(ElementName = "TMAMessage", IsNullable = false)]

        public class TMAMessage
        {
              public enum MSG_TYPE
            {
                Add_Update_Video_Content = 0,
                Add_Update_Doc_Content = 1,
                Add_Update_Web_Content = 2
            }
            private MSG_TYPE m_MessageType = MSG_TYPE.Add_Update_Doc_Content;  //default value

            private Int64 m_sequenceNumber;
            private string m_contentType;
            private string m_quoteOID;
            private string m_UID;
            private string m_status;
            private string m_logicalPath;
            private string m_GUID;
            private string m_language;
            private string m_geography;
            private string m_parentGUID;
            private string m_masterGUID;
            private string m_internalName;
            private string m_name;
            private string m_teaser;
            private string m_callToAction;
            private string m_externalURL;
            private string m_vgnName;
            private string m_description;

            public MSG_TYPE MessageType
            {
                get { return m_MessageType; }
                set { m_MessageType = value; }
            }
            public Int64 sequenceNumber
            {
                get { return m_sequenceNumber; }
                set { m_sequenceNumber = value; }
            }
            public string quoteOID
            {
                get { return m_quoteOID; }
                set { m_quoteOID = value; }
            }
            public string logicalPath
            {
                get { return m_logicalPath; }
                set { m_logicalPath = value; }
            }
            public string status
            {
                get { return m_status; }
                set { m_status = value; }
            }
            public string name
            {
                get { return m_name; }
                set { m_name = value; }
            }
            public string UID
            {
                get { return m_UID; }
                set { m_UID = value; }
            }

            public string GUID
            {
                get { return m_GUID; }
                set { m_GUID = value; }
            }
            public string language
            {
                get { return m_language; }
                set { m_language = value; }
            }
            public string geography
            {
                get { return m_geography; }
                set { m_geography = value; }
            }
            public string parentGUID
            {
                get { return m_parentGUID; }
                set { m_parentGUID = value; }
            }
            public string masterGUID
            {
                get { return m_masterGUID; }
                set { m_masterGUID = value; }
            }
            public string teaser
            {
                get { return m_teaser; }
                set { m_teaser = value; }
            }
            public string description
            {
                get { return m_description; }
                set { m_description = value; }
            }

            public string internalName
            {
                get { return m_internalName; }
                set { m_internalName = value; }
            }
            public string externalURL
            {
                get { return m_externalURL; }
                set { m_externalURL = value; }
            }
            public string vgnName
            {
                get { return m_vgnName; }
                set { m_vgnName = value; }
            }

            public TMAMessage()
            { }

            public TMAMessage(MSG_TYPE m_type)
            { MessageType = m_type; }

            public string ToXml()
            {

                StringWriter Output = new StringWriter(new StringBuilder());
                string Ret = "";

                try
                {
                    XmlSerializer s = new XmlSerializer(this.GetType());
                    s.Serialize(Output, this);

                    // To cut down on the size of the xml being sent to the database, we'll strip
                    // out this extraneous xml.

                    Ret = Output.ToString().Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                    Ret = Ret.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                    Ret = Ret.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "").Trim();
                }
                catch (Exception) { throw; }

                return Ret;
            } //toXML

 
        }
    class MyEncoder : UTF8Encoding
    {
        public MyEncoder()
        {

        }
        public override byte[] GetBytes(string s)
        {
            s = s.Replace("\\", "/");
            return base.GetBytes(s);
        }
    }
}
