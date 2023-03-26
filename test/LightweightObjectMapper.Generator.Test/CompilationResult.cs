using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper.Test;

public class CompilationResult
{
    #region Public 属性

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public GeneratorDriver Driver { get; }

    public Compilation InputCompilation { get; }

    public Compilation OutputCompilation { get; }

    public GeneratorDriverRunResult Result { get; }

    public LightweightObjectMapperSourceGenerator SourceGenerator { get; }

    #endregion Public 属性

    #region Public 构造函数

    public CompilationResult(LightweightObjectMapperSourceGenerator sourceGenerator,
                             GeneratorDriver driver,
                             Compilation inputCompilation,
                             Compilation outputCompilation,
                             ImmutableArray<Diagnostic> diagnostics,
                             GeneratorDriverRunResult result)
    {
        SourceGenerator = sourceGenerator;
        Driver = driver;
        InputCompilation = inputCompilation;
        OutputCompilation = outputCompilation;
        Diagnostics = diagnostics;
        Result = result;
    }

    #endregion Public 构造函数

    #region Public 方法

    public Diagnostic[] GetDiagnosticErrors()
    {
        return Diagnostics.Where(m => m.DefaultSeverity >= DiagnosticSeverity.Error).ToArray();
    }

    public Diagnostic[] GetDiagnosticWarnings()
    {
        return Diagnostics.Where(m => m.DefaultSeverity == DiagnosticSeverity.Warning).ToArray();
    }

    #endregion Public 方法
}
