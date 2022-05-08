namespace Broccolini.Tokenization;

internal sealed class TokenizerInput : ITokenizerInput
{
    private readonly string _input;
    private int _position;

    public TokenizerInput(string input) => _input = input;

    public Option<char> Peek(int lookAhead = 0)
        => _position + lookAhead < _input.Length
            ? _input[_position + lookAhead]
            : Option<char>.None();

    public char Read()
        => _position < _input.Length
            ? _input[_position++]
            : throw new InvalidOperationException("Unable to advance tokenizer: End of input");

    public string ReadWhile(Func<char, bool> predicate)
    {
        var startPosition = _position;

        for (; _position < _input.Length && predicate(_input[_position]); _position++)
        {
        }

        return _input.Substring(startPosition, _position - startPosition);
    }
}
