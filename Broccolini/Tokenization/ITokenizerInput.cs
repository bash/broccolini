namespace Broccolini.Tokenization;

internal interface ITokenizerInput
{
    Option<char> Peek(int lookAhead = 0);

    char Read();

    string ReadWhile(Func<char, bool> predicate);
}
