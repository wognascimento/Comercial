using Dapper;

namespace Comercial.Utils;

public class DateOnlyToDateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override DateTime Parse(object value)
    {
        return value switch
        {
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            DateTime dateTime => dateTime,
            _ => Convert.ToDateTime(value)
        };
    }

    public override void SetValue(System.Data.IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value;
    }
}