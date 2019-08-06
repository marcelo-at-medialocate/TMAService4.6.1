using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Configuration;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace TMAWebService
{
    public class BuildDir
    {

        public string buildQuoteDirectory(string quoteRoot, string yyyyMMDDHHMM, string projName, string XMLFilename)
        {
            string rootDir = ConfigurationSettings.AppSettings["MTD_PATH"];
            string quoteRootDir = rootDir + quoteRoot + yyyyMMDDHHMM + "-" + projName;
            string[] pathArray = XMLFilename.Split('\\');
            string filename = pathArray[pathArray.Length-1];
            projName = projName.Replace("\\", "-");
            projName = projName.Replace("/", "-");

            WindowsImpersonationContext impersonationContext = null;
            if (MiscTools.impersonateValidUser(ref impersonationContext))
            {
                //Insert your code that runs under the security context of a specific user here.
                if (Directory.Exists(rootDir) == true)
                {
                    if (Directory.Exists(rootDir + quoteRoot) == false)
                    {
                        Directory.CreateDirectory(rootDir + quoteRoot);


                    }

                    if (Directory.Exists(quoteRootDir) == false)
                    {
                        Directory.CreateDirectory(quoteRootDir);
                        /* According to Marcelo, no docs need to be ITAR compliant  9/6/12 */

                        //write file to this folder
                        File.Copy(XMLFilename, quoteRootDir + "\\" + filename);
                    }
                }
                else
                {
                    throw new Exception("Warning: the directory <b>" + rootDir + "</b> does not exists or is inaccessible.");
                }
                MiscTools.undoImpersonation(impersonationContext);
            }
            else
            {
                string error = "Your impersonation failed ";
                throw new Exception(error);
            }

            return quoteRootDir;

        }

        public string buildTransDirectory(string pathPrefix, string yyyyMMDDHHMM, string wsProjectID, string projName, string[] targetLocales, string XMLFilename)
        {
            string rootDir = ConfigurationSettings.AppSettings["MTD_PATH"];
            string transRoot = rootDir + pathPrefix + "4-MyMedialocate\\" + yyyyMMDDHHMM + "-" + wsProjectID + "-" + projName;
            string sourceRoot = rootDir + pathPrefix + "1-Preparation\\A-Source_Files\\" + yyyyMMDDHHMM + "-" + wsProjectID + "-" + projName;
            string[] pathArray = XMLFilename.Split('\\');
            string filename = pathArray[pathArray.Length-1];

            WindowsImpersonationContext impersonationContext = null;
            if (MiscTools.impersonateValidUser(ref impersonationContext))
            {
                //Insert your code that runs under the security context of a specific user here.
                if (Directory.Exists(rootDir) == true)
                {
                    if (Directory.Exists(sourceRoot) == false)
                    {
                        Directory.CreateDirectory(sourceRoot);

                        //write file to this folder
                        File.Copy(XMLFilename, sourceRoot + "\\" + filename);
                    }
                }
                if (Directory.Exists(rootDir) == true)
                {
                    if (Directory.Exists(transRoot) == false)
                    {
                        Directory.CreateDirectory(transRoot);

                        //write file to this folder
                    }
                    int count = targetLocales.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (targetLocales[i] != null)
                        {
                            string locale = targetLocales[i];
                            if (locale == "es_EC")
                            {
                                locale = "es_LA";
                            }

                            string transRootLang = transRoot + "\\" + locale;
                            if (Directory.Exists(transRootLang) == false)
                            {
                                Directory.CreateDirectory(transRootLang);
                                /* According to Marcelo, no docs need to be ITAR compliant  9/6/12 */
                            }
                        }
                    }

                }
                else
                {
                    throw new Exception("Warning: the directory <b>" + rootDir + "</b> does not exists or is inaccessible.");
                }
                MiscTools.undoImpersonation(impersonationContext);
            }
            else
            {
                string error = "Your impersonation failed ";
                throw new Exception(error);
            }

            return transRoot;

        }

        public void buildProjDirStructure(string monthYear, string projectNum, bool ITARFlag)
        {
            string debugLine = "";
            string rootDir = ConfigurationSettings.AppSettings["MTD_PATH"];
            string projRoot = rootDir + "Projects\\" + monthYear;

            string itarGroup = "ITAR";
            if (ConfigurationSettings.AppSettings["ITAR_GROUP"] != null)
            {
                itarGroup = ConfigurationSettings.AppSettings["ITAR_GROUP"];
            }

            //FileSystemAccessRule itarRule = new FileSystemAccessRule(itarGroup, FileSystemRights.FullControl, AccessControlType.Deny);
            FileSystemAccessRule itarRule = new FileSystemAccessRule(itarGroup, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit, PropagationFlags.InheritOnly, AccessControlType.Deny);

            WindowsImpersonationContext impersonationContext = null;
            if (MiscTools.impersonateValidUser(ref impersonationContext))
            {
                //Insert your code that runs under the security context of a specific user here.
                if (Directory.Exists(rootDir) == true)
                {
                    if (Directory.Exists(projRoot) == false)
                    {
                        Directory.CreateDirectory(projRoot);
                    }

                    projRoot = projRoot + "\\" + projectNum;
                    if (Directory.Exists(projRoot) == false)
                    {
                        DirectoryInfo itar = Directory.CreateDirectory(projRoot);
                        if (ITARFlag == true)
                        {
                            try
                            {

                                DirectorySecurity dirSec = itar.GetAccessControl();
                                dirSec.AddAccessRule(itarRule);
                                itar.SetAccessControl(dirSec);
                            }
                            catch (Exception ex)
                            {
                                debugLine = DateTime.Now + " Error setting project directory ITAR permissions"; WriteDebugfile(debugLine);
                            }
                        }

                    }

                    /////////////////////////////

                    // Admin
                    buildDirectory(projRoot + "\\0-Admin", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\0-Admin\\A-Schedules", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\0-Admin\\B-Misc", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\0-Admin\\C-Invoices", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\0-Admin\\D-Emails", ITARFlag, itarRule);

                    // Preparation
                    buildDirectory(projRoot + "\\1-Preparation", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\1-Preparation\\A-Source_Files", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\1-Preparation\\B-Engineering", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\1-Preparation\\B-Engineering\\1-File_Prep", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\1-Preparation\\B-Engineering\\2-Eng_Testing", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\1-Preparation\\B-Engineering\\3-To_Trans_Logs", ITARFlag, itarRule);

                    // Production

                    buildDirectory(projRoot + "\\2-Production", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\A-Translation", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\A-Translation\\1-From_Translation", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\A-Translation\\2-From_Editing", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\2-Production\\B-Formatting", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\B-Formatting\\1-To_Format", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\B-Formatting\\2-From_Format", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\2-Production\\C-Engineering", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\C-Engineering\\1-To_Engineering", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\C-Engineering\\2-Build", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\C-Engineering\\3-From_Engineering", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\2-Production\\D-Mech_QA", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\D-Mech_QA\\1-To_Mech_QA", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\D-Mech_QA\\2-From_Mech_QA", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\2-Production\\E-Ling_QA", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\E-Ling_QA\\1-To_Ling_QA", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\E-Ling_QA\\2-From_Ling_QA", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\2-Production\\F-Incorps", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\F-Incorps\\1-To_Incorps", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\F-Incorps\\2-From_Incorps", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\2-Production\\G-Client_Review", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\G-Client_Review\\1-To_Client", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\G-Client_Review\\2-From_Client", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\G-Client_Review\\3-From_Client_Review_Validation", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\2-Production\\H-Regression", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\H-Regression\\1-From_Regression", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\2-Production\\I-Final", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\2-Production\\I-Final\\1-Obsolete", ITARFlag, itarRule);

                    buildDirectory(projRoot + "\\3-Deliveries", ITARFlag, itarRule);
                    buildDirectory(projRoot + "\\4-MyMedialocate", ITARFlag, itarRule);
                }
                else
                {
                    debugLine = DateTime.Now + " Warning: the directory " + rootDir + " does not exists or is inaccessible."; WriteDebugfile(debugLine);
                }
                MiscTools.undoImpersonation(impersonationContext);
            }
            else
            {
                //Your impersonation failed. Therefore, include a fail-safe mechanism here.
            }


        }

        private void buildDirectory(string path, bool ITARFlag, FileSystemAccessRule itarRule)
        {
            string debugLine = "";
            if (Directory.Exists(path) == false)
            {
                DirectoryInfo itar = Directory.CreateDirectory(path);
                if (ITARFlag == true)
                {
                    try
                    {

                        DirectorySecurity dirSec = itar.GetAccessControl();
                        dirSec.AddAccessRule(itarRule);
                        itar.SetAccessControl(dirSec);
                    }
                    catch (Exception ex)
                    {
                        debugLine = DateTime.Now + " Error setting project directory ITAR permissions "; WriteDebugfile(debugLine);
                    }
                }
            }
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