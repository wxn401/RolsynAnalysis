using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolsynAnalysis
{
    public class Walker : CSharpSyntaxWalker
    {
        public Walker() : base(SyntaxWalkerDepth.StructuredTrivia)
        { }
        static int Tabs = 0;
        public override void Visit(SyntaxNode node)
        {
            Tabs++;
            var indents = new String(' ', Tabs * 3);
            Console.WriteLine(indents + node.Kind());
            base.Visit(node);
            Tabs--;
        }

        public override void VisitToken(SyntaxToken token)
        {
            var indents = new String(' ', Tabs * 3);
            Console.WriteLine(string.Format("{0}{1}:\t{2}", indents, token.Kind(), token));
            base.VisitToken(token);
        }

        public override void VisitLetClause(LetClauseSyntax node)
        {
            Console.WriteLine("Found a let clause " + node.Identifier.Text);
            base.VisitLetClause(node);
        }
    }
}
