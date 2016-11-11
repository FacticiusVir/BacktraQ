using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Keeper.LSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //    var tree = SyntaxTree(CompilationUnit()
            //                            .AddUsings(UsingDirective(IdentifierName("System")))
            //                            .AddMembers(NamespaceDeclaration(IdentifierName("Test"))
            //                                            .AddMembers(ClassDeclaration("Program")
            //                                                            .AddMembers(MethodDeclaration(ParseTypeName("int"), "Main")
            //                                                                            .AddModifiers(ParseToken("static"))
            //                                                                            .AddBodyStatements(ReturnStatement(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))))));
            //var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(".\\Input\\TestProgram.cs"));

            //Console.WriteLine(tree);

            //if (!Directory.Exists(".\\Output"))
            //{
            //    Directory.CreateDirectory(".\\Output");
            //}

            //var result = CSharpCompilation.Create("Test.exe", new[] { tree })
            //                                .AddReferences(MetadataReference.CreateFromFile(typeof(void).Assembly.Location))
            //                                .Emit(".\\Output\\Test.exe");

            //foreach(var diag in result.Diagnostics)
            //{
            //    Console.WriteLine(diag);
            //}

            using (new CommitContext())
            {
                var intVariable = new Var<int>();

                var tupleVariable = new Var<Tuple<int, int>>();

                var enumQuery = EnumerableQuery.Create(new[] { 1, 2, 3, 4 });

                var enumQuery2 = EnumerableQuery.Create(new[] { 1, 2, 3, 4 });

                var tupleQuery = QueryPipeline.Create(enumQuery, enumQuery2, Tuple.Create);

                var filterQuery = QueryPipeline.Create(tupleQuery, x => intVariable.TryUnify(x.Item1) && tupleVariable.TryUnify(x));

                foreach (var result in filterQuery.AsEnumerable())
                {
                    Console.WriteLine(result);
                    Console.WriteLine(intVariable.Value);
                    Console.WriteLine(tupleVariable.Value);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
