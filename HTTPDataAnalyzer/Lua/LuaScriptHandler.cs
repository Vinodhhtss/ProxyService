using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Neo.IronLua;

namespace HTTPDataAnalyzer
{
    public class LuaScriptHandler
    {
        private static Lua m_Lua;
        private static LuaGlobal m_LuaGlobal;
        private string LUA_SCRIPT_LOCATION = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                              Path.Combine(ConstantVariables.GetAppDataFolder(), ConstantVariables.GetLuaScriptsFolder()));

        public const string PROXY_SERVICE_API_OBJECT_NAME = "ProxyAPIObject";

        static LuaScriptHandler()
        {
            m_Lua = new Lua();
            m_LuaGlobal = m_Lua.CreateEnvironment();
        }

        public LuaScriptHandler()
        {
            if (!Directory.Exists(LUA_SCRIPT_LOCATION))
            {
                Directory.CreateDirectory(LUA_SCRIPT_LOCATION);
            }
        }

        public bool Execute(SessionHandler oSessionHndlr)
        {
            if (oSessionHndlr.LuaLogger == null)
            {
                //oSessionHndlr.LuaLogger = new CLogger(oSessionHndlr.HostName, string.Empty, oSessionHndlr.ThreadIndex, true);
            }
            //oSessionHndlr.LuaLogger.WriteLogInfo("Script Execution started");
            bool result = false;
            try
            {
                //oSessionHndlr.LuaLogger.WriteLogInfo("Getting list of lua script files");

                string[] scriptFiles = System.IO.Directory.GetFiles(@LUA_SCRIPT_LOCATION, ConstantVariables.LUA_SCRIPTS_SEARCH_PATTERN);
                if (scriptFiles.Length == 0)
                {
                    //oSessionHndlr.LuaLogger.WriteLogInfo("No ScriptFiles Found.So, Script files found");
                    return false;
                }

                    var proxyAPI = new ProxyServiceAPI(oSessionHndlr);
                    foreach (var scriptFile in scriptFiles)
                    {
                        string currentScriptFile = Path.GetFileNameWithoutExtension(scriptFile);
                        //oSessionHndlr.LuaLogger.WriteLogInfo("Getting  lua script filename:" + currentScriptFile);

                        try
                        {
                            LuaResult lr = m_LuaGlobal.DoChunk(File.ReadAllText(String.Format(scriptFile, 1)), 
                                currentScriptFile, new KeyValuePair<string, object>(PROXY_SERVICE_API_OBJECT_NAME, proxyAPI));
                        }
                        catch (Exception ex)
                        {
                            //oSessionHndlr.LuaLogger.WriteLogException("Script Name:" + currentScriptFile + ex.Message + Environment.NewLine + ex.StackTrace);
                        }
                    }
                }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
                Debug.Write(ex.Message + " " + ex.StackTrace);
            }
            return result;
        }
    }
}