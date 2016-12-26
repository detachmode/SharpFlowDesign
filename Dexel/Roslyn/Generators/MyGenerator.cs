using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dexel.Model;
using Dexel.Model.DataTypes;
using Dexel.Model.Manager;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Options;

namespace Roslyn
{

    public class MyGenerator
    {
        private readonly Workspace _workspace = new AdhocWorkspace();
        public SyntaxGenerator Generator;
        public MyGenerator()
        {
            Generator = SyntaxGenerator.GetGenerator(_workspace, LanguageNames.CSharp);
        }


        public void GenerateCodeAndPrint(MainModel model)
        {
            var methods = GenerateAllMethods(model);
            var datatypes = GenerateDataTypes(model);
            
            var interactionsClass = Class("Interactions", datatypes.Concat(methods).ToArray());
            var usingDirectives = Generator.NamespaceImportDeclaration("System");
            var namespaceDeclaration = Generator.NamespaceDeclaration("AutoGenerated", interactionsClass);

            CompileAndOutput(usingDirectives, namespaceDeclaration);
        }



        public IEnumerable<SyntaxNode> GenerateDataTypes(MainModel model)
        {
            return model.DataTypes.Select(dt =>
            {
                var body = DataTypesGenerator.GenerateFields(Generator, dt);
                return Class(Helper.FirstCharToUpper(dt.Name), body.ToArray());
            });
        }


        public string GenerateMethods(MainModel mainModel)
        {
            var methods = GenerateAllMethods(mainModel);
            return CompileToString(methods);
        }

        


        public List<SyntaxNode> GenerateAllMethods(MainModel model)
        {
            var operations = GeneratedOperations(model);
            return GenerateIntegrations(operations, model);
        }




        private List<SyntaxNode> GenerateIntegrations(List<SyntaxNode> operations, MainModel model)
        {
            model.FunctionUnits.Where(sc => sc.IsIntegrating.Count > 0).ToList().ForEach(isc =>
            {
                var body = IntegrationGenerator.GenerateIntegrationBody(Generator, model, isc);
                var main = MethodsGenerator.GenerateStaticMethod(Generator, isc, body);
                operations.Add(main);
            });
            return operations;
        }


        private List<SyntaxNode> GeneratedOperations(MainModel mainModel)
        {
            var operationBody = MethodsGenerator.GetNotImplementatedException(Generator);
            return mainModel.FunctionUnits
                .Where(functionUnit => functionUnit.IsIntegrating.Count == 0)
                .Select(functionUnit => MethodsGenerator.GenerateStaticMethod(Generator, functionUnit, operationBody))
                .ToList();

        }


        public string CompileToString(List<SyntaxNode> nodes)
        {
            var res = nodes.Select(n => n.NormalizeWhitespace().ToFullString()).Aggregate((f,s) => f + "\n\n" + s);
            return res;
        }


        private void CompileAndOutput(SyntaxNode usingDirectives, SyntaxNode namespaceDeclaration)
        {
            AdhocWorkspace cw = new AdhocWorkspace();
            var t = namespaceDeclaration.WithTrailingTrivia(SyntaxFactory.Space);
            OptionSet options = cw.Options;
            //options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, false);
            //options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInTypes, false);
            //options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInAnonymousMethods, true);
            options = options.WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInLambdaExpressionBody, false);



            var formattedResult = Formatter.Format(namespaceDeclaration, cw, options);
            //var newNode = Generator.CompilationUnit(usingDirectives, namespaceDeclaration).
            //    NormalizeWhitespace();

            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                File.WriteAllText(Path.Combine(desktop, @"generatedFlowDesign.cs"), formattedResult.ToFullString());
            }
            catch
            {
                Console.WriteLine("Couldn't generate or write file");
            }

            Console.Write(formattedResult.ToFullString());
        }


        public SyntaxNode Class(string name, SyntaxNode[] members)
        {
            // Generate the class
            var classDefinition = Generator.ClassDeclaration(
                name, 
                typeParameters: null,
                accessibility: Accessibility.Public,
                modifiers: DeclarationModifiers.None,
                baseType: null,
                interfaceTypes: null,
                members: members
                );
            return classDefinition;
        }

    }


    public class GeneratedLocalVariable
    {
        public DataStreamDefinition Source;
        public string VariableName;
        public IEnumerable<NameType> NameTypes;
    }


    //internal class CreatedLocalMethod
    //{
    //    public string LocalName = string.Empty;
    //    public string LocalType = string.Empty;
    //    public IfunctionUnit FunctionUnit = null;
    //}


    public class MethodWithParameterDependencies
    {
        public FunctionUnit OfFunctionUnit;
        public List<Parameter> Parameters = new List<Parameter>();
    }


    public enum Found
    {
        NotFound,
        FromParent,
        FoundInPreviousChild
    }


    public class Parameter
    {
        public Found FoundFlag;
        public DataStreamDefinition Source;
        public NameType NeededNameType;
        public bool AsOutput { get; set; }
    }




}