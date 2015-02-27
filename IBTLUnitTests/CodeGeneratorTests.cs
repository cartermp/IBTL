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
