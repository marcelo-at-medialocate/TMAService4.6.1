using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Xml;
using Com.Idiominc.Webservices.Client;
using Com.Idiominc.Webservices.Client.Scoping;
using Com.Idiominc.Webservices.Client.Ais;
using Com.Idiominc.Webservices.Client.Workflow;
using Com.Idiominc.Webservices.Client.User;
using Com.Idiominc.Webservices.Client.Mt;
using Com.Idiominc.Webservices.Client.Tm;
using Com.Idiominc.Webservices.Client.Asset;
using Com.Idiominc.Webservices.Client.Quote;
using Com.Idiominc.Webservices.Client.Linguistic;

using AppClass;
using Base;
using Services;

namespace TMAWebService
{
    public class WSScopeMgr
    {
        private static WSContext ctx;

        public string[] getScopeInfo(string srcName, string projName, string projDescription, string file, Code srcLocale, Code[] dstLocales, int localeCount, string datatype, BaseExtendable mtdProject, IMTDService service, string WS_Client_Name, string Project_Type, string quoteType, string WSurl, string ProjectNum, string[] projectAttribs)
        {

            string retVal = "success";
            string FILE_NAME = "473263";
            string workgroupName = "default";
            string workflowName = "1. Translation Only";
            string[] entityIIDArray = new string[4];
            string update = "no";
            string review = "no";
            string client_review = "no";

            if (projectAttribs != null) {
                foreach (string projectAtrib in projectAttribs)
                {
                    if (projectAtrib == "update")
                    {
                        update = "yes";
                    } else if (projectAtrib == "review")
                    {
                        review = "yes";
                    } else if (projectAtrib == "client_review")
                    {
                        client_review = "yes";
                    }
                }
            }
            projName = projName.Replace("\\", "-");
            projName = projName.Replace("/", "-");
            projName = projName.Replace(":", "-");
            string src_asset = srcName.Replace("\\", "/");
            string WSuserName = ConfigurationSettings.AppSettings["WS_USR"];
            string WSpwd = ConfigurationSettings.AppSettings["WS_PWD"];

            WSContext ctx = new WSContext(WSuserName, WSpwd, WSurl); //http://172.20.20.36:8585/ws/services

            string debugLine = DateTime.Now + " Got WS context "; WriteDebugfile(debugLine);
            debugLine = DateTime.Now + " source asset name = " + src_asset; WriteDebugfile(debugLine);

            WSAisManager aisManager = ctx.getAisManager();
            WSUserManager userMgr = ctx.getUserManager();
            WSAssetManager assetMgr = ctx.getAssetManager();
            WSWorkflowManager workflowMgr = ctx.getWorkflowManager();
            WSQuoteManager quoteMgr = ctx.getQuoteManager();
            WSScopeManager scopeMgr = ctx.getScopeManager();
            Code mdlCode = null;
            Base.Text codeID = null;
            int ID = 0;
            codeID = srcLocale.Description_TID;
            ID = Convert.ToInt32(codeID.Text_IID.ToString());
            
            debugLine = DateTime.Now + " Got src locale id= " + ID.ToString(); WriteDebugfile(debugLine);
            WSLocale wslocale = null;
            if (WS_Client_Name == "Seagate")
            {
                wslocale = userMgr.getLocale2(GetSeagateLocID(ID));
            } else {
                wslocale = userMgr.getLocale2(ID);
            }
            
            if (wslocale == null)
            {
                throw new System.InvalidOperationException("Invalid src locale");
            }

            try
            {
                WSWorkgroup workgroup = userMgr.getWorkgroup(workgroupName);
                WSWorkflow workflow = workflowMgr.getWorkflow(workflowName);
                //int pId = 13066;

                //tokenize dest locales
                //char[] sep = { ';' };
                //String[] res = dstLocales.Split(sep);
                int numLocs = localeCount; //dstLocales.Length;
                WSLocale[] locales = null;
                Code targCode;
                locales = new WSLocale[numLocs];
                for (int i = 0; i < numLocs; i++)
                {
                    //string localeName = res[i].Trim();
                    targCode = dstLocales[i];
                    debugLine = DateTime.Now + " Got target locale code= " + targCode; WriteDebugfile(debugLine);
                    if (targCode != null)
                    {
                        //mdlCode = Code.FindAltType("LOC", localeName);
                        codeID = targCode.Description_TID;
                        debugLine = DateTime.Now + " Got target locale codeID= " + codeID; WriteDebugfile(debugLine);
                        ID = Convert.ToInt32(codeID.Text_IID.ToString());
                        debugLine = DateTime.Now + " Got target locale id= " + ID.ToString(); WriteDebugfile(debugLine);
                        WSLocale locale = null;
                        if (WS_Client_Name == "Seagate")
                        {
                            locale = userMgr.getLocale2(GetSeagateLocID(ID));
                        } else {
                            locale = userMgr.getLocale2(ID);
                        }
                        locales[i] = locale;
                        if (locale == null)
                        {
                            throw new System.InvalidOperationException("Error in target locale " + locale);
                        }
                    }
                    else
                    {
                        throw new System.InvalidOperationException("Error in target code " + targCode);
                    }
                }
                if (locales[0] == null) 
                { 
                    throw new System.InvalidOperationException("No target locales defined"); 
                }
//                WSClient[] ctxclients = ctx.getUser.get.getClients();
                WSWorkflow DefaultWF = null;
                WSTm DefaultTm = null;
                WSClient client = userMgr.getClient(WS_Client_Name);
                WSProjectType projectType = null;
//                for (int o = 0; o < ctxclients.Length; o++)
//               {

                    if (!(client == null))
                    {

//                        client = ctxclients[o];

//                        WSProjectType[] ctxtypes = client.;

                        String inProjType = Project_Type.Trim();
//                        inProjType = inProjType.Replace(" ","");
                        projectType = workflowMgr.getProjectType(inProjType);
                        if (projectType == null)
                        {
                            throw new System.InvalidOperationException("No match in WS found for project type " + Project_Type);
                        }
//                       for (int p = 0; p < ctxtypes.Length; p++)
//                       {
//                           String projType = ctxtypes[p].getName.Trim();
//                           projType = projType.Replace(" ","");
//                           //if (String.Compare(projType, Project_Type) == 0)
//                           if (projType.Equals(inProjType) )
//                           {
//                                projectType = ctxtypes[p];
                                DefaultWF = projectType.getDefaultWorkflow;
                                DefaultTm = projectType.getDefaultTm;
                                debugLine = DateTime.Now + " Got project type "+ projectType.getDisplayString; WriteDebugfile(debugLine);

//                                break;
//                            }
//
//                       }


                    }
                    else
	                {
                        throw new System.InvalidOperationException("Client Name does not exist \"" + WS_Client_Name + "\"");
	                }

//                }


                string[] attachedFile = new string[1];
                attachedFile[0] = src_asset;  //file to 'attach' to project
                string[] custom = new string[2];
                custom[0] = null;
                custom[1] = null; //custom ais properties

                try
                {
                    string clientPath = "/Client Files/" + WS_Client_Name;
                    if (clientPath == null)
                    {
                        debugLine = DateTime.Now + " Parameter clientPath cannot be null = " + clientPath; WriteDebugfile(debugLine);
                        throw new System.ArgumentException("Parameter clientPath cannot be null", clientPath);
                    }
                    WSNode clientNode = aisManager.getNode(clientPath);
                    string clientTempPath = clientPath + "/Temp";
                    if (clientTempPath == null)
                    {
                        aisManager.create(clientTempPath,clientNode);
                    }
                    WSNode clientTempNode = aisManager.getNode(clientTempPath);
                    string aisPath =   clientTempPath + "/Upload";
                    if (aisPath == null)
                    {
                        aisManager.create(aisPath,clientTempNode);
                    }
                    string oldPath = aisPath;
                    WSNode oldNode = aisManager.getNode(oldPath);
                    WSProjectGroup projectGroup = workflowMgr.createProjectGroup(ProjectNum + "-" + WS_Client_Name + "-" + projName, projDescription, locales, attachedFile, client, projectType, custom);
                    WSProject[] theProjects = projectGroup.getProjects;
                    foreach (WSProject theProject in theProjects)
                    {
                        theProject.setAttribute("update", update);
                        theProject.setAttribute("review", review);
                        theProject.setAttribute("creview", client_review);

                        //string theLocaleString = "Target-" + theProject.getTargetLocale.getDisplayString;
                        //WSTask[] theProjectTasks = theProject.getTasks();
                        //string theNodeString = "";
                        //string [] theNodeStringArray = theProjectTasks[0].getTargetPath.Split('/');
                        //int countHolder = 0;
                        //for (int i = theNodeStringArray.Length -1; theNodeStringArray.Length > 0; i--)
                        //{
                        //    if ( theNodeStringArray[i].Equals(theLocaleString))
                        //    {
                        //        countHolder = i;
                        //        break;
                        //    }
                        //}
                        
                        //for (int j=1; j <= countHolder; j++)
                        //{
                        //    theNodeString += "/" + theNodeStringArray[j];
                        //}
                    
                    }


                    debugLine = DateTime.Now + " Created WS Project Group = " + projectGroup.getId; WriteDebugfile(debugLine);
                    entityIIDArray = createProjectScope(mtdProject, service, projectGroup, projName, projDescription, locales, attachedFile, client, projectType);
                }
                catch (Exception e)
                { 
                    debugLine = DateTime.Now + " " + e.Message; WriteDebugfile(debugLine);
                    throw new System.InvalidOperationException(e.Message);
                }

            }
            catch (Exception e)
            { 
                throw new System.InvalidOperationException(e.Message);
            }
        return entityIIDArray;
     }

