﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Syntax
{
    /// <summary>
    /// Representation of any given node in an IBTL AST.
    /// </summary>
    public class ASTNode
    {
        public Token Token { get; set; }
        public List<ASTNode> Children { get; set; }

        public ASTNode() { }

        public ASTNode(Token token)
        {
            Token = token;
        }

        public void Add(Token token)
        {
            Children.Add(new ASTNode(token));
        }

        public Token LastChild()
        {
            return Children.Last().Token;
        }
    }
}
