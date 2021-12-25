namespace CodeGen.Exceptions
{
    public enum HumanizedExceptionAction
    {
        Hide,
        InformUserAndExit,
        InformUserAndContinue,
        InformUserAndThrow
    }
    public class HumanizedException : Exception
    {
        public HumanizedExceptionAction Action { get; protected set; }    
        public HumanizedException(string message, HumanizedExceptionAction action = HumanizedExceptionAction.InformUserAndExit, Exception innerException = null) : base(message, innerException)
        {
            this.Action = action;
        }
    }
}