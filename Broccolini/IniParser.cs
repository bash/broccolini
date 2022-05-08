using Broccolini.Parsing;
using Broccolini.Syntax;
using Broccolini.Tokenization;

namespace Broccolini;

public static class IniParser
{
    public static IniDocument Parse(string input)
    {
        var tokens = Tokenizer.Tokenize(new TokenizerInput(input));
        return Parser.Parse(new ParserInput(tokens));
    }
}
