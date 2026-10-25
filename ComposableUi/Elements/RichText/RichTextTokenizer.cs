using System;
using System.Collections.Generic;

namespace ComposableUi
{
    public static class RichTextTokenizer
    {
        private static readonly WordBreakRule[] _wordBreakRules =
        [
            new(['!', '?', '-', '}', '/', '|'],
                [')', ']', '.', ',', '"', '\'', ';', ':'],
                true),
            new ([')', ']', '.', ',', ';', ':', '$', '%', '\\', '+', '№'],
                ['(', '[', '{', '$', '%', '\\', '+', '№'],
                false)
        ];

        public static void Tokenize(string text, List<RichTextToken> tokens)
        {
            for (var i = 0; i < text.Length; i++)
            {
                var currentIndex = i;
                var length = 1;

                var currentCharacter = text[i];
                char? nextCharacter = currentIndex < text.Length - 1
                    ? text[currentIndex + 1]
                    : null;

                var isCaretReturn = currentCharacter == '\r';
                if (isCaretReturn)
                {
                    var hasNextNewLine = nextCharacter.HasValue
                        && nextCharacter.Value == '\n';
                    if (hasNextNewLine)
                    {
                        i++;
                        length++;
                    }

                    tokens.Add(new RichTextToken(TokenType.LineBreak, currentIndex, length));
                    continue;
                }

                var isNewLine = currentCharacter == '\n';
                if (isNewLine)
                {
                    tokens.Add(new RichTextToken(TokenType.LineBreak, currentIndex, length));
                    continue;
                }

                if (char.IsWhiteSpace(currentCharacter))
                {
                    tokens.Add(new RichTextToken(TokenType.Whitespace, currentIndex, length));
                    tokens.Add(new RichTextToken(TokenType.WordBreak, currentIndex, 0));
                    continue;
                }

                if (char.IsControl(currentCharacter))
                    continue;

                if (char.IsLetterOrDigit(currentCharacter))
                {
                    tokens.Add(new RichTextToken(TokenType.LetterOrDigit, currentIndex, length));
                    continue;
                }

                tokens.Add(new RichTextToken(TokenType.Symbol, currentIndex, length));

                if (!nextCharacter.HasValue)
                    break;

                foreach (var wordBreakRule in _wordBreakRules)
                {
                    if (Array.IndexOf(wordBreakRule.TriggerCharacters, currentCharacter) < 0)
                        continue;

                    var nextCharacterMatchesCondition = Array.IndexOf(wordBreakRule.ConditionalCharacters, nextCharacter.Value) >= 0;
                    if (nextCharacterMatchesCondition == !wordBreakRule.NegateCondition)
                    {
                        tokens.Add(new RichTextToken(TokenType.WordBreak, currentIndex, 0));
                        break;
                    }
                }
            }
        }
    }
}
