using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Compiler.Syntax;
using System.Collections.Generic;
using Compiler.Exceptions;

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
                            Token = new Token{ Value = "\"smoke weed errday\"\"", Type = TokenType.String }
                        },
                        new ASTNode
                        {
                            Token = new Token{ Value = "\"swagswagswag\"\"", Type = TokenType.String }
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
        /// (or true false) => "true false or".
        /// </summary>
        [TestMethod]
        public void BasicOrTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "or", Type = TokenType.BinaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "true", Type = TokenType.True }
                        },
                        new ASTNode
                        {
                            Token = new Token{ Value = "false", Type = TokenType.False }
                        }
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "true false or CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (and true true) => "true true and".
        /// </summary>
        [TestMethod]
        public void BasicAndTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "and", Type = TokenType.BinaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "true", Type = TokenType.True }
                        },
                        new ASTNode
                        {
                            Token = new Token{ Value = "true", Type = TokenType.False }
                        }
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "true true and CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
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

        /// <summary>
        /// (if (> 5 3) 7 2) => "5 3.0 > if 7 else 2 endif"
        /// </summary>
        [TestMethod]
        public void IfWithRealsTest()
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
                                    Token = new Token { Value = "3.0", Type = TokenType.Real }
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "5 s>f 3.0e f> if 7 else 2 endif CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (stdout ("hello" "world")) => "s" hello" s" world" s+ type".
        /// </summary>
        [TestMethod]
        public void StdoutStringsTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Type = TokenType.Statement, Value = "stdout" },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token { Value = "+", Type = TokenType.BinaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token{ Value = "\"hello\"\"", Type = TokenType.String }
                                },
                                new ASTNode
                                {
                                    Token = new Token{ Value = "\"world\"\"", Type = TokenType.String }
                                }
                            }
                        }
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "s\" hello\" s\" world\" s+ type CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (stdout (12 13)) => "12 13 + .".
        /// </summary>
        [TestMethod]
        public void StdoutIntsTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Type = TokenType.Statement, Value = "stdout" },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token { Value = "+", Type = TokenType.BinaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token{ Value = "12", Type = TokenType.Int }
                                },
                                new ASTNode
                                {
                                    Token = new Token{ Value = "13", Type = TokenType.Int }
                                }
                            }
                        }
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "12 13 + . CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (stdout (12 13)) => "12 13 + .".
        /// </summary>
        [TestMethod]
        public void StdoutRealsTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Type = TokenType.Statement, Value = "stdout" },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token { Value = "+", Type = TokenType.BinaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token{ Value = "12.0", Type = TokenType.Real }
                                },
                                new ASTNode
                                {
                                    Token = new Token{ Value = "13", Type = TokenType.Int }
                                }
                            }
                        }
                    }
                }
            };

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "12.0e 13 s>f f+ f. CR";
            string actual = new AST(nodes).ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (^ 1.0 2) => "1.0e 2 s>f f**".
        /// </summary>
        [TestMethod]
        public void RealsPowerTest()
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1.0e 2 s>f f** CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (% 1 2) => "1 2 mod".
        /// </summary>
        [TestMethod]
        public void ModIntTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "%", Type = TokenType.BinaryOperator },
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1 2 mod CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (% 1.0 2) => "1.0e 2 s>f fmod".
        /// </summary>
        [TestMethod]
        public void ModRealTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "%", Type = TokenType.BinaryOperator },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token { Value = "1.0", Type = TokenType.Real }
                        },
                        new ASTNode
                        {
                            Token = new Token { Value = "2", Type = TokenType.Int }
                        }
                    }
                }
            };

            AST ast = new AST(nodes);

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "1.0e 2 s>f fmod CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (while (> 5 3) (stdout 5)) => "begin 5 3 > while 5 . repeat"
        /// </summary>
        [TestMethod]
        public void BasicWhileTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "while", Type = TokenType.Statement },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token { Value = ">", Type = TokenType.BinaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token { Value = "5", Type = TokenType.Int}
                                },
                                new ASTNode
                                {
                                    Token = new Token { Value = "3", Type = TokenType.Int}
                                }
                            }
                        },
                        new ASTNode
                        {
                            Token = new Token { Value = "stdout", Type = TokenType.Statement },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token { Value = "5", Type = TokenType.Int }
                                }
                            }
                        }
                    }
                }
            };

            AST ast = new AST(nodes);

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "begin 5 3 > while 5 . repeat CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (while (> 5 3) (stdout 5) (+ 1 2) (- 1 2)) => "begin 5 3 > while 5 . 1 2 + 1 2 - repeat"
        /// </summary>
        [TestMethod]
        public void WhileWithExprListTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = "while", Type = TokenType.Statement },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token { Value = ">", Type = TokenType.BinaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token { Value = "5", Type = TokenType.Int}
                                },
                                new ASTNode
                                {
                                    Token = new Token { Value = "3", Type = TokenType.Int}
                                }
                            }
                        },
                        new ASTNode
                        {
                            Token = new Token { Value = "stdout", Type = TokenType.Statement },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token { Value = "5", Type = TokenType.Int }
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
                        },
                        new ASTNode
                        {
                            Token = new Token { Value = "-", Type = TokenType.BinaryOperator },
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

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "begin 5 3 > while 5 . 1 2 + 1 2 - repeat CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// (if (> 5 3) 7 2)(while (> 5 3) (stdout 5)) => "begin 5 3 > while 5 . repeat"
        /// </summary>
        [TestMethod]
        public void MixedIfAndWhileTest()
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
                },
                new ASTNode
                {
                    Token = new Token { Value = "while", Type = TokenType.Statement },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token { Value = ">", Type = TokenType.BinaryOperator },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token { Value = "5", Type = TokenType.Int}
                                },
                                new ASTNode
                                {
                                    Token = new Token { Value = "3", Type = TokenType.Int}
                                }
                            }
                        },
                        new ASTNode
                        {
                            Token = new Token { Value = "stdout", Type = TokenType.Statement },
                            Children = new List<ASTNode>
                            {
                                new ASTNode
                                {
                                    Token = new Token { Value = "5", Type = TokenType.Int }
                                }
                            }
                        }
                    }
                }
            };

            AST ast = new AST(nodes);

            string expected = ":^ 1 swap 0 u+do over * loop nip ; \n\n" + "5 3 > if 7 else 2 endif CR\n" + "begin 5 3 > while 5 . repeat CR";
            string actual = ast.ToGforth();

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Tests that an exception is thrown because the variable isn't bound yet.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SemanticException))]
        public void BasicAssignTest()
        {
            var nodes = new List<ASTNode>
            {
                new ASTNode
                {
                    Token = new Token { Value = ":=", Type = TokenType.Assignment },
                    Children = new List<ASTNode>
                    {
                        new ASTNode
                        {
                            Token = new Token{ Value = "x", Type = TokenType.Identifier }
                        },
                        new ASTNode
                        {
                            Token = new Token{ Value = "1.0", Type = TokenType.Real }
                        }
                    }
                }
            };

            string actual = new AST(nodes).ToGforth();
        }
    }
}
