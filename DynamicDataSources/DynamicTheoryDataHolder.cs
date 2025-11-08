// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace CsabaDu.DynamicTestData.Lite.xUnit.DynamicDataSources;

/// <summary>
/// Abstract base class for providing dynamic theory test data sources with type-safe argument handling.
/// </summary>
/// <remarks>
/// <para>
/// This class serves as a foundation for creating strongly-typed test data sources
/// that can be used with xUnit theory tests. It maintains type consistency across
/// all added test data and provides various methods for adding different kinds of
/// test cases (normal, return value, and exception cases).
/// </para>
/// <para>
/// The class ensures all test data added to a single instance maintains consistent
/// generic type testParams through runtime checks.
/// </para>
/// </remarks>
/// <param name="argsCode">The strategy for converting test data to method arguments</param>
public abstract class DynamicTheoryDataHolder(ArgsCode argsCode)
: DynamicDataSource<TheoryData>(
    argsCode,
    PropsCode.Expected)
{
    #region Private fields
    private Type? _testDataType = null;
    private readonly HashSet<string> testCaseNames = [];
    #endregion

    #region Override methods
    #region ResetDataHolder
    public override void ResetDataHolder()
    {
        _testDataType = null;
        testCaseNames.Clear();
        base.ResetDataHolder();
    }
    #endregion

    #region Add
    protected override void Add<TTestData>(TTestData testData)
    {
        switch (ArgsCode)
        {
            case ArgsCode.Instance:
                add(() => testData);
                break;
            case ArgsCode.Properties:
                add(() => testData.ToParams(
                    ArgsCode,
                    PropsCode));
                break;
            default:
                break;
        }

        #region Local methods
        void add<TRow>(Func<TRow> getRow)
        {
            if (DataHolder is not TheoryData<TRow> theoryData)
            {
                theoryData = ArgsCode switch
                {
                    ArgsCode.Instance
                    or ArgsCode.Properties
                    when _testDataType == typeof(TTestData) => initTheoryData<TRow>(),
                    _ => throw new InvalidOperationException(
                        "'ArgsCode' property value is invalid.",
                        ArgsCode.GetInvalidEnumArgumentException(nameof(ArgsCode))),
                };
            };

            var testCaseName = testData.GetTestCaseName();

            if (testCaseNames.Add(testCaseName))
            {
                theoryData.Add(getRow());
            }
        }

        TheoryData<TRow> initTheoryData<TRow>()
        {
            testCaseNames.Clear();
            _testDataType = typeof(TTestData);

            return [];
        }
        #endregion
    }
    #endregion
    #endregion
}