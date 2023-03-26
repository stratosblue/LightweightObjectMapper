using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LightweightObjectMapper.Test;

// see https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators

public abstract class SourceGeneratorTestBase
{
    #region Protected 方法

    public virtual IEnumerable<MetadataReference>? ProvideAppendReferences() => null;

    public string[] ReadCodes(params string[] filenames) => ReadCodesWithPath("", filenames);

    public string[] ReadCodesWithPath(string path, params string[] filenames)
    {
        const string NamePrefix = "LightweightObjectMapper.Generator.Test..Codes.";

        var assembly = Assembly.GetExecutingAssembly();

        var allResourceNames = assembly.GetManifestResourceNames()
                                       .Select(m => (Path: m, FilteredPath: m.Substring(NamePrefix.Length)));

        if (!string.IsNullOrWhiteSpace(path))
        {
            path = path.Replace('/', '.');
            allResourceNames = allResourceNames.Where(m => m.FilteredPath.StartsWith(path))
                                               .Select(m => (Path: m.Path, FilteredPath: m.FilteredPath.Substring(path.Length)));
        }

        var selectedResources = allResourceNames.Where(m => filenames.Contains(m.FilteredPath));

        return selectedResources.Select(m => ReadToEnd(assembly.GetManifestResourceStream(m.Path)))
                                .ToArray();

        static string ReadToEnd(Stream? stream)
        {
            if (stream is null)
            {
                return string.Empty;
            }
            using var reader = new StreamReader(stream, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: Convert.ToInt32(stream.Length), leaveOpen: true);
            return reader.ReadToEnd();
        }
    }

    protected static void CheckDiagnosticsIsSame(Diagnostic[] diagnostics, int count, string id)
    {
        Assert.AreEqual(count, diagnostics.Length);

        foreach (var diagnostic in diagnostics)
        {
            Assert.AreEqual(id, diagnostic.Descriptor.Id);
        }
    }

    protected static IEnumerable<MetadataReference> LoadFrameworkMetadataReferences()
    {
        var stringAssemblyLocation = typeof(string).GetTypeInfo().Assembly.Location;
        if (Path.GetDirectoryName(stringAssemblyLocation) is string locationBase
            && !string.IsNullOrWhiteSpace(locationBase))
        {
            var dlls = Directory.GetFiles(locationBase, "*.dll");
            return dlls.Select(m => MetadataReference.CreateFromFile(m));
        }
        throw new InvalidOperationException("Load framework metadata reference fail.");
    }

    protected static MetadataReference LoadMetadataReferenceByType<T>()
    {
        return MetadataReference.CreateFromFile(typeof(T).GetTypeInfo().Assembly.Location);
    }

    protected CompilationResult CompilationEmbeddedSource(params string[] sources) => CompilationSource(ReadCodes(sources));

    protected CompilationResult CompilationEmbeddedSourceWithPath(string path, params string[] sources) => CompilationSource(ReadCodesWithPath(path, sources));

    protected CompilationResult CompilationSource(params string[] sourceCodes)
    {
        if (sourceCodes.Length == 0)
        {
            throw new ArgumentException("没有正确输入需要编译的代码", nameof(sourceCodes));
        }

        var inputCompilation = CreateCompilation(sourceCodes);

        var generator = new LightweightObjectMapperSourceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.WithUpdatedParseOptions(ProvideParseOptions() ?? CSharpParseOptions.Default);

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult();

        if (runResult.GeneratedTrees.Length <= 2    //排除两个预定义代码
            && inputCompilation.GetDiagnostics() is { } inputDiagnostics
            && inputDiagnostics.Where(m => m.DefaultSeverity >= DiagnosticSeverity.Error) is { } inputErrorDiagnostics
            && inputErrorDiagnostics.Any())
        {
            foreach (var item in inputErrorDiagnostics)
            {
                Console.WriteLine($"编译错误 - {item.GetMessage()}");
            }

            Assert.Fail($"输入代码包含编译错误 {inputErrorDiagnostics.Count()} 个");
        }

        return new CompilationResult(generator, driver, inputCompilation, outputCompilation, diagnostics, runResult);
    }

    protected CSharpParseOptions? ProvideParseOptions() => null;

    #endregion Protected 方法

    #region Private 方法

    private Compilation CreateCompilation(params string[] sources)
    {
        IEnumerable<MetadataReference> GetReferences()
        {
            var references = ProvideAppendReferences() ?? new List<MetadataReference>();
            var pinned = new[]
            {
                LoadMetadataReferenceByType<Binder>(),
            };
            return pinned.Concat(references);
        }

        var parseOptions = ProvideParseOptions() ?? CSharpParseOptions.Default;

        return CSharpCompilation.Create("SourceGeneratorTestAssembly",
                                        sources.Select(m => CSharpSyntaxTree.ParseText(m, parseOptions)),
                                        GetReferences(),
                                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion Private 方法

    #region Assert

    protected static void NoCompilationError(CompilationResult compilationResult)
    {
        var errors = compilationResult.GetDiagnosticErrors();

        if (errors.Length > 0)
        {
            foreach (var item in errors)
            {
                Console.WriteLine($"编译错误 - {item.GetMessage()}");
            }
            Assert.Fail($"当前包含编译错误 {errors.Count()} 个");
        }
    }

    protected static void NoCompilationWarning(CompilationResult compilationResult)
    {
        var warnings = compilationResult.GetDiagnosticWarnings();

        if (warnings.Length > 0)
        {
            foreach (var item in warnings)
            {
                Console.WriteLine($"编译警告 - {item.GetMessage()}");
            }
            Assert.Fail($"当前包含编译警告 {warnings.Count()} 个");
        }
    }

    #endregion Assert
}
