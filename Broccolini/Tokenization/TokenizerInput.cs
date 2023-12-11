namespace Broccolini.Tokenization;

internal sealed class TokenizerInput(string input) : ITokenizerInput
{
    private int _position;

    public bool Peek(out char character, int lookAhead = 0)
    {
        if (_position + lookAhead < input.Length)
        {
            character = input[_position + lookAhead];
            return true;
        }

        character = default;
        return false;
    }

    public char Read()
        => _position < input.Length
            ? input[_position++]
            : throw new InvalidOperationException("Unable to advance tokenizer: End of input");

    public string ReadWhile(Func<char, bool> predicate)
    {
        var startPosition = _position;

        for (; _position < input.Length && predicate(input[_position]); _position++)
        {
        }

        return input.Substring(startPosition, _position - startPosition);
    }
}
