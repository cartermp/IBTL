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
                case TokenType.Statement:
                    HandleStatement(ref tokenStack, parentToken);
                    break;
            }
        }

        /// <summary>
        /// Delegates work to different Statement types.
        /// </summary>
        private void HandleStatement(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            switch (parentToken.Value.ToLower())
            {
                case "if":
                    HandleIfStatement(ref tokenStack, parentToken);
                    break;
                case "stdout":
                    HandleStdoutStatement(ref tokenStack, parentToken);
                    break;
            }
        }

        /// <summary>
        /// Handles generating gforth code for an (stdout (expr)) subtree.
        /// </summary>
        private void HandleStdoutStatement(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            var expression = tokenStack.Pop().Value + " type";
            tokenStack.Push(new SemanticToken
            {
                Type = TokenType.Statement,
                Value = expression
            });
        }

        /// <summary>
        /// Handles generating GForth code for an (if expr expr)|(if expr expr expr) subtree.
        /// </summary>
        private void HandleIfStatement(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            SemanticToken expr2 = tokenStack.Count == 3 ? tokenStack.Pop() : null;

            var expr1 = tokenStack.Pop();
            var predicate = tokenStack.Pop();

            if (predicate.Type != TokenType.Boolean)
            {
                throw new SemanticException(predicate + " does not evaluate to true or false.");
            }

            string expression = predicate.Value + " if " + expr1.Value + (expr2 != null ? " else " + expr2.Value : string.Empty) + " endif";
            tokenStack.Push(new SemanticToken
            {
                Type = TokenType.Statement,
                Value = expression
            });
        }

        /// <summary>
        /// Handles generating GForth code for a subtree whose root is a Unary Operator (non-relational).
        /// </summary>
        private void HandleUnaryOperator(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            var operand = tokenStack.Pop();

            if (IsATrigOperand(parentToken) && operand.Type != TokenType.Real)
            {
                operand.Value = ConvertNumberToGforthReal(operand);
            }

            if (operand.Type == TokenType.Real)
            {
                string number = ConvertNumberToGforthReal(operand);
                string subExpression = number + " " + "f" + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = IsAPredicate(parentToken) ? TokenType.Boolean : TokenType.Real,
                    Value = subExpression
                });
            }
            else if (operand.Type == TokenType.Int)
            {
                string subExpression = operand.Value + " " + parentToken.Value;
                tokenStack.Push(new SemanticToken
                {
                    Type = IsAPredicate(parentToken) ? TokenType.Boolean : TokenType.Int,
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

            if (parentToken.Value == "%")
            {
                parentToken.Value = "mod";
            }

            if (lhs.Type == TokenType.Real || rhs.Type == TokenType.Real)
            {
                string left = ConvertNumberToGforthReal(lhs);
                string right = ConvertNumberToGforthReal(rhs);

                if (parentToken.Value == "^")
                {
                    parentToken.Value = "**";
                }

                string subExpression = left + " " + right + " " + "f" + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = IsAPredicate(parentToken) ? TokenType.Boolean : TokenType.Real,
                    Value = subExpression
                });
            }
            else if (lhs.Type == TokenType.Int && rhs.Type == TokenType.Int)
            {
                var subExpression = lhs.Value + " " + rhs.Value + " " + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = IsAPredicate(parentToken) ? TokenType.Boolean : TokenType.Int,
                    Value = subExpression
                });
            }
            else if (lhs.Type == TokenType.String || rhs.Type == TokenType.String)
            {
                HandleStringConcatenation(tokenStack, parentToken, rhs, lhs);
            }
            else if (IsAPredicate(parentToken))
            {
                if (lhs.Type == TokenType.Real)
                {
                    lhs.Value = ConvertNumberToGforthReal(lhs);
                }

                if (rhs.Type == TokenType.Real)
                {
                    rhs.Value = ConvertNumberToGforthReal(rhs);
                }

                string expression = lhs.Value + " " + rhs.Value + " " + parentToken.Value;
                tokenStack.Push(new SemanticToken
                {
                    Type = TokenType.Boolean,
                    Value = expression
                });
            }
        }

        /// <summary>
        /// Generates GForth String Concatenation code.
        /// </summary>
        private void HandleStringConcatenation(Stack<SemanticToken> tokenStack, Token parentToken, SemanticToken rhs, SemanticToken lhs)
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

        /// <summary>
        /// Converts a numeric token into a GForth real.
        /// </summary>
        private string ConvertNumberToGforthReal(SemanticToken token)
        {
            return token.Type == TokenType.Int ? token.Value + " s>f"
                : !token.Value.ToLower().Contains("e") ? token.Value + "e" : token.Value;
        }

        /// <summary>
        /// Converts a string token into a GForth string.
        /// </summary>
        private string ConvertStringToGforthString(SemanticToken token)
        {
            return string.Format("s\" {0}\"", token.Value.Substring(1, token.Value.Length - 3));
        }

        /// <summary>
        /// Determines if a token is a trig operator.
        /// </summary>
        private bool IsATrigOperand(Token token)
        {
            string val = token.Value.ToLower();
            return val == "sin" || val == "cos" || val == "tan";
        }

        // <summary>
        /// Determines if the given token represents a predicate expression.
        /// </summary>
        private bool IsAPredicate(Token token)
        {
            string val = token.Value.ToLower();
            return val == "not" || val == "and" || val == "or" ||
                   val == ">" || val == "<" || val == "==" ||
                   val == "!=" || val == ">=" || val == "<=";
        }
    }
}
