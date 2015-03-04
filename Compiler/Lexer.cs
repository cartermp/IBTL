using Compiler.Exceptions;
using Compiler.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Lexer
    {
        private Dictionary<string, Token> m_table = new Dictionary<string, Token>();
        private Token m_peekedToken;

        /// <summary>
        /// Gets an IBTL token.  Uses the peeked token if available; otherwise, extracts from the input string.
        /// </summary>
        public Token GetToken(ref string input)
        {
            m_peekedToken = null;
            return PeekToken(ref input);
        }

        /// <summary>
        /// Extracts a token from the input string, storing it in the temporary peek buffer.
        /// </summary>
        public Token PeekToken(ref string input)
        {
            m_peekedToken = GetTokenImpl(ref input);
            return m_peekedToken;
        }

        /// <summary>
        /// Top-level function.  Returns the next token and delegates work.
        /// Trims the input string as necessary.
        /// </summary>
        private Token GetTokenImpl(ref string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            input = input.TrimStart();

            char c = GetFirstCharAndTrimOff(ref input);

            if (char.IsLetter(c) || c == '_')
            {
                return LexIdentifier(ref input, c);
            }

            if (char.IsNumber(c))
            {
                return LexNumber(ref input, c);
            }

            if (c == '-')
            {
                return new Token { Type = TokenType.Minus, Value = "-" };
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
        public void PreloadTable()
        {
            m_table.Add("true", new Token { Type = TokenType.True, Value = "true" });
            m_table.Add("false", new Token { Type = TokenType.False, Value = "false" });

            m_table.Add("and", new Token { Type = TokenType.BinaryOperator, Value = "and" });
            m_table.Add("or", new Token { Type = TokenType.BinaryOperator, Value = "or" });

            m_table.Add("not", new Token { Type = TokenType.UnaryOperator, Value = "not" });

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
        private Token LexRelop(ref string input, char c)
        {
            string tmp = string.Empty + c;

            char peek = input.First();
            if (peek == '=')
            {
                tmp += GetFirstCharAndTrimOff(ref input);
            }

            return new Token { Value = tmp, Type = TokenType.BinaryOperator };
        }

        /// <summary>
        /// Lexes a hard-coded string.
        /// </summary>
        private Token LexString(ref string input, char c)
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
        private Token LexNumber(ref string input, char c)
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
                c = input.FirstOrDefault();

                if (!char.IsNumber(c) && c != '.')
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
        private Token LexNumberWithExponent(ref string input, string numStr, char c)
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
        private Token LexIdentifier(ref string input, char c)
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

            if (m_table.TryGetValue(tmp, out t))
            {
                return t;
            }

            return new Token { Type = TokenType.Identifier, Value = tmp };
        }

        private bool IsRelop(char c)
        {
            return c == '>' || c == '-' || c == '*' || c == '=';
        }

        private bool IsBinop(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '^';
        }

        private bool IsParen(char c)
        {
            return c == ')' || c == '(';
        }

        private char GetFirstCharAndTrimOff(ref string input)
        {
            char c = input.First();
            input = input.Substring(1, input.Length - 1);
            return c;
        }
    }
}
