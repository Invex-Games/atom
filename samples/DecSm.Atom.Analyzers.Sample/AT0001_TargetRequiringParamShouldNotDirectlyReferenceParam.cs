#pragma warning disable AT0001

using DecSm.Atom.Build;
using DecSm.Atom.Build.Definition;
using DecSm.Atom.Build.Params;
using JetBrains.Annotations;

namespace DecSm.Atom.Analyzers.Sample;

/// <summary>
///     This interface serves as a sample for the DecSm.Atom analyzer AT0001.
/// </summary>
/// <remarks>
///     Analyzer AT0001 flags targets that directly reference a parameter property within the
///     <see cref="TargetDefinition.RequiresParam(IEnumerable{string})" /> method. Instead, the analyzer
///     expects the parameter name to be passed as a string literal or via nameof().
///     This sample demonstrates both correct and incorrect usages for the analyzer to detect.
/// </remarks>
[PublicAPI]
public interface IMyTarget : IBuildAccessor
{
    /// <summary>
    ///     A sample parameter definition.
    /// </summary>
    /// <remarks>
    ///     This parameter is intended to be referenced by its name (e.g., <c>nameof(MyParam1)</c>)
    ///     in <see cref="TargetDefinition.RequiresParam(IEnumerable{string})" />.
    /// </remarks>
    [ParamDefinition("my-param-1", "My Param 1")]
    string MyParam1 => GetParam(() => MyParam1)!;

    /// <summary>
    ///     Another sample parameter definition.
    /// </summary>
    /// <remarks>
    ///     Similar to <see cref="MyParam1" />, this parameter should be referenced by its name.
    /// </remarks>
    [ParamDefinition("my-param-2", "My Param 2")]
    string MyParam2 => GetParam(() => MyParam2)!;

    /// <summary>
    ///     A property that is NOT a parameter.
    /// </summary>
    /// <remarks>
    ///     This property is included to demonstrate that <see cref="TargetDefinition.RequiresParam(IEnumerable{string})" />
    ///     can correctly reference non-parameter properties by their name without triggering AT0001.
    /// </remarks>
    string NotParam1 { get; }

    /// <summary>
    ///     Another property that is NOT a parameter.
    /// </summary>
    /// <remarks>
    ///     Similar to <see cref="NotParam1" />, this property is not a parameter.
    /// </remarks>
    string NotParam2 { get; }

    /// <summary>
    ///     Demonstrates incorrect and correct usages of <see cref="TargetDefinition.RequiresParam(IEnumerable{string})" />.
    /// </summary>
    /// <remarks>
    ///     The reference to <c>MyParam1</c> directly will be flagged by AT0001.
    ///     The reference to <c>NotParam1</c> is correct as it's not a parameter.
    ///     The reference to <c>nameof(MyParam2)</c> is the correct way to reference a parameter.
    /// </remarks>
    Target MyTarget =>
        t => t
            .RequiresParam(MyParam1) // Analyzer AT0001 should flag this
            .RequiresParam(NotParam1)
            .RequiresParam(nameof(MyParam2));

    /// <summary>
    ///     Another demonstration of incorrect and correct usages of
    ///     <see cref="TargetDefinition.RequiresParam(IEnumerable{string})" />.
    /// </summary>
    /// <remarks>
    ///     This target further illustrates the analyzer's behavior with mixed parameter and non-parameter references.
    ///     The reference to <c>MyParam1</c> directly will be flagged by AT0001.
    /// </remarks>
    Target MyTarget2 =>
        t => t
            .RequiresParam(NotParam1)
            .RequiresParam(MyParam1) // Analyzer AT0001 should flag this
            .RequiresParam(NotParam2);
}

#pragma warning restore AT0001
