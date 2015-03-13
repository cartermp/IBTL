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
        private Dictionary<string, Tuple<Token, TokenType>> m_table = new Dictionary<string, Tuple<Token, TokenType>>();

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

        public void AddSymbols(Dictionary<string, Token> symbolTable)
        {
            if (symbolTable != null)
            {
                foreach (var item in symbolTable.Keys)
                {
                    // Because we do not know the type of identifiers which aren't registered keywords,
                    // we label them as undefined.  They are defined when a "let" statement binds them.
                    m_table.Add(item, Tuple.Create(symbolTable[item], TokenType.Undefined));
                }
            }
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
            if (mNodes == null || !mNodes.Any() || mNodes.First() == null || mNodes.First().Token == null)
            {
                return string.Empty;
            }

            // Defining our own integer power function here so that we don't
            // have to incorporate any int->float->int casting fuckery.
            string gforthIntegerPower = ":^ 1 swap 0 u+do over * loop nip ; \n\n";

            Stack<SemanticToken> tokenStack = new Stack<SemanticToken>();

            mNodes.ForEach(node => Walk(node, ref tokenStack));

            return gforthIntegerPower + string.Join(" CR\n", tokenStack.Reverse().Select(s => s.Value)) + " CR";
        }

        /// <summary>
        /// Performs a Postorder walk of the AST, generating Gforth code.  Does so by flattening
        /// each subtree with a stack.
        /// </summary>
        private void Walk(ASTNode node, ref Stack<SemanticToken> tokenStack)
        {
            if (node.Children == null || !node.Children.Any())
            {
                tokenStack.Push(new SemanticToken
                {
                    Type = node.Token.Type,
                    Value = node.Token.Value
                });

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
                    HandleStatement(ref tokenStack, parentToken, node.Children.Count);
                    break;
                case TokenType.Assignment:
                    HandleAssignmentOperator(ref tokenStack, parentToken);
                    break;
                case TokenType.Identifier:
                    // This is for let statements.
                    tokenStack.Push(new SemanticToken
                    {
                        Type = node.Token.Type,
                        Value = node.Token.Value
                    });

                    break;
            }
        }

        /// <summary>
        /// Handles generating gforth code for an assignment operator (:= identifier (oper)).
        /// </summary>
        private void HandleAssignmentOperator(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            var expression = tokenStack.Pop();
            string identifier = tokenStack.Pop().Value;

            Tuple<Token, TokenType> val;
            if (m_table.TryGetValue(identifier, out val))
            {
                if (val.Item2 == TokenType.Undefined)
                {
                    throw new SemanticException(identifier + " is unbound.");
                }
            }
            else
            {
                throw new SemanticException(identifier + " is unrecognized.");
            }

            string expr = expression.Value + " " + identifier + " " + ((expression.Type == TokenType.Real) ? "f" : string.Empty) + "!";

            tokenStack.Push(new SemanticToken
            {
                Type = expression.Type,
                Value = expr
            });
        }

        /// <summary>
        /// Delegates work to different Statement types.
        /// </summary>
        private void HandleStatement(ref Stack<SemanticToken> tokenStack, Token parentToken, int numChildren)
        {
            switch (parentToken.Value.ToLower())
            {
                case "if":
                    HandleIfStatement(ref tokenStack, parentToken);
                    break;
                case "stdout":
                    HandleStdoutStatement(ref tokenStack, parentToken);
                    break;
                case "while":
                    HandleWhileStatement(ref tokenStack, parentToken, numChildren);
                    break;
                case "let":
                    HandleLetStatement(ref tokenStack, parentToken, numChildren);
                    break;
            }
        }

        /// <summary>
        /// Generates gforth code corresponding to a (let (varlist)) statement.
        /// </summary>
        private void HandleLetStatement(ref Stack<SemanticToken> tokenStack, Token parentToken, int numChildren)
        {
            var semanticTokens = new List<SemanticToken>();

            while (numChildren-- > 0)
            {
                var identifier = tokenStack.Pop();
                var type = tokenStack.Pop();

                Tuple<Token, TokenType> tmp;
                if (!m_table.TryGetValue(identifier.Value, out tmp))
                {
                    throw new SemanticException(identifier.Value + " not recognized.");
                }

                // We need to give the Identifier Token its bound type.
                m_table[identifier.Value] = Tuple.Create(tmp.Item1, type.Type);

                semanticTokens.Add(new SemanticToken
                {
                    Type = TokenType.Identifier,
                    Value = "variable " + identifier.Value
                });
            }

            foreach (var token in semanticTokens)
            {
                tokenStack.Push(token);
            }
        }

        /// <summary>
        /// Handles generating gforth code for a while (while (predicate) (exprlist)) subtree.
        /// </summary>
        private void HandleWhileStatement(ref Stack<SemanticToken> tokenStack, Token parentToken, int numChildren)
        {
            var expressionList = new List<SemanticToken>();
            while (expressionList.Count < numChildren - 1)
            {
                expressionList.Add(tokenStack.Pop());
            }

            // Due to the effects of being on the stack, these need to be back in proper order.
            expressionList.Reverse();

            var predicate = tokenStack.Pop();

            string whileExpression = "begin " + predicate.Value + " while " + string.Join(" ", expressionList.Select(e => e.Value)) + " repeat";

            tokenStack.Push(new SemanticToken
            {
                Type = TokenType.Statement,
                Value = whileExpression
            });
        }

        /// <summary>
        /// Handles generating gforth code for an (stdout (expr)) subtree.
        /// </summary>
        private void HandleStdoutStatement(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            string stdoutExpression = string.Empty;

            var token = tokenStack.Pop();
            switch (token.Type)
            {
                case TokenType.Int:
                    stdoutExpression = token.Value + " .";
                    break;
                case TokenType.Real:
                    stdoutExpression = token.Value + " f.";
                    break;
                case TokenType.String:
                    stdoutExpression = token.Value + " type";
                    break;
            }

            tokenStack.Push(new SemanticToken
            {
                Type = TokenType.Statement,
                Value = stdoutExpression
            });
        }

        /// <summary>
        /// Handles generating GForth code for an (if expr expr)|(if expr expr expr) subtree.
        /// </summary>
        private void HandleIfStatement(ref Stack<SemanticToken> tokenStack, Token parentToken)
        {
            SemanticToken expr2 = tokenStack.Count >= 3 ? tokenStack.Pop() : null;

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

            if (parentToken.Value == "-")
            {
                parentToken.Value = "negate";
            }

            if (IsATrigOperand(parentToken) && operand.Type != TokenType.Real && operand.Type != TokenType.Identifier)
            {
                operand.Value = ConvertTokenToGforthReal(operand);
            }

            if (operand.Type == TokenType.Real)
            {
                string number = ConvertTokenToGforthReal(operand);
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
            else if (operand.Type == TokenType.Identifier)
            {
                HandleIdentifierForUnary(ref tokenStack, parentToken, operand);
            }
        }

        /// <summary>
        /// Generates gforth code for when an identifier is used with a unary operator.
        /// </summary>
        private void HandleIdentifierForUnary(ref Stack<SemanticToken> tokenStack, Token parentToken, SemanticToken operand)
        {
            Tuple<Token, TokenType> item;
            if (m_table.TryGetValue(operand.Value, out item))
            {
                if (item.Item2 == TokenType.Undefined)
                {
                    throw new SemanticException(operand.Value + " is unbound identifier.");
                }

                if (operand.Type == TokenType.Real)
                {
                    string number = ConvertTokenToGforthReal(operand);
                    string subExpression = number + " @ " + "f" + parentToken.Value;

                    tokenStack.Push(new SemanticToken
                    {
                        Type = IsAPredicate(parentToken) ? TokenType.Boolean : TokenType.Real,
                        Value = subExpression
                    });
                }
                else if (operand.Type == TokenType.Int)
                {
                    string subExpression = operand.Value + " @ " + parentToken.Value;
                    tokenStack.Push(new SemanticToken
                    {
                        Type = IsAPredicate(parentToken) ? TokenType.Boolean : TokenType.Int,
                        Value = subExpression
                    });
                }
            }
            else
            {
                throw new SemanticException("Unknown identifier: " + operand.Value);
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
                string left = ConvertTokenToGforthReal(lhs);
                string right = ConvertTokenToGforthReal(rhs);

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
            else if (!IsAPredicate(parentToken) && (lhs.Type == TokenType.Identifier || rhs.Type == TokenType.Identifier))
            {
                HandleIdentifierForBinary(ref tokenStack, parentToken, lhs, rhs);
            }
            else if (IsAPredicate(parentToken))
            {
                if (lhs.Type == TokenType.Real)
                {
                    lhs.Value = ConvertTokenToGforthReal(lhs);
                }

                if (rhs.Type == TokenType.Real)
                {
                    rhs.Value = ConvertTokenToGforthReal(rhs);
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
        /// Generates gforth code for a binary operation which uses one or more identifiers.
        /// </summary>
        private void HandleIdentifierForBinary(ref Stack<SemanticToken> tokenStack, Token parentToken, SemanticToken lhs, SemanticToken rhs)
        {
            Tuple<Token, TokenType> val;

            string gforthLhs = string.Empty;
            string gforthRhs = string.Empty;
            bool isARealOperation = false;
            string subExpression = string.Empty;

            if (lhs.Type == TokenType.Identifier && m_table.TryGetValue(lhs.Value, out val))
            {
                if (val.Item2 == TokenType.Undefined)
                {
                    throw new SemanticException(lhs.Value + " is an unbound identifier.");
                }

                gforthLhs += lhs.Value + " @ ";

                if (val.Item2 == TokenType.Real)
                {
                    isARealOperation = true;
                }
            }
            else
            {
                throw new SemanticException(lhs.Value + " is an unrecognized identifier.");
            }

            if (rhs.Type == TokenType.Identifier && m_table.TryGetValue(rhs.Value, out val))
            {
                if (val.Item2 == TokenType.Undefined)
                {
                    throw new SemanticException(rhs.Value + " is an unbound identifier.");
                }

                gforthRhs += rhs.Value + " @ ";

                if (val.Item2 == TokenType.Real)
                {
                    isARealOperation = true;
                }
            }
            else
            {
                throw new SemanticException(rhs.Value + " is an unrecognized identifier.");
            }

            if (isARealOperation)
            {
                gforthLhs += (lhs.Type != TokenType.Real) ? "s>f " : string.Empty;
                gforthRhs += (rhs.Type != TokenType.Real) ? "s>f " : string.Empty;

                subExpression = gforthLhs + " " + gforthRhs + " " + "f" + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = IsAPredicate(parentToken) ? TokenType.Boolean : TokenType.Real,
                    Value = subExpression
                });
            }
            else
            {
                subExpression = lhs.Value + " " + rhs.Value + " " + parentToken.Value;

                tokenStack.Push(new SemanticToken
                {
                    Type = IsAPredicate(parentToken) ? TokenType.Boolean : TokenType.Int,
                    Value = subExpression
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

            string left = ConvertTokenToGforthString(lhs);
            string right = ConvertTokenToGforthString(rhs);

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
        private string ConvertTokenToGforthReal(SemanticToken token)
        {
            return token.Type == TokenType.Int ? token.Value + " s>f"
                : !token.Value.ToLower().Contains("e") ? token.Value + "e" : token.Value;
        }

        /// <summary>
        /// Converts a string token into a GForth string.
        /// </summary>
        private string ConvertTokenToGforthString(SemanticToken token)
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
