namespace BooksPortal.Application.Common.Exceptions;

public class SetupIncompleteException : BusinessRuleException
{
    public const string ErrorCode = "SETUP_INCOMPLETE";

    public SetupIncompleteException(
        string message,
        IReadOnlyCollection<string> missingSteps,
        IReadOnlyCollection<string> hints)
        : base(message)
    {
        MissingSteps = missingSteps;
        Hints = hints;
    }

    public IReadOnlyCollection<string> MissingSteps { get; }
    public IReadOnlyCollection<string> Hints { get; }
}
