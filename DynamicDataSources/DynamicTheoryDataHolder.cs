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
    private readonly HashSet<string> TestCaseNames = [];
    #endregion

    #region Override methods
    #region ResetDataHolder
    public override void ResetDataHolder()
    {
        _testDataType = null;
        TestCaseNames.Clear();
        base.ResetDataHolder();
    }
    #endregion

    #region Add
    protected override void Add<TTestData>(TTestData testData)
    {
        switch (ArgsCode)
        {
            case ArgsCode.Instance:
                add(testData);
                break;
            case ArgsCode.Properties:
                var testDataToParams =
                    testData.ToParams(ArgsCode, PropsCode);
                add(testDataToParams);
                break;
            default:
                break;
        }

        #region Local methods
        void add<TRow>(TRow row)
        {
            if (DataHolder is not TheoryData<TRow> theoryData)
            {
                theoryData = initTheoryData<TRow>();
            }

            if (ArgsCode == ArgsCode.Properties)
            {
                var testDataType = typeof(TTestData);

                if (_testDataType == null)
                {
                    setTestDataType();
                }

                if (_testDataType != testDataType)
                {
                    theoryData = initTheoryData<TRow>();
                    setTestDataType();
                }

                #region Local methods
                void setTestDataType()
                => _testDataType = testDataType;
                #endregion
            }

            var testCaseName = testData.GetTestCaseName();

            if (TestCaseNames.Add(testCaseName))
            {
                theoryData.Add(row);
            }
        }

        TheoryData<TRow> initTheoryData<TRow>()
        {
            TestCaseNames.Clear();
            return [];
        }
        #endregion
    }
    #endregion
    #endregion
}