namespace Chickensoft.AutoInject.Analyzers.Tests.Util;

using System;
using System.IO;

public static class AssemblyHelper
{
  /// <summary>
  /// Get the path to the assembly for a given type, without the file extension
  /// if one exists.
  /// </summary>
  /// <param name="type">A type belonging to the desired assembly.</param>
  /// <returns>The path to the assembly, excluding any file extension.</returns>
  public static string GetAssemblyPath(Type type)
  {
    var path = type.Assembly.Location;
    var extension = Path.GetExtension(path);
    return path[..^extension.Length];
  }
}
