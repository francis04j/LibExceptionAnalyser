using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace LibExceptionAnalyser
{
    public class SqlExceptionAnalyser : IAnalyser<SqlException>
    {
        public void Execute(StatementSyntax syntax)
        {
            if (syntax != null && syntax.Equals(typeof(IndexOutOfRangeException)))
            {
                SqlException exception = Instantiate<SqlException>();
                description = exception.Number.ToString();
                warnings.AppendLine(description);
            }
        }

        /**
         * looks through the assembly for exceptions that inherit SqlException
         */
        public SyntaxToken Find(CSharpSyntaxNode syntaxNode)
        {
            var identifierSyn = (syntaxNode.Declaration.Type) as IdentifierNameSyntax;
            return identifierSyn.Identifier;
        }

        /**
         * creates an instance of the exception
         */
        public SqlException Instantiate()
        {
            return FormatterServices.GetUninitializedObject(typeof(T)) as T;
        }
    }
}
