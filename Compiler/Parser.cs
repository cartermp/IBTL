using Compiler.Exceptions;
using Compiler.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Parser
    {
        private Lexer m_lexer = new Lexer();

        /// <summary>
        /// Top-level parsing function.
        /// </summary>
        public AST Parse(string fileName)
        {
            string contents = GetFileContents(fileName);

            AST ast = new AST();

            var lastToken = m_lexer.GetToken(ref contents);

            ASTNode node = new ASTNode();

            while (m_lexer.PeekToken(ref contents) != null)
            {
                ParseExpression(lastToken, ref contents, node);
                ast.Add(node);
            }

            return ast;
        }

        /// <summary>
        /// Recursively parses IBTL token-by-token.
        /// </summary>
        private void ParseExpression(Token lastToken, ref string contents, ASTNode node)
        {
            if (lastToken.Type == TokenType.LeftParenthesis)
            {
                lastToken = m_lexer.GetToken(ref contents);
                ParseExpression(lastToken, ref contents, node.BackChild());
            }

            while (lastToken.Type != TokenType.LeftParenthesis && lastToken.Type != TokenType.RightParenthesis)
            {
                lastToken = m_lexer.GetToken(ref contents);
                ParseInner(lastToken, ref contents, node);
            }

            if (lastToken.Type == TokenType.LeftParenthesis)
            {
                lastToken = m_lexer.GetToken(ref contents);
                ParseExpression(lastToken, ref contents, node);
            }
            else if (lastToken.Type != TokenType.RightParenthesis)
            {
                throw new ParserException("Expression needs to end with ')'");
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
                case TokenType.RelationalOperator:
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
                    node.Add(lastToken);
                    break;
                case TokenType.Identifier:
                    node.Add(lastToken);
                    break;
                case TokenType.Statement:
                    node.Add(lastToken);
                    ParseStatement(node, ref contents, lastToken);
                    break;
                default:
                    throw new ParserException("Unrecognized token.");
            }
        }

        /// <summary>
        /// Parses an IBTL Statement.
        /// </summary>
        private void ParseStatement(ASTNode node, ref string contents, Token lastToken)
        {
            node.Add(lastToken);
            switch (lastToken.Value)
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
        }

        /// <summary>
        /// Parses an IBTL Let form (let (varlist)) 
        /// </summary>
        private void ParseLetStatement(ASTNode node, ref string contents)
        {
            var lastToken = m_lexer.GetToken(ref contents);

            if (lastToken.Type == TokenType.RightParenthesis)
            {
                throw new ParserException("(let (varlist)) form not matched.");
            }

            do
            {
                lastToken = m_lexer.GetToken(ref contents);
                ParseVar(node, lastToken, ref contents);
            } while (lastToken.Type != TokenType.RightParenthesis);
        }

        /// <summary>
        /// Parses a "var" for an IBTL let statement.
        /// </summary>
        private void ParseVar(ASTNode node, Token lastToken, ref string contents)
        {
            if (lastToken.Type != TokenType.LeftParenthesis)
            {
                throw new ParserException("let statement not in (let (varlist)) form.");
            }

            // First attempt to add an identifier.

            lastToken = m_lexer.GetToken(ref contents);
            if (lastToken.Type != TokenType.Identifier)
            {
                throw new ParserException("let statement not in (let (varlist)) form.");
            }

            node.Add(lastToken);

            // Now attempt to add a type.

            lastToken = m_lexer.GetToken(ref contents);
            if (lastToken.Type != TokenType.Type)
            {
                throw new ParserException("let statement not in (let (varlist)) form.");
            }

            node.Add(lastToken);

            lastToken = m_lexer.PeekToken(ref contents);
            if (lastToken.Type != TokenType.RightParenthesis)
            {
                throw new ParserException("let statement not in (let (varlist)) form.");
            }

            // Get rid of the Right Parenthesis in this scope.
            m_lexer.GetToken(ref contents);
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
                lastToken = m_lexer.GetToken(ref contents);
                ParseStandardExpression(node, lastToken, ref contents);
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

            lastToken = m_lexer.PeekToken(ref contents);
            if (lastToken != null && lastToken.Type != TokenType.RightParenthesis)
            {
                lastToken = m_lexer.GetToken(ref contents);
                ParseStandardExpression(node, lastToken, ref contents);

                lastToken = m_lexer.PeekToken(ref contents);
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

            node.Add(last);

            last = m_lexer.GetToken(ref contents);

            ParseOper(node, last, ref contents);
        }

        /// <summary>
        /// Parses the oper IBTL form.
        /// </summary>
        private void ParseOper(ASTNode node, Token last, ref string contents)
        {
            switch (last.Type)
            {
                case TokenType.LeftParenthesis:
                    ParseExpression(last, ref contents, node);
                    break;
                case TokenType.Int:
                case TokenType.Real:
                case TokenType.Identifier:
                case TokenType.String:
                    node.Add(last);
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
                    ParseExpression(lastToken, ref contents, node);
                    break;
                case TokenType.Int:
                case TokenType.Real:
                case TokenType.Identifier:
                case TokenType.String:
                    node.Add(lastToken);
                    break;
                case TokenType.Statement:
                    ParseStatement(node, ref contents, lastToken);
                    break;
                default:
                    throw new ParserException("expression form not matched.");
            }
        }

        /// <summary>
        /// Reads a file and outputs a string of its contents.
        /// </summary>
        private string GetFileContents(string filename)
        {
            return string.Join("", File.ReadAllLines(filename).SelectMany(c => c));
        }
    }
}
