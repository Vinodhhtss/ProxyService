using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace HTTPProxyServer
{
    public class LuaScriptHandler
    {
        private static Lua m_Lua;
        private static LuaGlobal m_LuaGlobal;
        private const string PROXY_SERVICE_API_OBJECT_NAME = "ProxyAPIObject";

        private string LUA_SCRIPT_LOCATION = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                              Path.Combine(ConstantVariables.GetAppDataFolder(), ConstantVariables.GetLuaScriptsFolder()));

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
                //oSessionHndlr.LuaLogger = new CLogger(oSessionHndlr.RequestURL, string.Empty, oSessionHndlr.ThreadIndex, true);
            }
            ////oSessionHndlr.LuaLogger.Logger.Info("Script Execution started");
            bool result = false;
            try
            {
                ////oSessionHndlr.LuaLogger.Logger.Info("Getting list of lua script files");

                string[] scriptFiles = System.IO.Directory.GetFiles(@LUA_SCRIPT_LOCATION, ConstantVariables.LUA_SCRIPTS_SEARCH_PATTERN);
                if (scriptFiles.Length == 0)
                {
                    ////oSessionHndlr.LuaLogger.Logger.Info("No ScriptFiles Found.So, Script files found");
                    return false;
                }

                    var proxyAPI = new ProxyServiceAPI(oSessionHndlr);
                    foreach (var scriptFile in scriptFiles)
                    {
                        string currentScriptFile = Path.GetFileNameWithoutExtension(scriptFile);
                        ////oSessionHndlr.LuaLogger.Logger.Info("Getting  lua script filename:" + currentScriptFile);

                        try
                        {
                            LuaResult lr = m_LuaGlobal.DoChunk(File.ReadAllText(String.Format(scriptFile, 1)), 
                                currentScriptFile, new KeyValuePair<string, object>(PROXY_SERVICE_API_OBJECT_NAME, proxyAPI));
                        }
                        catch (Exception ex)
                        {
                            ////oSessionHndlr.LuaLogger.Logger.Error ("Script Name:" + currentScriptFile ,ex);
                        }
                    }
                }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            return result;
        }
    }
}