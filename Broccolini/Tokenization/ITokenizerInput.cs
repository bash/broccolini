namespace Broccolini.Tokenization;

internal interface ITokenizerInput
{
    bool Peek(out char character, int lookAhead = 0);

    char Read();

    string ReadWhile(Func<char, bool> predicate);
}
