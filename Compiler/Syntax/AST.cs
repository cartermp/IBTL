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

        public AST() : this(new List<ASTNode>()) { }

        public AST(List<ASTNode> nodes)
        {
            mNodes = nodes;
        }

        public int Size()
        {
            return mNodes.Count;
        }

        public bool Empty()
        {
            return !mNodes.Any();
        }

        public ASTNode Back()
        {
            return mNodes.Last();
        }

        public string ToGforth()
        {
            List<Token> tokens = new List<Token>();
            string gforth = string.Empty;

            mNodes.ForEach(node => Walk(node, tokens, ref gforth));

            return gforth + "CR";
        }

        /// <summary>
        /// Traverses the IBTL AST in post-order fashion.
        /// </summary>
        private bool Walk(ASTNode node, List<Token> tokens, ref string gforth)
        {
            int start = tokens.Count > 0 ? tokens.Count - 1 : 0;

            bool containsReal = false;

            if (node.Children == null || !node.Children.Any())
            {
                tokens.Add(node.Token);
                return node.Token.Type == TokenType.Real;
            }
            else
            {
                foreach (var child in node.Children)
                {
                    // If we've already seen a Real, we don't want to overwrite our flag.
                    if (containsReal)
                    {
                        Walk(child, tokens, ref gforth);
                    }
                    else
                    {
                        containsReal = Walk(child, tokens, ref gforth);
                    }
                }
            }

            tokens.Add(node.Token);

            if (containsReal)
            {
                HandleReal(tokens, start, ref gforth);
            }
            else
            {
                HandleNoReals(tokens, start, ref gforth);
            }

            return containsReal;
        }

        /// <summary>
        /// Handles when a given subtree has no real tokens.
        /// </summary>
        private void HandleNoReals(List<Token> tokens, int start, ref string gforth)
        {
            bool sawString = false;

            for (int i = start; i < tokens.Count; i++)
            {
                TokenType type = tokens[i].Type;
                string value = tokens[i].Value;

                if (!sawString && type == TokenType.String)
                {
                    sawString = true;
                }

                if (sawString)
                {
                    if (type == TokenType.BinaryOperator)
                    {
                        if (value != "+")
                        {
                            throw new SemanticException("Must use '+' as concatenation operator.");
                        }

                        gforth += "s" + value;
                    }
                    else if (type == TokenType.String)
                    {
                        gforth += "s\" " + value + "\"";
                    }
                    else
                    {
                        throw new SemanticException("Non-strings are no valid for string literals.");
                    }
                }
                else
                {
                    gforth += value;
                }

                gforth += " ";
            }
        }

        /// <summary>
        /// Handles generating Gforth when a given subtree contains a real.
        /// </summary>
        private void HandleReal(List<Token> tokens, int start, ref string gforth)
        {
            for (int i = start; i < tokens.Count; i++)
            {
                TokenType type = tokens[i].Type;
                string value = tokens[i].Value;

                if (type == TokenType.BinaryOperator || type == TokenType.UnaryOperator)
                {
                    gforth += "f" + tokens[i].Value;
                }
                else if (type == TokenType.Int)
                {
                    gforth += value + " s>f";
                }
                else if (type == TokenType.Real)
                {
                    gforth += value;
                    if (!value.Contains("e"))
                    {
                        gforth += "e";
                    }
                }
                else
                {
                    throw new SemanticException("wah");
                }

                gforth += " ";
            }
        }
    }
}
