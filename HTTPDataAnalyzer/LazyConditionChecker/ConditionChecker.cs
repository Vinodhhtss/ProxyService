using HTTPDataAnalyzer.Lazy;
using System;
using System.Collections.Generic;

namespace HTTPDataAnalyzer
{
    public class ConditionChecker
    {
        public static Dictionary<string, string> Conditions = new Dictionary<string, string>();

        static ConditionChecker()
        {
            Conditions.Add("NameIdentifier", "HTTPDataAnalyzer.NameIdentifier");
            Conditions.Add("PDFIdentifier", "HTTPDataAnalyzer.PDFIdentifier");
        }
        public static void CheckAllConditions(SessionHandler oSessionHandler)
        {
            //LazyAnalyser.Logger.Info("Enter");

            string tempAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            foreach (var item in Conditions)
            {
                try
                {
                    var obj = Activator.CreateInstance(tempAssemblyName, item.Value);
                    var condition = (IConditionChecker)obj.Unwrap();
                    if (condition.CheckCondition(oSessionHandler))
                    {
                        AnalyzerManager.ProxydbObj.UpdateDB("update response set lazy_status = 2 where request_id = '" + oSessionHandler.RequestID + "'");
                    }
                    else
                    {
                        AnalyzerManager.ProxydbObj.UpdateDB("update response set lazy_status = 1 where request_id = '" + oSessionHandler.RequestID + "'");
                    }
                }
                catch (Exception ex)
                {
                    //LazyAnalyser.Logger.Error(ex);
                }
            }

            //LazyAnalyser.Logger.Info("Exit");
        }
    }
}
