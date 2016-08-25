using System;

/// <summary>
/// Indicates that a field should be injected by the GameFactory
/// </summary>
/// <author>Justin Strandburg</author>
[AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
public class Inject : Attribute {
}
