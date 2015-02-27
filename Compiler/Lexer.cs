using Compiler.Exceptions;
using Compiler.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public static class Lexer
    {
        /// <summary>
        /// Top-level function.  Returns the next token and delegates work.
        /// Trims the input string as necessary.
        /// </summary>
        public static Token GetToken(ref string input)
        {
            input.TrimStart();

            char c = GetFirstCharAndTrimOff(ref input);

            if (char.IsLetter(c) || c == '_')
            {
                return LexIdentifier(ref input, c);
            }

            if (char.IsNumber(c))
            {
                return LexNumber(ref input, c);
            }

            if (IsBinop(c))
            {
                return new Token { Type = TokenType.BinaryOperator, Value = c + string.Empty };
            }

            if (IsRelop(c))
            {
                return LexRelop(ref input, c);
            }

            if (c == ':' && input.First() == '=')
            {
                GetFirstCharAndTrimOff(ref input);
                return new Token { Type = TokenType.Assignment, Value = ":=" };
            }

            if (c == '\"')
            {
                return LexString(ref input, c);
            }

            
            if (!IsParen(c))
            {
                throw new LexerException("not paren", 1);
            }

            return new Token
            {
                Type = c == '(' ? TokenType.LeftParenthesis : TokenType.RightParenthesis,
                Value = c + string.Empty
            };
        }

        /// <summary>
        /// Lexes a relational operator.
        /// </summary>
        private static Token LexRelop(ref string input, char c)
        {
            string tmp = string.Empty + c;

            char peek = input.First();
            if (peek == '=')
            {
                tmp += GetFirstCharAndTrimOff(ref input);
            }

            return new Token { Value = tmp, Type = TokenType.RelationalOperator };
        }

        /// <summary>
        /// Lexes a hard-coded string.
        /// </summary>
        private static Token LexString(ref string input, char c)
        {
            string tmp = string.Empty + c;

            do
            {
                c = GetFirstCharAndTrimOff(ref input);
                tmp += c;
            } while (c != '\"');

            return new Token { Value = tmp + '\"', Type = TokenType.String };
        }

        /// <summary>
        /// Lexes a number.
        /// </summary>
        private static Token LexNumber(ref string input, char c)
        {
            string numStr = string.Empty;
            int radixCount = 0;

            do
            {
                if (c == '.')
                {
                    radixCount++;
                }

                numStr += c;
                c = input.First();

                if (!char.IsNumber(c))
                {
                    break;
                }

                c = GetFirstCharAndTrimOff(ref input);
            } while (radixCount <= 1);

            if (numStr == ".")
            {
                throw new LexerException("whoops", 1);
            }

            if (c == 'e')
            {
                numStr += c;
                GetFirstCharAndTrimOff(ref input);
                return LexNumberWithExponent(ref input, numStr, c);
            }

            return new Token
            {
                Type = radixCount > 0 ? TokenType.Real : TokenType.Int,
                Value = numStr
            };
        }

        /// <summary>
        /// Lexes a number with an exponent, as per C specification.
        /// </summary>
        private static Token LexNumberWithExponent(ref string input, string numStr, char c)
        {
            bool isSigned = false;
            char peek;

            do
            {
                peek = input.First();
                if ((peek == '-' || peek == '+') && c == 'e')
                {
                    if (c == 'e')
                    {
                        isSigned = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if ((!char.IsDigit(peek) && !isSigned) || char.IsWhiteSpace(peek))
                {
                    break;
                }

                numStr += GetFirstCharAndTrimOff(ref input);
            } while (char.IsDigit(c) || (peek == '-' || peek == '+'));

            return new Token { Value = numStr, Type = TokenType.Real };
        }

        /// <summary>
        /// Handles parsing an identifier.  Valid C-style identifier rules.
        /// </summary>
        private static Token LexIdentifier(ref string input, char c)
        {
            string tmp = string.Empty;

            do
            {
                tmp += c;
                c = GetFirstCharAndTrimOff(ref input);
            } while (char.IsLetterOrDigit(c) || c == '_');

            // Need to place last char back in, lest we miss first character
            // of the next token.
            input = c + input;

            return new Token { Type = TokenType.Identifier, Value = tmp };
        }

        private static bool IsRelop(char c)
        {
            return c == '>' || c == '-' || c == '*' || c == '=';
        }

        private static bool IsBinop(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '^';
        }

        private static bool IsParen(char c)
        {
            return c == ')' || c == '(';
        }

        private static char GetFirstCharAndTrimOff(ref string input)
        {
            char c = input.First();
            input.Substring(1, input.Length - 1);
            return c;
        }
    }
}
