using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Dapper;
using Npgsql;
using APIEstados.Models;

namespace APIEstados.Data
{
    public class EstadosRepository
    {
        public readonly IConfiguration _configuration;

        public EstadosRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<Estado>> Get(string siglaEstado = null)
        {
            var conexao = new NpgsqlConnection(
                _configuration.GetConnectionString("BaseDadosGeograficos"));

            var strbQuery = new StringBuilder();
            strbQuery.Append(
                "SELECT * " +
                "FROM \"Estados\" E " +
                "INNER JOIN \"Regioes\" R ON R.\"IdRegiao\" = E.\"IdRegiao\" ");
            
            object filtroEstado = null;
            if (!String.IsNullOrWhiteSpace(siglaEstado))
            {
                strbQuery.Append("WHERE (E.\"SiglaEstado\" = @Sigla) ");
                filtroEstado = new { Sigla = siglaEstado };
            }

            strbQuery.Append("ORDER BY E.\"NomeEstado\"");

            return await conexao.QueryAsync<Estado, Regiao, Estado>(
                strbQuery.ToString(),
                param: filtroEstado,
                map: (estado, regiao) =>
                {
                    estado.DadosRegiao = regiao;
                    return estado;
                },
                splitOn: "SiglaEstado,IdRegiao");
        }
    }
}