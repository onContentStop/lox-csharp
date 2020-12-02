namespace Lox
{
    internal interface IErrorReporter
    {
        void Report(int line, string message, string where = "");
        bool HadError { get; }
    }
}