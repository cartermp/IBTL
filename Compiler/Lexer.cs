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
        private static Dictionary<string, Token> m_table = new Dictionary<string, Token>();

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
        /// Preloads the symbol table with some known tokens.
        /// </summary>
        private static void PreloadTable()
        {
            m_table.Add("true", new Token{ Type = TokenType.True, Value = "true"});
            m_table.Add("false", new Token { Type = TokenType.False, Value = "false" });

            m_table.Add("and", new Token { Type = TokenType.RelationalOperator, Value = "and" });
            m_table.Add("or", new Token { Type = TokenType.RelationalOperator, Value = "or" });

            m_table.Add("not", new Token { Type = TokenType.UnaryRelationalOperator, Value = "not" });

            m_table.Add("sin", new Token { Type = TokenType.UnaryOperator, Value = "sin" });
            m_table.Add("cos", new Token { Type = TokenType.UnaryOperator, Value = "cos" });
            m_table.Add("tan", new Token { Type = TokenType.UnaryOperator, Value = "tan" });

            m_table.Add("int", new Token { Type = TokenType.Type, Value = "int" });
            m_table.Add("real", new Token { Type = TokenType.Type, Value = "real" });
            m_table.Add("bool", new Token { Type = TokenType.Type, Value = "bool" });
            m_table.Add("string", new Token { Type = TokenType.Type, Value = "string" });

            m_table.Add("let", new Token { Type = TokenType.Statement, Value = "let" });
            m_table.Add("stdout", new Token { Type = TokenType.Statement, Value = "stdout" });
            m_table.Add("while", new Token { Type = TokenType.Statement, Value = "while" });
            m_table.Add("if", new Token { Type = TokenType.Statement, Value = "if" });
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

            Token t;

            if (m_table.TryGetValue(input, out t))
            {
                return t;
            }

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
