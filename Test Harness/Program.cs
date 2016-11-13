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

            Console.WriteLine("Running");

            var ints = new[] { 1, 2, 3, 4 };

            var x = new Var<string>();
            var y = new Var<string>();

            foreach (var result in IsParent(x, y)
                                    .AsEnumerable())
            {
                Console.WriteLine($"X = {Format(x)}, Y = {Format(y)}");
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static string Format<T>(Var<T> variable)
        {
            return variable.HasValue
                ? variable.Value.ToString()
                : "?";
        }

        private static Query IsMale(Var<string> deity)
        {
            return EnumerableQuery.Create(new[]
            {
                "cronus",
                "pluto",
                "poseidon",
                "zeus",
                "ares",
                "hephaestus"
            }, deity);
        }

        private static Query IsFemale(Var<string> deity)
        {
            return EnumerableQuery.Create(new[]
            {
                "rhea",
                "hestia",
                "hera",
                "demeter",
                "athena",
                "hebe",
                "persephone"
            }, deity);
        }

        private static Query IsParent(Var<string> parent, Var<string> child)
        {
            return parent.Unify("cronus")
                            .And(EnumerableQuery.Create, new[]
                            {
                                "hestia",
                                "pluto",
                                "poseidon",
                                "zeus",
                                "hera",
                                "demeter",
                            }, child)
                            .Or(() =>
                                parent.Unify("rhea")
                                   .And(EnumerableQuery.Create, new[]
                                   {
                                        "hestia",
                                        "pluto",
                                        "poseidon",
                                        "zeus",
                                        "hera",
                                        "demeter",
                                   }, child)
                            );

            //return new EnumerableQuery<string[]>(new[]
            //{
            //    new [] { "cronus", "hestia" },
            //    new [] { "cronus", "pluto" },
            //    new [] { "cronus", "poseidon" },
            //    new [] { "cronus", "zeus" },
            //    new [] { "cronus", "hera" },
            //    new [] { "cronus", "demeter" },
            //    new [] { "rhea", "hestia" },
            //    new [] { "rhea", "pluto" },
            //    new [] { "rhea", "poseidon" },
            //    new [] { "rhea", "zeus" },
            //    new [] { "rhea", "hera" },
            //    new [] { "rhea", "demeter" },
            //    new [] { "zeus", "athena" },
            //    new [] { "zeus", "ares" },
            //    new [] { "zeus", "hebe" },
            //    new [] { "zeus", "hephaestus" },
            //    new [] { "hera", "ares" },
            //    new [] { "hera", "hebe" },
            //    new [] { "hera", "hephaestus" },
            //    new [] { "zeus", "persephone" },
            //    new [] { "demeter", "persephone" }
            //}, array => parent.TryUnify(array[0]) && child.TryUnify(array[1]));
        }

        private static Query IsSon(Var<string> parent, Var<string> child)
        {
            return IsParent(parent, child)
                    .And(IsMale, child);
        }

        private static Query IsAncestor(Var<string> ancestor, Var<string> descendant)
        {
            return IsParent(ancestor, descendant)
                .Or(() =>
                {
                    var parent = new Var<string>();

                    return IsParent(parent, descendant)
                            .And(IsAncestor, ancestor, parent);
                });
        }
    }
}
