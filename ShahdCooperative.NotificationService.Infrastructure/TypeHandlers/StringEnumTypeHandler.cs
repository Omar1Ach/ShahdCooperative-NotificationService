using System.Data;
using Dapper;

namespace ShahdCooperative.NotificationService.Infrastructure.TypeHandlers;

/// <summary>
/// Dapper TypeHandler for mapping enums as strings in the database
/// </summary>
public class StringEnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, Enum
{
    public override T Parse(object value)
    {
        if (value == null || value is DBNull)
        {
            return default;
        }

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return default;
        }

        if (Enum.TryParse<T>(stringValue, true, out var result))
        {
            return result;
        }

        throw new InvalidCastException($"Cannot convert '{stringValue}' to {typeof(T).Name}");
    }

    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value.ToString();
        parameter.DbType = DbType.String;
    }
}
