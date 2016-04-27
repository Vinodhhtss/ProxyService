using System.IO;

namespace HTTPDataAnalyzer
{
    public class NameIdentifier : IConditionChecker
    {
        public string ExeName = "Exploiter";

        public bool CheckCondition(SessionHandler oSessionHandler)
        {
            if (oSessionHandler.ClientName.Contains(Path.GetFileName(ExeName)))
            {
                return true;
            }
            return false;
        }
    }
}
