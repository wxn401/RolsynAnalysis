using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;

namespace RolsynAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            //算式();

            //CS文件();
            //
            //var work = MSBuildWorkspace.Create();
            //var solution = work.OpenSolutionAsync(@"..\..\..\RoslynPlayGround.sln").Result;
            //var project = solution.Projects.FirstOrDefault(p => p.Name == "Chapter3");
            //if (project == null)
            //    throw new Exception("Could not find the Chapter 3 project");
            //var compilation = project.GetCompilationAsync().Result;
            //MSBuildWorkspace;
            var refference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("internal").WithReferences(refference);

            //var targetType = compilation.GetTypeByMetadataName("Chapter3.IGreetingProfile");
            //IEnumerable<BaseTypeDeclarationSyntax> type = Symbols.FindClassDerivedOrImplementedByType(compilation, targetType);
            //Console.WriteLine(type.First().Identifier.ToFullString());

            //Symbols.CreateMetadataByCurrent();
            //Symbols.CreateEnum();
            //Symbols.CreateRules();
            //Symbols.GerateRules();
            //Symbols.CreateClassAndMethodWithSyntax();
            //Symbols.CreateClassAndMethodWithSyntax2();
            Symbols.WorkerVisit();
            Symbols.CreateClassAndMethodWithSyntax3();
            Console.ReadLine();
        }

        private static void CS文件()
        {
            var code = "";
            using (var sr = new StreamReader("../../GreetingRules.cs"))
            {
                code = sr.ReadToEnd();
            }
            var tree = CSharpSyntaxTree.ParseText(code);
            var walker = new Walker();
            walker.Visit(tree.GetRoot());
        }

        private static void 算式()
        {
            var tree = CSharpSyntaxTree.ParseText("a=b+c;");
            var walker = new Walker();
            walker.Visit(tree.GetRoot());
            Console.WriteLine("We can get back to the original code by call ToFullString ");
            Console.WriteLine(tree.GetRoot().ToFullString());
        }



}
}
