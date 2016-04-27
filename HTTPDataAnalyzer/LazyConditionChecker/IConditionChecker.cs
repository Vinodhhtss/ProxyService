namespace HTTPDataAnalyzer
{
    public interface IConditionChecker
    {
        bool CheckCondition(SessionHandler oSessionHandler);
        //void SendMessage(SessionHandler oSessionHandler);
    }
}
