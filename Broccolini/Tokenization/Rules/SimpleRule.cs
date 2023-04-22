using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed record SimpleRule : ITokenizerRule
{
    private readonly char _expectedCharacter;
    private readonly IniToken _token;

    public SimpleRule(char expectedCharacter, IniToken token)
    {
        _expectedCharacter = expectedCharacter;
        _token = token;
    }

    public IniToken? Match(ITokenizerInput input)
    {
        if (input.Peek(out var character) && character == _expectedCharacter)
        {
            input.Read();
            return _token;
        }

        return null;
    }
}
