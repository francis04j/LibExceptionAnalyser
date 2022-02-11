using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysisApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length != 2)
            //    return;

            string solutionPath = @"C:\Dev\ConsoleApp1\ConsoleApp1.sln";
            // args[0];
            string logPath = @"C:\Dev\ConsoleApp1\log.txt";
                // args[1];

            StringBuilder warnings = new StringBuilder();

            const string warningMessageFormat =
              "'if' with equal 'then' and 'else' blocks is found in file {0} at line {1}";

            
            if (!MSBuildLocator.IsRegistered) MSBuildLocator.RegisterDefaults();
            using (var workspace = MSBuildWorkspace.Create())
            {
                Project currProject = GetProjectFromSolution(solutionPath, workspace);
                foreach (var document in currProject.Documents)
                {
                    var tree = document.GetSyntaxTreeAsync().Result;
                    
                    var catchStatementNodes = tree.GetRoot()
                                              .DescendantNodesAndSelf()
                                              .OfType<CatchClauseSyntax>();
                    foreach (var catchClause in catchStatementNodes)
                    {
                        var typeStr = FindExceptions(catchClause);
                        Type type = Type.GetType($"System.Data.SqlClient.{typeStr}");
                        var description = string.Empty;
                        if (type != null && type.Equals(typeof(IndexOutOfRangeException)))
                        {
                            SqlException exception = Instantiate<SqlException>();
                            description = exception.Number.ToString();
                            warnings.AppendLine(description);
                        }
                        warnings.AppendLine(typeStr);
                    }
                    
                }

                if (warnings.Length != 0)
                    File.AppendAllText(logPath, warnings.ToString());
            }
        }

        static Project GetProjectFromSolution(String solutionPath,
                                      MSBuildWorkspace workspace)
        {
       
            if (!MSBuildLocator.IsRegistered) MSBuildLocator.RegisterDefaults();
            Solution currSolution = workspace.OpenSolutionAsync(solutionPath)
                                             .Result;
            return currSolution.Projects.Single();
        }

        static bool ApplyRule(IfStatementSyntax ifStatement)
        {
            if (ifStatement?.Else == null)
                return false;

            StatementSyntax thenBody = ifStatement.Statement;
            StatementSyntax elseBody = ifStatement.Else.Statement;

            return SyntaxFactory.AreEquivalent(thenBody, elseBody);
        }

        static string FindExceptions(CatchClauseSyntax catchClauseSyntax)
        {
             var conssss= (catchClauseSyntax.Declaration.Type) as IdentifierNameSyntax;
            return conssss.Identifier.ValueText;
        }

        private static T Instantiate<T>() where T : class
        {
            return FormatterServices.GetUninitializedObject(typeof(T)) as T;
        }
    }
}
