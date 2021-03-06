﻿using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Dashboard.Azure
{
    public static partial class TableQueryUtil
    {
        
        public static string Column(string columnName, byte[] value, ColumnOperator op = ColumnOperator.Equal)
        {
            return TableQuery.GenerateFilterConditionForBinary(
                columnName,
                ToQueryComparison(op),
                value);
        }
        
        public static string Column(string columnName, long value, ColumnOperator op = ColumnOperator.Equal)
        {
            return TableQuery.GenerateFilterConditionForLong(
                columnName,
                ToQueryComparison(op),
                value);
        }
        
        public static string Column(string columnName, bool value, ColumnOperator op = ColumnOperator.Equal)
        {
            return TableQuery.GenerateFilterConditionForBool(
                columnName,
                ToQueryComparison(op),
                value);
        }
        
        public static string Column(string columnName, int value, ColumnOperator op = ColumnOperator.Equal)
        {
            return TableQuery.GenerateFilterConditionForInt(
                columnName,
                ToQueryComparison(op),
                value);
        }
        
        public static string Column(string columnName, Guid value, ColumnOperator op = ColumnOperator.Equal)
        {
            return TableQuery.GenerateFilterConditionForGuid(
                columnName,
                ToQueryComparison(op),
                value);
        }
        
        public static string Column(string columnName, string value, ColumnOperator op = ColumnOperator.Equal)
        {
            return TableQuery.GenerateFilterCondition(
                columnName,
                ToQueryComparison(op),
                value);
        }
        
        public static string Column(string columnName, DateTimeOffset value, ColumnOperator op = ColumnOperator.Equal)
        {
            return TableQuery.GenerateFilterConditionForDate(
                columnName,
                ToQueryComparison(op),
                value);
        }
        
        public static string Column(string columnName, double value, ColumnOperator op = ColumnOperator.Equal)
        {
            return TableQuery.GenerateFilterConditionForDouble(
                columnName,
                ToQueryComparison(op),
                value);
        }
    }
}
