using Compiler.Exceptions;
using Compiler.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Compiler
{
    /// <summary>
    /// Represents the parsing stage of the IBTL compiler.
    /// 
    /// Fundamentally, this is a recursive-descent LL(1) parser.
    /// 
    /// Realistically, this is some recursive code that I threw together
    /// in a few days because I didn't want to follow the asinine grammar
    /// we were given.
    /// </summary>
    public class Parser
    {
        // fuck it, make it public
        public Lexer m_lexer = new Lexer();

        /// <summary>
        /// Top-level parsing function.
        /// </summary>
        public AST Parse(string expression)
        {
            AST ast = new AST();

            m_lexer.PreloadTable();

            while (!string.IsNullOrWhiteSpace(expression))
            {
                ast.Add(new ASTNode());
                ParseExpression(ref expression, ast.Back());
            }

            return ast;
        }

        /// <summary>
        /// Recursively parses IBTL token-by-token.
        /// </summary>
        private void ParseExpression(ref string contents, ASTNode node)
        {
            var lastToken = m_lexer.GetToken(ref contents);

            if (lastToken.Type == TokenType.LeftParenthesis)
            {
                if (!string.IsNullOrWhiteSpace(contents))
                {
                    ParseExpression(ref contents, node.Children == null ? node : node.BackChild());
                    lastToken = m_lexer.LastLexedToken;
                }
            }
            else if (lastToken != null && lastToken.Type != TokenType.LeftParenthesis && lastToken.Type != TokenType.RightParenthesis)
            {
                ParseInner(lastToken, ref contents, node);
                return;
            }

            // hack hack hack hack hack hack hack hack hack hack hack
            if (lastToken == null)
            {
                return;
            }

            if (lastToken.Type == TokenType.LeftParenthesis)
            {
                if (!string.IsNullOrWhiteSpace(contents))
                {
                    ParseExpression(ref contents, node.Children == null ? node : node.BackChild());
                    return;
                }
            }
            else if (lastToken != null && lastToken.Type != TokenType.RightParenthesis)
            {
                throw new ParserException("Mismatched parenthesis");
            }
        }

        /// <summary>
        /// Parses the inner part of an IBTL expression ((([parses_this_stuff])))
        /// </summary>
        private void ParseInner(Token lastToken, ref string contents, ASTNode node)
        {
            switch (lastToken.Type)
            {
                case TokenType.Assignment:
                    node.Add(lastToken);
                    ParseAssign(node, ref contents);
                    break;
                case TokenType.BinaryOperator:
                    node.Add(lastToken);
                    ParseBinaryOperator(node, ref contents);
                    break;
                case TokenType.UnaryOperator:
                    node.Add(lastToken);
                    ParseUnaryOperator(node, ref contents);
                    break;
                case TokenType.Int:
                case TokenType.Real:
                case TokenType.String:
                case TokenType.Identifier:
                case TokenType.True:
                case TokenType.False:
                    node.Add(lastToken);
                    break;
                case TokenType.Statement:
                    node.Add(lastToken);
                    ParseStatement(node, ref contents, lastToken);
                    break;
                case TokenType.Minus:
                    node.Add(lastToken);
                    ParseMinus(node, ref contents);
                    break;
                default:
                    throw new ParserException("Unrecognized token.");
            }
        }

        /// <summary>
        /// Parses a minus operator as either a binary or unary operator, depending on context.
        /// </summary>
        private void ParseMinus(ASTNode node, ref string contents, Token parentToken = null)
        {
            var last = m_lexer.GetToken(ref contents);
            ParseOper(node, last, ref contents);

            // If there's a parent token and it's an operator (or unkown operator in the case of another Minus),
            // We treat the current Minus token as unary.  Treating it as Binary would interfere with a parent's
            // potential operands.
            //
            // Or a statement, apparently.
            if (parentToken != null && (parentToken.Type == TokenType.BinaryOperator || parentToken.Type == TokenType.UnaryOperator || 
                                        parentToken.Type == TokenType.Minus || parentToken.Type == TokenType.Statement))
            {
                node.Token.Type = TokenType.UnaryOperator;
                return;
            }

            last = m_lexer.GetToken(ref contents);
            if (last != null && last.Type != TokenType.RightParenthesis)
            {
                ParseOper(node, last, ref contents);
                node.Token.Type = TokenType.BinaryOperator;
            }
            else
            {
                node.Token.Type = TokenType.UnaryOperator;
            }

            last = m_lexer.GetToken(ref contents);
            if (last.Type != TokenType.RightParenthesis)
            {
                throw new ParserException("Mismatches parenthesis in minus");
            }
        }

        /// <summary>
        /// Parses an IBTL Statement.
        /// </summary>
        private void ParseStatement(ASTNode node, ref string contents, Token lastToken)
        {
            switch (lastToken.Value.ToLower())
            {
                case "if":
                    ParseIfStatement(node, ref contents);
                    break;
                case "while":
                    ParseWhileStatement(node, ref contents);
                    break;
                case "let":
                    ParseLetStatement(node, ref contents);
                    break;
                case "stdout":
                    ParseStdout(node, ref contents);
                    break;
                default:
                    throw new ParserException("Unrecognized Statement: " + lastToken.Value);
            }
        }

        /// <summary>
        /// Parses an IBTL stdout (stdout oper) statement.
        /// </summary>
        private void ParseStdout(ASTNode node, ref string contents)
        {
            var lastToken = m_lexer.GetToken(ref contents);
            ParseOper(node, lastToken, ref contents);

            lastToken = m_lexer.GetToken(ref contents);
            if (lastToken.Type != TokenType.RightParenthesis)
            {
                throw new ParserException("Mismatched parenthesis in stdout statement.");
            }
        }

        /// <summary>
        /// Parses an IBTL Let form (let (varlist)) 
        /// </summary>
        private void ParseLetStatement(ASTNode node, ref string contents)
        {
            Token lastToken = null;
            do
            {
                lastToken = m_lexer.GetToken(ref contents);
                if (lastToken == null)
                {
                    throw new ParserException("mismatched parenthesis.");
                }

                if (lastToken.Type == TokenType.RightParenthesis)
                {
                    break;
                }

                if (lastToken.Type == TokenType.LeftParenthesis)
                {
                    ParseVar(node, lastToken, ref contents);
                }
                else
                {
                    throw new ParserException("varlist in let statement did not being with lparen.");
                }
            } while (lastToken != null && lastToken.Type != TokenType.RightParenthesis);
        }

        /// <summary>
        /// Parses a "var" for an IBTL let statement.
        /// </summary>
        private void ParseVar(ASTNode node, Token lastToken, ref string input)
        {
            int parenCount = 0;

            // Trim off parenthesis, keeping track of the nesting level
            // so that stupid people can nest expressions.
            while (lastToken.Type == TokenType.LeftParenthesis)
            {
                parenCount++;
                lastToken = m_lexer.GetToken(ref input);
            }

            // First attempt to add an identifier.
            if (lastToken.Type != TokenType.Identifier)
            {
                throw new ParserException("let statement not in (let (varlist)) form.");
            }

            node.AddToChildren(lastToken);

            // Now attempt to add a type.
            lastToken = m_lexer.GetToken(ref input);
            if (lastToken.Type != TokenType.IntType && lastToken.Type != TokenType.RealType && lastToken.Type != TokenType.BoolType && lastToken.Type != TokenType.StringType)
            {
                throw new ParserException("let statement not in (let (varlist)) form.");
            }

            node.Children.Last().AddToChildren(lastToken);

            while (parenCount-- > 0)
            {
                lastToken = m_lexer.GetToken(ref input);
                if (lastToken == null || lastToken.Type != TokenType.RightParenthesis)
                {
                    throw new ParserException("unmatched parenthesis inside varlist.");
                }
            }
        }

        /// <summary>
        /// Parses an IBTL While form (while expr exprlist)
        /// </summary>
        private void ParseWhileStatement(ASTNode node, ref string contents)
        {
            var lastToken = m_lexer.GetToken(ref contents);
            ParseStandardExpression(node, lastToken, ref contents);

            lastToken = m_lexer.GetToken(ref contents);
            if (lastToken.Type == TokenType.RightParenthesis)
            {
                throw new ParserException("while statement not in (while exp exprlist)");
            }

            do
            {
                ParseStandardExpression(node, lastToken, ref contents);
                lastToken = m_lexer.GetToken(ref contents);
            } while (lastToken.Type != TokenType.RightParenthesis);
        }

        /// <summary>
        /// Parses the IBTL if form (if expr expr expr)
        /// </summary>
        private void ParseIfStatement(ASTNode node, ref string contents)
        {
            var lastToken = m_lexer.GetToken(ref contents);
            ParseStandardExpression(node, lastToken, ref contents);

            lastToken = m_lexer.GetToken(ref contents);
            ParseStandardExpression(node, lastToken, ref contents);

            lastToken = m_lexer.GetToken(ref contents);
            if (lastToken != null && lastToken.Type != TokenType.RightParenthesis)
            {
                ParseStandardExpression(node, lastToken, ref contents);

                lastToken = m_lexer.GetToken(ref contents);
                if (lastToken.Type != TokenType.RightParenthesis)
                {
                    throw new ParserException("If statement not in (if expr expr expr) or (if expr expr) form.");
                }
            }
        }

        /// <summary>
        /// Parses the unary operator IBTL form.  (op oper).
        /// </summary>
        private void ParseUnaryOperator(ASTNode node, ref string contents)
        {
            var last = m_lexer.GetToken(ref contents);
            ParseOper(node, last, ref contents);

            last = m_lexer.GetToken(ref contents);
            if (last.Type != TokenType.RightParenthesis)
            {
                throw new ParserException("Mismatched parenethesis in binary operator.");
            }
        }

        /// <summary>
        /// Parses the binary operator IBTL form. (op oper oper).
        /// </summary>
        /// <param name="node"></param>
        /// <param name="contents"></param>
        private void ParseBinaryOperator(ASTNode node, ref string contents)
        {
            var last = m_lexer.GetToken(ref contents);

            ParseOper(node, last, ref contents);

            last = m_lexer.GetToken(ref contents);

            ParseOper(node, last, ref contents);

            last = m_lexer.GetToken(ref contents);
            if (last != null && last.Type != TokenType.RightParenthesis)
            {
                throw new ParserException("Mismatched parenethesis in binary operator.");
            }
        }

        /// <summary>
        /// Parses the Assignment (:= indent oper) IBTL Form.
        /// </summary>
        private void ParseAssign(ASTNode node, ref string contents)
        {
            var last = m_lexer.GetToken(ref contents);
            if (last.Type != TokenType.Identifier)
            {
                throw new ParserException(":= must be followed by an identifier.");
            }

            node.AddToChildren(last);

            last = m_lexer.GetToken(ref contents);

            ParseOper(node, last, ref contents);

            last = m_lexer.GetToken(ref contents);
            if (last.Type != TokenType.RightParenthesis)
            {
                throw new ParserException("Mismatched parenethesis in binary operator.");
            }
        }

        /// <summary>
        /// Parses the oper IBTL form.
        /// </summary>
        private void ParseOper(ASTNode node, Token last, ref string contents)
        {
            switch (last.Type)
            {
                case TokenType.LeftParenthesis:
                    last = m_lexer.GetToken(ref contents);
                    
                    node.Children = node.Children ?? new List<ASTNode>();
                    node.Children.Add(new ASTNode(last));

                    ParseInner(last, ref contents, node.Children.Last());
                    break;
                case TokenType.Int:
                case TokenType.Real:
                case TokenType.Identifier:
                case TokenType.String:
                case TokenType.True:
                case TokenType.False:
                    node.AddToChildren(last);
                    break;
                case TokenType.Minus:
                    node.AddToChildren(last);
                    ParseMinus(node.Children.Last(), ref contents, node.Token);
                    break;
                default:
                    throw new ParserException("Expression form not matched.");
            }
        }

        /// <summary>
        /// Parses a "standard" IBTL expression.
        /// </summary>
        private void ParseStandardExpression(ASTNode node, Token lastToken, ref string contents)
        {
            switch (lastToken.Type)
            {
                case TokenType.LeftParenthesis:
                    lastToken = m_lexer.GetToken(ref contents);

                    node.Children = node.Children ?? new List<ASTNode>();
                    node.Children.Add(new ASTNode(lastToken));

                    ParseInner(lastToken, ref contents, node.Children.Last());
                    break;
                case TokenType.Int:
                case TokenType.Real:
                case TokenType.Identifier:
                case TokenType.String:
                case TokenType.True:
                case TokenType.False:
                    node.AddToChildren(lastToken);
                    break;
                case TokenType.Statement:
                    ParseStatement(node, ref contents, lastToken);
                    break;
                case TokenType.Minus:
                    node.AddToChildren(lastToken);
                    ParseMinus(node.BackChild(), ref contents, node.Token);
                    break;
                default:
                    throw new ParserException("expression form not matched.");
            }
        }
    }
}
