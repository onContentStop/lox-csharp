namespace Lox
{
    internal interface IErrorReporter
    {
        void Report(int line, string message, string where = "");
        void Report(Token token, string message);
        bool HadError { get; }
        void ReportRuntimeError(RuntimeError error);
    }
}