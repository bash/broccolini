using Broccolini.Syntax;

namespace Broccolini.Tokenization;

internal interface ITokenizerRule
{
    IniToken? Match(ITokenizerInput input);
}
