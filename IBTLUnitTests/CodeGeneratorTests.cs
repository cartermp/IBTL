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
        /// Tests (> 1 2) => "1 2 > CR".
        /// </summary>
        [TestMethod]
        public void BasicRelationalOperator()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = ">", Type = TokenType.BinaryOperator },
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1 2 > CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests (^ 1 2) => "1 2 ^ CR".
        /// </summary>
        [TestMethod]
        public void BasicExponentWithIntTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "^", Type = TokenType.BinaryOperator },
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1 2 ^ CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests (+ 1 2) => "1 2 + CR".
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1 2 + CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests (+ 1.0 2) => "1.0e 2 s>f f+ CR".
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1.0e 2 s>f f+ CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests (+ 1.0 (- 1 2)) => "1.0e 1 2 - s>f f+ CR".
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1.0e 1 2 - s>f f+ CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests (+ "smoke weed errday" "swagswagswag") => "s\" smoke weed errday\" s\" swagswagswag\" s+ CR".
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "s\" smoke weed errday\" s\" swagswagswag\" s+ CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests (sin 1.0) => "1.0e fsin CR"
        /// </summary>
        [TestMethod]
        public void BasicUnaryOperatorTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "sin", Type = TokenType.UnaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "1.0", Type = TokenType.Real }
                        }
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1.0e fsin CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests (sin (cos 1.0)) => "1.0e fsin fcos CR".
        /// </summary>
        [TestMethod]
        public void BasicChainedUnaryOperatorTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "sin", Type = TokenType.UnaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "cos", Type = TokenType.UnaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token{ Value = "1.0", Type = TokenType.Real }
                                }
                            }
                        }
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1.0e fcos fsin CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (- (cos 1.0) (+ 1 2)) => "1.0e fcos 1 2 + s>f f-
        /// </summary>
        [TestMethod]
        public void UnaryAndBinaryTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "-", Type = TokenType.BinaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "cos", Type = TokenType.UnaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token{ Value = "1.0", Type = TokenType.Real }
                                }
                            }
                        },
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
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1.0e fcos 1 2 + s>f f- CR";
            string actual = new AST(nodes).ToGforth();

            Assert.IsTrue(expected == actual);
        }

        /// <summary>
        /// (and true false) => 
        /// </summary>
        [TestMethod]
        public void AndTest()
        {

        }

        /// <summary>
        /// (if (> 5 3) 7 2) => "5 3 > if 7 else 2 endif"
        /// </summary>
        [TestMethod]
        public void IfStatementTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "if", Type = TokenType.Statement },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token { Value = ">", Type = TokenType.BinaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token { Value = "5", Type = TokenType.Int }
                                },
                                new ASTNode
                                {
                                    Token = new Token { Value = "3", Type = TokenType.Int }
                                }
                            }
                        },
                        new ASTNode
                        {
                            Token = new Token { Value = "7", Type = TokenType.Int }
                        },
                        new ASTNode
                        {
                            Token = new Token { Value = "2", Type = TokenType.Int }
                        }
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "5 3 > if 7 else 2 endif CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
        }
    }
}
