using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Compiler.Syntax;
using System.Collections.Generic;

namespace IBTLUnitTests
{
    /// <summary>
    /// tests things
    /// </summary>
    [TestClass]
    public class CodeGeneratorTests
    {
        /// <summary>
        /// Tests (+ 1 2).
        /// </summary>
        [TestMethod]
        public void BasicBinaryOpTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "+", Type = TokenType.BinaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "1", Type = TokenType.Int }
                        },
                        new ASTNode
                        {
                            Token = new Token{ Value = "2", Type = TokenType.Int }
                        }
                    }
                }
            };

            AST ast = new AST(nodes);

            string expected = "1 2 + CR";
            string actual = ast.ToGforth();

            Assert.IsTrue(expected == actual);
        }

        /// <summary>
        /// Tests (+ 1.0 2).
        /// </summary>
        [TestMethod]
        public void BasicBinaryOpWithRealTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "+", Type = TokenType.BinaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "1.0", Type = TokenType.Real }
                        },
                        new ASTNode
                        {
                            Token = new Token{ Value = "2", Type = TokenType.Int }
                        }
                    }
                }
            };

            AST ast = new AST(nodes);

            string expected = "1.0e 2 s>f f+ CR";
            string actual = ast.ToGforth();

            Assert.IsTrue(expected == actual);
        }

        /// <summary>
        /// Tests (+ 1.0 (- 1 2)).
        /// </summary>
        [TestMethod]
        public void BinaryOpWithRealsAndIntsTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "+", Type = TokenType.BinaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "1.0", Type = TokenType.Real }
                        },
                        new ASTNode
                        {
                            Token = new Token{ Value = "-", Type = TokenType.BinaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token{ Value = "1", Type = TokenType.Int }
                                },
                                new ASTNode
                                {
                                    Token = new Token{ Value = "2", Type = TokenType.Int }
                                }
                            }
                        }
                    }
                }
            };

            AST ast = new AST(nodes);

            string expected = "1.0e 1 2 - s>f f+ CR";
            string actual = ast.ToGforth();

            Assert.IsTrue(expected == actual);
        }

        /// <summary>
        /// Tests (+ "smoke weed errday" "swagswagswag").
        /// </summary>
        [TestMethod]
        public void BasicStringTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "+", Type = TokenType.BinaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "smoke weed errday", Type = TokenType.String }
                        },
                        new ASTNode
                        {
                            Token = new Token{ Value = "swagswagswag", Type = TokenType.String }
                        }
                    }
                }
            };

            AST ast = new AST(nodes);

            string expected = "s\" smoke weed errday\" s\" swagswagswag\" s+ CR";
            string actual = ast.ToGforth();

            Assert.IsTrue(expected == actual);
        }
    }
}
