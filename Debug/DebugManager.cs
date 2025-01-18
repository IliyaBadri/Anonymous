namespace Anonymous.Debug
{
    public class DebugManager
    {
        public static void LogException(Exception exception)
        {
            if (exception == null) {
                return;
            }

            string report = "Exception log:\n-----BEGIN ANONYMOUS ERROR-----\nId (HRESULT): " + exception.HResult.ToString() + "\nMessage: " + exception.Message;

            if (exception.Source != null) {
                report += "\nSource: " + exception.Source;
            }

            if (exception.HelpLink != null) {
                report += "\nHelp link: " + exception.HelpLink;
            }

            if (exception.StackTrace != null) {
                report += "\nStack trace:\n" + exception.StackTrace;
            }

            report += "\n-----END ANONYMOUS ERROR-----";

            Console.WriteLine(report);
        
        }
    }
}