        private string[] createProjectScope(BaseExtendable mtdProject,  IMTDService service, WSProjectGroup projectGroup, string projName, string projDescription, WSLocale[] locales, string[] attachedFile, WSClient client, WSProjectType projectType)
     {
                string retVal = null;
                string[] custom = null;
                string debugLine;
                string pNum = "";
 
                EntityVersion newVersion = new EntityVersion();
                string[] entityIIDArray = createQuoteSectionAndAssessment(service, mtdProject, newVersion, projName, projDescription);
                string mtdProjID = mtdProject.OID;  //to be used for project tracking code
                string WSProjID = projectGroup.getId;

//              TODO: the project tracking code should be the identifier for the Quote/Assessment relationship and the project number. As well as the WS project number.
                string projectTrackingCode = mtdProjID + "_" + WSProjID;
                entityIIDArray[2] = mtdProjID;
                entityIIDArray[3] = WSProjID;

               debugLine = DateTime.Now + " Project tracking code: " + projectTrackingCode;  WriteDebugfile(debugLine);
               debugLine = DateTime.Now + " Entiry Array code: " + entityIIDArray[0] + ", " + entityIIDArray[1]; WriteDebugfile(debugLine);           

               return entityIIDArray;
        }


        private string getScope(WSAggregateTranslationScope aggscope, WSAssetTranslationScope scope, WSLocale locale, BaseExtendable mtdProject, IMTDService service, int sequence)
        {
            //create Quote Section
            
            // TODO: This line should probably go away. We should pass the correct entity relation so that the words counts are loaded in the correct Assessment and Quote sections.
             EntityVersion newVersion = new EntityVersion();

//            I don't think we need this here. We don't want to create a new quote and assessment section for every locale.
//            createQuoteSectionAndAssessment(service, mtdProject, newVersion, projName, projDescription);



            TranslationMemory memory = new TranslationMemory();
            int total_word_count = 0;
            if (scope != null)
            {
                memory.Words_XTrans += scope.getIceWordCount;
                memory.Words_Rep += scope.getRepetitionWordCount;
                memory.Words_100 += scope.getPerfectWordCount;
                memory.Words_95 += scope.getFuzzyWordCount[0];
                memory.Words_85 += scope.getFuzzyWordCount[1];
                memory.Words_75 += scope.getFuzzyWordCount[2];
                memory.Words_50 += scope.getFuzzyWordCount[3];
                memory.Words_NM += scope.getFuzzyWordCount[4];
                total_word_count += memory.Words_XTrans + memory.Words_Rep + memory.Words_100 + memory.Words_95 + memory.Words_85 + memory.Words_75 + memory.Words_50 + memory.Words_NM;
            }
            else  //aggregate scopes for 'auto' quote
            {
                foreach (WSAssetTranslationScope ats in aggscope.getScopes)
                {
                    memory.Words_XTrans += ats.getIceWordCount;
                    memory.Words_Rep += ats.getRepetitionWordCount;
                    memory.Words_100 += ats.getPerfectWordCount;
                    memory.Words_95 += ats.getFuzzyWordCount[0];
                    memory.Words_85 += ats.getFuzzyWordCount[1];
                    memory.Words_75 += ats.getFuzzyWordCount[2];
                    memory.Words_50 += ats.getFuzzyWordCount[3];
                    memory.Words_NM += ats.getFuzzyWordCount[4];
                    total_word_count += memory.Words_XTrans + memory.Words_Rep + memory.Words_100 + memory.Words_95 + memory.Words_85 + memory.Words_75 + memory.Words_50 + memory.Words_NM;
                }
            }
            string custIID =ConfigurationSettings.AppSettings["CUSTOMER_IID"];
            int customerIID = Convert.ToInt32(custIID);
            string lang = locale.getName.ToUpper();
            lang = lang.Replace(" - UTF8", "");  //Code lang names are LANG_LOCATION unlike lang name in WS 
            lang = lang.Replace(" (","_");
            lang = lang.Replace(")","");

            //Code test = Code.FindAltType("LOC", "ar_EG");
            //use alternate_type to find code in MDL
            
            Code langCode = Code.Find("LOC", lang);
            QuoteItem[] existingItems = null;
            Rate currRate = null;
            string retValue = null;
            if (langCode != null)  //it should never be null, otherwise what is the translation?
            {
                currRate = service.LoadRate(customerIID, langCode);
            }
            else
            {
                retValue = "missing langCode for " + lang;
                return retValue;
            }
            
            // Now do Translation memory Items
            Code wordCode = Code.Find("QUOTE_UNIT", "WORDS");
                if (total_word_count > 0)
                {
                    // If separate ICE matches selected, split them into their own category, otherwise add them to REPS
                        if (memory.Words_XTrans > 0)
                        {
                            Code repCode = Code.Find("QUOTE_ITEM", "TRANSLATION_ICE");
                            QuoteItem storeItem = fetchMemItemMatch(newVersion, langCode, repCode, wordCode, memory.Words_XTrans, 0 /* currRate.Reps_Rate */, ref  sequence, existingItems);
                            service.Store(storeItem);
                            retValue = retValue + " XTrans Quote IID " + storeItem.Quote_Item_IID;
                        }
                        if ((memory.Words_Rep + memory.Words_100) > 0)
                        {
                            Code repCode = Code.Find("QUOTE_ITEM", "TRANSLATION_REPS");
                            QuoteItem storeItem = fetchMemItemMatch(newVersion, langCode, repCode, wordCode, (memory.Words_Rep + memory.Words_100), currRate.Reps_Rate, ref  sequence, existingItems);
                            service.Store(storeItem);
                            retValue = retValue + " 100 Quote IID " + storeItem.Quote_Item_IID;
                        }
 
                    if ((memory.Words_95 + memory.Words_85 + memory.Words_75) > 0)
                    {
                        Code fuzzyCode = Code.Find("QUOTE_ITEM", "TRANSLATION_FUZZY");
                        QuoteItem storeItem = fetchMemItemMatch(newVersion, langCode, fuzzyCode, wordCode, memory.Words_95 + memory.Words_85 + memory.Words_75, currRate.Fuzzy_Rate, ref  sequence, existingItems);
                        service.Store(storeItem);
                        retValue = retValue + " 95 Quote IID " + storeItem.Quote_Item_IID;
                    }
 
                    if ((memory.Words_50 + memory.Words_NM) > 0)
                    {
                        Code newCode = Code.Find("QUOTE_ITEM", "TRANSLATION_NEW");
                        QuoteItem storeItem = fetchMemItemMatch(newVersion, langCode, newCode, wordCode, memory.Words_50 + memory.Words_NM, currRate.New_Rate, ref  sequence, existingItems);
                        service.Store(storeItem);
                        retValue = retValue + " 50 Quote IID " + storeItem.Quote_Item_IID;
           
                    }
 
                }

                // TODO: This should be imported into the assessment and quote section matching the Entity Relation
                return retValue;
        }

