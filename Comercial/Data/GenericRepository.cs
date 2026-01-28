using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dapper;
using Npgsql;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comercial.Data;

public class GenericRepository
{
    private static (string TableName, PropertyInfo KeyProperty, IEnumerable<PropertyInfo> Properties) GetInfo<T>()
    {
        var type = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        if (tableAttr == null)
            throw new InvalidOperationException($"A classe {type.Name} não possui o atributo [Table].");

        var props = type.GetProperties()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .ToList();

        var key = props.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
        if (key == null)
            throw new InvalidOperationException($"A classe {type.Name} precisa de uma propriedade [Key].");

        return (tableAttr.Name, key, props);
    }

    private static string GetColumnName(PropertyInfo prop)
    {
        return prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name.ToLower();
    }

    // CREATE
    public async Task<long> InsertAsync<T>(IDbConnection conn, T entity)
    {
        var info = GetInfo<T>();
        try
        {
            var cols = info.Properties.Where(p => p != info.KeyProperty).Select(GetColumnName);
            var vals = info.Properties.Where(p => p != info.KeyProperty).Select(p => "@" + p.Name);

            var sql = $"INSERT INTO {info.TableName} ({string.Join(", ", cols)}) " +
                      $"VALUES ({string.Join(", ", vals)}) RETURNING {GetColumnName(info.KeyProperty)};";

            return await conn.ExecuteScalarAsync<long>(sql, entity);
        }
        catch (PostgresException ex)
        {
            throw new RepositoryException($"Erro no banco de dados ({ex.SqlState}): {ex.MessageText}", ex);
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Erro ao inserir em {info.TableName}: {ex.Message}", ex);
        }
    }

    // UPDATE
    public async Task<int> UpdateAsync<T>(IDbConnection conn, T entity)
    {
        var info = GetInfo<T>();
        try
        {
            var setClause = string.Join(", ",
                info.Properties.Where(p => p != info.KeyProperty)
                .Select(p => $"{GetColumnName(p)} = @{p.Name}"));

            var sql = $"UPDATE {info.TableName} SET {setClause} WHERE {GetColumnName(info.KeyProperty)} = @{info.KeyProperty.Name}";

            return await conn.ExecuteAsync(sql, entity);
        }
        catch (PostgresException ex)
        {
            throw new RepositoryException($"Erro no banco de dados ({ex.SqlState}): {ex.MessageText}", ex);
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Erro ao atualizar em {info.TableName}: {ex.Message}", ex);
        }
    }

    // DELETE
    public async Task<int> DeleteAsync<T>(IDbConnection conn, object id)
    {
        var info = GetInfo<T>();
        try
        {
            var sql = $"DELETE FROM {info.TableName} WHERE {GetColumnName(info.KeyProperty)} = @id";
            return await conn.ExecuteAsync(sql, new { id });
        }
        catch (PostgresException ex)
        {
            throw new RepositoryException($"Erro no banco de dados ({ex.SqlState}): {ex.MessageText}", ex);
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Erro ao deletar em {info.TableName}: {ex.Message}", ex);
        }
    }

    // GET BY ID
    public async Task<T?> GetByIdAsync<T>(IDbConnection conn, object id)
    {
        var info = GetInfo<T>();
        try
        {
            var sql = $"SELECT * FROM {info.TableName} WHERE {GetColumnName(info.KeyProperty)} = @id";
            return await conn.QueryFirstOrDefaultAsync<T>(sql, new { id });
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Erro ao buscar em {info.TableName}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<T>> GetWhereAsync<T>(
        IDbConnection conn,
        IDictionary<string, object>? filtros = null, // filtros dinâmicos: coluna -> valor
        string? orderBy = null,
        bool descending = false)
        {
            var info = GetInfo<T>();

            try
            {
                var validColumns = typeof(T)
                    .GetProperties()
                    .Select(p => p.Name.ToLower())
                    .ToHashSet();

                var sql = $"SELECT * FROM {info.TableName}";

                var whereClauses = new List<string>();
                var parameters = new DynamicParameters();

                if (filtros != null)
                {
                    foreach (var filtro in filtros)
                    {
                        var coluna = filtro.Key.ToLower();
                        if (!validColumns.Contains(coluna))
                            continue; // ignora colunas inválidas

                        var paramName = $"@{coluna}";
                        whereClauses.Add($"{coluna} = {paramName}");
                        parameters.Add(paramName, filtro.Value);
                    }
                }

                if (whereClauses.Any())
                    sql += " WHERE " + string.Join(" AND ", whereClauses);

                // Ordenação
                var orderColumn = validColumns.Contains((orderBy ?? "").ToLower())
                    ? orderBy
                    : validColumns.First(); // padrão: primeira propriedade

                var direction = descending ? "DESC" : "ASC";
                sql += $" ORDER BY {orderColumn} {direction}";

                return await conn.QueryAsync<T>(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Erro ao listar {info.TableName}: {ex.Message}", ex);
            }
    }

    // GET ALL
    public async Task<IEnumerable<T>> GetAllAsync<T>(IDbConnection conn, string? orderBy = null, bool descending = false)
    {
        var info = GetInfo<T>();

        try
        {
            // 🔒 Lista de colunas válidas obtidas via reflexão
            var validColumns = typeof(T)
                .GetProperties()
                .Select(p => p.Name.ToLower())
                .ToHashSet();

            // 🔎 Verifica se a coluna passada é válida
            var orderColumn = validColumns.Contains((orderBy ?? "").ToLower())
                ? orderBy
                : validColumns.First(); // padrão: primeira propriedade da model

            var direction = descending ? "DESC" : "ASC";

            var sql = $"SELECT * FROM {info.TableName} ORDER BY {orderColumn} {direction}";

            return await conn.QueryAsync<T>(sql);
        }
        catch (Exception ex)
        {
            throw new RepositoryException($"Erro ao listar {info.TableName}: {ex.Message}", ex);
        }
    }
}
