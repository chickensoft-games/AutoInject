namespace Chickensoft.AutoInject.Analyzers.PerformanceTests.Utils;

// Modified from https://github.com/dotnet/roslyn-analyzers/

// Copyright(c) .NET Foundation and Contributors
//
// All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;

public static class Compilations {
  public static async Task<(Compilation?, AnalyzerOptions)> CreateCompilation(
    (string, string)[] sources,
    (string, string)[] globalOptions
  ) {
    var (project, options) = CreateProject(
      sources,
      globalOptions,
      "TestProject",
      "/0/Test",
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
      new CSharpParseOptions(LanguageVersion.Default)
    );
    return (await project.GetCompilationAsync().ConfigureAwait(false), options);
  }

  public static (Project, AnalyzerOptions) CreateProject(
    (string, string)[] sources,
    (string, string)[]? globalOptions,
    string projectName,
    string defaultPrefix,
    CompilationOptions compilationOptions,
    ParseOptions parseOptions
  ) {
    var projectId = ProjectId.CreateNewId(debugName: projectName);
    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
              .Where(
                assembly =>
                  assembly.Location != string.Empty && !assembly.IsDynamic
              ).Concat(
                // Make sure we capture CodeAnalysis.CSharp.Workspaces
                [typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions).Assembly]
              );
    var host = MefHostServices.Create(assemblies);
    var workspace = new AdhocWorkspace(host);
    var project = workspace
      .CurrentSolution
      .AddProject(projectName, projectName, LanguageNames.CSharp)
      .WithCompilationOptions(compilationOptions)
      .WithParseOptions(parseOptions)
      .AddMetadataReferences(
        assemblies.Select(a => MetadataReference.CreateFromFile(a.Location))
      );
    foreach (var source in sources) {
      var fileName = CreateFileName(defaultPrefix, source.Item1);
      var document = project.AddDocument(
        fileName,
        source.Item2,
        filePath: fileName
      );
      project = document.Project;
    }
    var analyzerOptions = project.AnalyzerOptions;
    if (globalOptions is not null) {
      analyzerOptions = new AnalyzerOptions(
        [],
        new MyOptionsProvider(globalOptions)
      );
    }
    return (project, analyzerOptions);
  }

  public static string CreateFileName(string prefix, string file) =>
    $"{prefix}{file}.cs";
}

public sealed class MyOptionsProvider : AnalyzerConfigOptionsProvider {
  public override AnalyzerConfigOptions GlobalOptions { get; }

  public MyOptionsProvider((string, string)[] options) {
    GlobalOptions = new MyOptions(options);
  }

  public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
    GlobalOptions;
  public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
    GlobalOptions;
}

public sealed class MyOptions : AnalyzerConfigOptions {
  private readonly Dictionary<string, string> _options;

  public MyOptions((string, string)[] options) {
    _options = [];
    foreach (var option in options) {
      _options[option.Item1] = option.Item2;
    }
  }
  public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
    _options.TryGetValue(key, out value);
}
