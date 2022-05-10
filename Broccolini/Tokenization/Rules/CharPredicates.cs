namespace Broccolini.Tokenization.Rules;

internal static class CharPredicates
{
    public static bool IsIdentifier(char c) => !IsLineBreak(c) && !IsWhitespace(c) && c is not '[' and not ']' and not '=' and not '\'' and not '"' and not ';';

    public static bool IsLineBreak(char c) => c is '\r' or '\n';

    public static bool IsWhitespace(char c) => c is ' ' or '\t' or '\v' or '\f';
}
