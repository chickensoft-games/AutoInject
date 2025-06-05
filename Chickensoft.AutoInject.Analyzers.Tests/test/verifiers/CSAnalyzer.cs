namespace Chickensoft.AutoInject.Analyzers.Tests.Verifiers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chickensoft.AutoInject;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Shouldly;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Design",
  "CA1000: Do not declare static members on generic types",
  Justification = "CA1000 prefers no generic arguments, but either method or class needs them here"
)]
public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new() {
  /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
  public static DiagnosticResult Diagnostic()
      => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();

  /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
  public static DiagnosticResult Diagnostic(string diagnosticId)
      => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>
        .Diagnostic(diagnosticId);

  /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
  public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
      => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>
        .Diagnostic(descriptor);

  /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
  public static async Task VerifyAnalyzerAsync(
      string source,
      params DiagnosticResult[] expected) {
    var test = new Test {
      TestCode = source,
    };

    // we need the AutoInject dll to compile the test code,
    // but we have to strip the extension
    var autoInjectPath =
      typeof(IAutoNode).Assembly.Location;
    var autoInjectExtension = Path.GetExtension(autoInjectPath);
    autoInjectPath = autoInjectPath[..^autoInjectExtension.Length];

    var latestGodot = await GetLatestVersion("GodotSharp");
    var latestIntrospection = await GetLatestVersion("Chickensoft.Introspection");

    latestGodot.ShouldNotBeNull();
    latestIntrospection.ShouldNotBeNull();

    test.ReferenceAssemblies = ReferenceAssemblies.Net.Net80
      .AddPackages(
        [
          new PackageIdentity("GodotSharp", latestGodot.ToNormalizedString()),
          new PackageIdentity(
            "Chickensoft.Introspection",
            latestIntrospection.ToNormalizedString()
          ),
        ]
      )
      .AddAssemblies(
        [
          autoInjectPath
        ]
      );

    test.ExpectedDiagnostics.AddRange(expected);
    await test.RunAsync(CancellationToken.None);
  }

  private static async Task<NuGetVersion?> GetLatestVersion(string packageID) {
    var log = new NullLogger();
    var providers = new List<Lazy<INuGetResourceProvider>>();
    providers.AddRange(Repository.Provider.GetCoreV3());
    var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
    var sourceRepository = new SourceRepository(packageSource, providers);
    var packageMetadataResource = await sourceRepository
      .GetResourceAsync<PackageMetadataResource>();
    var sourceCacheContext = new SourceCacheContext();
    var searchMetadata = await packageMetadataResource
      .GetMetadataAsync(
        packageID,
        false,
        false,
        sourceCacheContext,
        log,
        CancellationToken.None);

    NuGetVersion? latest = null;
    foreach (var result in searchMetadata) {
      if (result is PackageSearchMetadataRegistration registration) {
        if (latest is null || registration.Version.CompareTo(latest) > 0) {
          latest = registration.Version;
        }
      }
    }

    return latest;
  }
}
