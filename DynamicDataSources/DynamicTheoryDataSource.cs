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
public abstract class DynamicTheoryDataSource(ArgsCode argsCode)
: DynamicDataHolderSource<TheoryData>(argsCode, PropsCode.Expected)
{
    #region Private fields
    private Type? _testDataType = null;
    private HashSet<string> _testCaseNames = [];
    #endregion

    #region Override methods
    #region ResetDataHolder
    public override void ResetDataHolder()
    {
        _testDataType = null;
        _testCaseNames.Clear();
        base.ResetDataHolder();
    }
    #endregion

    #region Add<TTestData>
    protected override void Add<TTestData>(TTestData testData)
    {
        switch (ArgsCode)
        {
            case ArgsCode.Instance:
                add(testData);
                break;
            case ArgsCode.Properties:
                add(testData.ToParams(
                    ArgsCode,
                    PropsCode));
                break;
            default:
                break;
        }

        #region Local methods
        void add<TRow>(TRow row)
        {
            bool isDataHolderTyped =
                IsDataHolderTyped(out TheoryData<TRow>? theoryData);
            bool typeMatches =
                ArgsCode == ArgsCode.Instance ||
                _testDataType == typeof(TTestData);

            Add(isDataHolderTyped && typeMatches,
                testData,
                addWithCheckedName);

            #region Local methods
            void addWithCheckedName(TTestData testData)
            {
                if (_testCaseNames.Add(testData.TestCaseName))
                {
                    theoryData!.Add(row);
                }

            }
            #endregion
        }
        #endregion
    }
    #endregion

    #region InitDataHolder<TTestData>
    protected override void InitDataHolder<TTestData>(TTestData testData)
    {
        _testDataType = typeof(TTestData);
        _testCaseNames = [testData.TestCaseName];

        switch (ArgsCode)
        {
            case ArgsCode.Instance:
                DataHolder = new TheoryData<TTestData>(testData);
                break;
            case ArgsCode.Properties:
                DataHolder = new TheoryData<object?[]>(testData.ToParams(
                    ArgsCode,
                    PropsCode));
                break;
            default:
                break;
        }
    }
    #endregion
    #endregion
}