namespace Chickensoft.AutoInject.Analyzers.PerformanceTests;

using BenchmarkDotNet.Running;

public class Program {
  public static void Main(string[] args) =>
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