        private string[] createQuoteSectionAndAssessment(IMTDService service, BaseExtendable m_project, EntityVersion newVersion, string projName, string projDescription)
        {
            //Quote section
            string[] returnValue = new string[4];
            BaseExtendable newQuote = new BaseExtendable(Code.Find(AppCodes.QUOTE_TARGET_TYPE).CID);
            newQuote.Description = projName;
            service.Store(newQuote);


            newVersion.Target_IID = newQuote.Entity_IID;
            newVersion.Target_Type_CID = Code.Find(AppCodes.QUOTE_TARGET_TYPE);
            newVersion.Version = "1.0";
            newVersion.Released = true;
            service.Store(newVersion);

            EntityRelation newRelation = new EntityRelation(Code.Find(AppCodes.QUOTE_TARGET_TYPE));
            newRelation.Entity_IID_1 = m_project.Entity_IID;
            newRelation.Entity_IID_2 = newQuote.Entity_IID;
            newRelation.Entity_2_Version_IID = newVersion.Entity_Version_IID;
            returnValue[0] = Convert.ToString(newVersion.Entity_Version_IID);
            service.Store(newRelation);
            
            //Assessment
            BaseExtendable newAssessment = new BaseExtendable(Code.Find(AppCodes.ASSESSMENT_TARGET_TYPE).CID);
            newAssessment.Description = projName;
            service.Store(newAssessment);
          
            EntityRelation newRelation2 = new EntityRelation(Code.Find(AppCodes.ASSESSMENT_TARGET_TYPE));
            newRelation2.Entity_IID_1 = m_project.Entity_IID;
            newRelation2.Entity_IID_2 = newAssessment.Entity_IID;
            returnValue[1] = Convert.ToString(newAssessment.Entity_IID);
            service.Store(newRelation2);

            return returnValue;
        }

