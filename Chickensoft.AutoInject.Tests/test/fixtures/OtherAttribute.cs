namespace Chickensoft.AutoInject.Tests.Fixtures;

using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class OtherAttribute : Attribute { }
