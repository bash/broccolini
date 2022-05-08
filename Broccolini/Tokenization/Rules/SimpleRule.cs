using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed record SimpleRule : ITokenizerRule
{
    private readonly char _expectedCharacter;
    private readonly Token _token;

    public SimpleRule(char expectedCharacter, Token token)
    {
        _expectedCharacter = expectedCharacter;
        _token = token;
    }

    public Option<Token> Match(ITokenizerInput input, IReadOnlyList<Token> context)
    {
        if (input.Peek() == _expectedCharacter)
        {
            input.Read();
            return _token;
        }

        return Option<Token>.None();
    }
}