        private QuoteItem fetchMemItemMatch(EntityVersion newVersion, Code langCode, Code memCode, Code wordCode, int quantity, decimal rate, ref int sequence, QuoteItem[] existingItems)
        {


            QuoteItem storeItem = null;

            // We have a new Item
            if (storeItem == null)
            {
                storeItem = new QuoteItem();

                storeItem.Quote_IID = newVersion.IID;
                storeItem.Quote_Version_IID = newVersion.Entity_Version_IID;
                storeItem.Language_CID = langCode;
                storeItem.Sequence_Number = sequence;
                storeItem.Charge_Item_CID = memCode;
                storeItem.Quantity = quantity;
                storeItem.Unit_CID = wordCode;
                storeItem.Unit_Rate = rate;

            }

            return storeItem;
        }

        public string getWSProjName(string WSurl, string projNumber)
        {
            string projName="";
            string WSuserName = ConfigurationSettings.AppSettings["WS_USR"];
            string WSpwd = ConfigurationSettings.AppSettings["WS_PWD"];

            WSContext ctx = new WSContext(WSuserName, WSpwd, WSurl);
            WSWorkflowManager workflowMgr = ctx.getWorkflowManager();
            int intProjNumber = Int32.Parse(projNumber);
            if (workflowMgr.getProjectGroup(intProjNumber) == null)
            {
                return "Project does not exist";
            }
            else
            {
                projName = workflowMgr.getProjectGroup(intProjNumber).getName;
            }
            return projName;
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


    }
}