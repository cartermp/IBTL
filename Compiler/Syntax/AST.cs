using Compiler.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Syntax
{
    /// <summary>
    /// The AST for IBTL itself, represented as a List of ASTNodes.
    /// Conceptually, this is an n-ary tree with a sentinel as its root.
    /// </summary>
    public class AST
    {
        private List<ASTNode> mNodes;

        /// <summary>
        /// Initializes a new AST.
        /// </summary>
        public AST() : this(new List<ASTNode>()) { }

        /// <summary>
        /// Initializes a new AST with given AST Nodes.
        /// </summary>
        public AST(List<ASTNode> nodes)
        {
            mNodes = nodes;
        }

        public void Add(ASTNode node)
        {
            mNodes.Add(node);
        }

        /// <summary>
        /// Returns the last child Node in the AST.
        /// </summary>
        public ASTNode Back()
        {
            return mNodes.Last();
        }

        /// <summary>
        /// Converts the AST to Gforth code.
        /// </summary>
        public string ToGforth()
        {
            // Defining our own integer power function here so that we don't
            // have to incorporate any int->float->int casting fuckery.
            string gforthIntegerPower = ":^ 1 swap 0 u+do over * loop nip ; \n\n";

            Stack<SemanticToken> tokenStack = new Stack<SemanticToken>();

            mNodes.ForEach(node => Walk(node, ref tokenStack));

            return gforthIntegerPower + tokenStack.Pop().Value + " CR";
        }

        /// <summary>
        /// Performs a Postorder walk of the AST, generating Gforth code.
        /// </summary>
        private void Walk(ASTNode node, ref Stack<SemanticToken> tokenStack)
        {
            if (node.Children == null || !node.Children.Any())
            {
                var semanticToken = new SemanticToken
                {
                    Type = node.Token.Type,
                    Value = node.Token.Value
                };

                tokenStack.Push(semanticToken);
                return;
            }
            else
            {
                foreach (var child in node.Children)
                {
                    Walk(child, ref tokenStack);
                }
            }

            var parentToken = node.Token;

            switch (parentToken.Type)
            {
                case TokenType.BinaryOperator:
                    HandleBinaryOperator(ref tokenStack, parentToken);
                    break;
                case TokenType.UnaryOperator:
                    HandleUnaryOperator(ref tokenStack, parentToken);
                    break;
            }
        }

        /// <summary>
        /// Handles generating GForth code for a subtree whose root is a Unary Operator (non-relational).
        /// </summary>
        private void HandleUnaryOperator(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            var operand = tokenStack.Pop();

            if (IsATrigOperand(parentToken) && operand.Type != TokenType.Real)
            {
                throw new SemanticException("Trig operators only work on reals.");
            }

            if (operand.Type == TokenType.Real)
            {
                string number = ConvertNumberToGforthReal(operand);
                string subExpression = number + " " + "f" + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = TokenType.Real,
                    Value = subExpression
                });
            }
            else if (operand.Type == TokenType.Int)
            {
                string subExpression = operand.Value + " " + parentToken.Value;
                tokenStack.Push(new SemanticToken
                {
                    Type = TokenType.Real,
                    Value = subExpression
                });
            }
        }

        /// <summary>
        /// Handles generating Gforth code for a subtree whose root is a Binary Operator.
        /// </summary>
        private void HandleBinaryOperator(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            var rhs = tokenStack.Pop();
            var lhs = tokenStack.Pop();

            if (lhs.Type == TokenType.Real || rhs.Type == TokenType.Real)
            {
                string left = ConvertNumberToGforthReal(lhs);
                string right = ConvertNumberToGforthReal(rhs);

                string subExpression = left + " " + right + " " + "f" + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = TokenType.Real,
                    Value = subExpression
                });
            }
            else if (lhs.Type == TokenType.Int && rhs.Type == TokenType.Int)
            {
                var subExpression = lhs.Value + " " + rhs.Value + " " + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = TokenType.Int,
                    Value = subExpression
                });
            }
            else if (lhs.Type == TokenType.String || rhs.Type == TokenType.String)
            {
                if (parentToken.Value != "+")
                {
                    throw new SemanticException("Strings can only be concatenated.  " + parentToken.Value + " is not a valid operator for strings.");
                }

                if (lhs.Type != TokenType.String || rhs.Type != TokenType.String)
                {
                    throw new SemanticException("Strings cannot be concatenated by non-strings.");
                }

                string left = ConvertStringToGforthString(lhs);
                string right = ConvertStringToGforthString(rhs);

                string subExpression = left + " " + right + " " + "s" + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = TokenType.String,
                    Value = subExpression
                });
            }
        }

        /// <summary>
        /// Converts a numeric token into a GForth real.
        /// </summary>
        private string ConvertNumberToGforthReal(SemanticToken token)
        {
            return token.Type == TokenType.Int ? token.Value + " s>f"
                : !token.Value.Contains("e") ? token.Value + "e" : token.Value;
        }

        /// <summary>
        /// Converts a string token into a GForth string.
        /// </summary>
        private string ConvertStringToGforthString(SemanticToken token)
        {
            return "s\" " + token.Value + "\"";
        }

        /// <summary>
        /// Determines if a token is a trig operator.
        /// </summary>
        private bool IsATrigOperand(Token token)
        {
            string val = token.Value;
            return val == "sin" || val == "cos" || val == "tan";
        }
    }
}
