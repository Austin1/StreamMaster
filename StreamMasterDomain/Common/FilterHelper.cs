﻿using StreamMasterDomain.Filtering;

using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace StreamMasterDomain.Common;

public static class FilterHelper<T> where T : class
{
    private static readonly ConcurrentDictionary<Type, ParameterExpression> ParameterCache = new();
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> PropertyCache = new();

    public static IQueryable<T> ApplyFiltersAndSort(IQueryable<T> query, List<DataTableFilterMetaData>? filters, string orderBy)
    {

        if (filters != null)
        {
            // Apply filters
            foreach (DataTableFilterMetaData filter in filters)
            {
                query = FilterHelper<T>.ApplyFilter(query, filter);
            }
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = query.OrderBy(orderBy);
        }

        return query;
    }
    public static IQueryable<T> ApplyFilter(IQueryable<T> query, DataTableFilterMetaData filter)
    {
        if (!ParameterCache.TryGetValue(typeof(T), out ParameterExpression? parameter))
        {
            parameter = Expression.Parameter(typeof(T), "entity");
            ParameterCache[typeof(T)] = parameter;
        }

        (Type, string FieldName) propertyKey = (typeof(T), filter.FieldName);
        if (!PropertyCache.TryGetValue(propertyKey, out PropertyInfo? property))
        {
            property = typeof(T).GetProperties().FirstOrDefault(p => string.Equals(p.Name, filter.FieldName, StringComparison.OrdinalIgnoreCase));
            if (property != null)
            {
                PropertyCache[propertyKey] = property;
            }
            else
            {
                throw new ArgumentException($"Property {filter.FieldName} not found on type {typeof(T).FullName}");
            }
        }

        Expression propertyAccess = Expression.Property(parameter, property);

        Expression filterExpression = CreateArrayExpression(filter, propertyAccess);
        //filter.MatchMode switch
        //{
        //    //case "channelGroups":
        //    //    filterExpression = CreateArrayExpression(filter, propertyAccess, filter.MatchMode);
        //    //    break;
        //    //case "contains":
        //    //case "startsWith":
        //    //case "endsWith":
        //    //    filterExpression = CreateArrayExpression(filter, propertyAccess, filter.MatchMode);
        //    //    break;
        //    "equals" => Expression.Equal(propertyAccess, Expression.Constant(ConvertValue(filter.Value, property.PropertyType))),
        //    _ => CreateArrayExpression(filter, propertyAccess),
        //};
        Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
        return query.Where(lambda);
    }

    private static MethodInfo? GetMethodCaseInsensitive(Type type, string methodName, Type[] parameterTypes)
    {
        return type.GetMethods()
                   .FirstOrDefault(m => string.Equals(m.Name, methodName, StringComparison.OrdinalIgnoreCase)
                                        && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
    }

    private static Expression CreateArrayExpression(DataTableFilterMetaData filter, Expression propertyAccess)
    {
        string stringValue = filter.Value.ToString() ?? string.Empty;
        if (filter.MatchMode == "channelGroupsMatch")
        {
            filter.MatchMode = "equals";
        }
        List<Expression> containsExpressions = new();

        MethodInfo? methodInfo = GetMethodCaseInsensitive(typeof(string), filter.MatchMode, new[] { typeof(string) });
        if (methodInfo == null)
        {
            throw new InvalidOperationException($"Method {filter.MatchMode} not found on string type.");
        }
        MethodCallExpression toLowerCall = Expression.Call(propertyAccess, typeof(string).GetMethod("ToLower", Type.EmptyTypes));

        if (stringValue.StartsWith("[\"") && stringValue.EndsWith("\"]"))
        {
            string[] values = JsonSerializer.Deserialize<string[]>(stringValue) ?? Array.Empty<string>();
            foreach (string value in values)
            {
                MethodCallExpression matchCall = Expression.Call(toLowerCall, methodInfo, Expression.Constant(value.ToLower()));
                containsExpressions.Add(matchCall);
            }

        }
        else
        {
            MethodCallExpression matchCall = Expression.Call(toLowerCall, methodInfo, Expression.Constant(stringValue.ToLower()));
            containsExpressions.Add(matchCall);
        }

        Expression filterExpression = containsExpressions[0];
        for (int i = 1; i < containsExpressions.Count; i++)
        {
            filterExpression = Expression.OrElse(filterExpression, containsExpressions[i]);
        }

        return filterExpression;
    }

    private static object ConvertValue(object value, Type targetType)
    {
        // Handle null values immediately
        if (value == null)
        {
            return null;
        }


        // If targetType is nullable, get the underlying type
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            targetType = Nullable.GetUnderlyingType(targetType);
        }

        if (targetType == typeof(string))
        {
            return value.ToString();
        }

        if (targetType == typeof(bool))
        {
            return bool.TryParse(value.ToString(), out bool parsedValue) && parsedValue;
        }

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value.ToString());
        }

        // For all other types, attempt to change the type
        return Convert.ChangeType(value, targetType);
    }
}
