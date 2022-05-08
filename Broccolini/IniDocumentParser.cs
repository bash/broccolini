using Broccolini.Parsing;
using Broccolini.Tokenization;

namespace Broccolini;

public static class IniDocumentParser
{
    public static IniDocument Parse(string input)
    {
        var tokens = Tokenizer.Tokenize(new TokenizerInput(input));
        return Parser.Parse(new ParserInput(tokens));
    }
}
