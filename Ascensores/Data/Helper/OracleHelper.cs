using Oracle.ManagedDataAccess.Client;

namespace Ascensores.Data.Helper
{
    public class OracleHelper
    {
        private readonly string _connString;
        public OracleHelper(string connString)
        {
            _connString = connString;
        }


        public OracleConnection GetConnection()
        {
            return new OracleConnection(_connString);
        }


        public async Task<int> ExecuteNonQueryAsync(string sql, params OracleParameter[] parameters)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var transaction = conn.BeginTransaction();
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            int rows = await cmd.ExecuteNonQueryAsync();
            transaction.Commit(); 
            return rows;
        }



        public async Task<object> ExecuteScalarAsync(string sql, params OracleParameter[] parameters)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteScalarAsync();
        }


        public async Task<List<T>> QueryAsync<T>(string sql, Func<OracleDataReader, T> map, params OracleParameter[] parameters)
        {
            var list = new List<T>();
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(map(reader));
            }
            return list;
        }
    }
}
