using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace LibExceptionAnalyser
{
    public interface IAnalyser<T>
    {
        SyntaxToken Find(CSharpSyntaxNode syntaxNode);
        T Instantiate();
        void Execute(string assemblyPath);
    }
}
