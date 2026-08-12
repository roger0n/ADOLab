using System.Data;
using Microsoft.Data.SqlClient;

/// <summary>
/// Classe de repositório para gerenciar entidades Aluno no banco de dados.
/// </summary>
public class AlunoRepository : IRepository<Aluno>
{
    /// <summary>
    /// Obtém ou define a string de conexão com o banco de dados.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Inicializa uma nova instância da classe AlunoRepository.
    /// </summary>
    public AlunoRepository(string connectionString)
    {
        ConnectionString = connectionString;
    }

    /// <summary>
    /// Garante que o esquema do banco de dados para a tabela Aluno exista.
    /// </summary>
    public void GarantirEsquema()
    {
        const string ddl = @"
        IF OBJECT_ID('dbo.Alunos', 'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Alunos (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Nome NVARCHAR(100) NOT NULL,
                Idade INT NOT NULL,
                Email NVARCHAR(100) NOT NULL,
                DataNascimento DATE NOT NULL
            );
        END";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(ddl, conn)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 30
        };

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Insere um novo registro de Aluno no banco de dados.
    /// </summary>
    public int Inserir(string nome, int idade, string email, DateTime dataNascimento)
    {
        const string sql = @"
            INSERT INTO dbo.Alunos
                (Nome, Idade, Email, DataNascimento)
            VALUES
                (@Nome, @Idade, @Email, @DataNascimento);

            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = nome;
        cmd.Parameters.Add("@Idade", SqlDbType.Int).Value = idade;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
        cmd.Parameters.Add("@DataNascimento", SqlDbType.Date).Value = dataNascimento.Date;

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>
    /// Recupera uma lista de todos os registros de Aluno.
    /// </summary>
    public List<Aluno> Listar()
    {
        const string sql = @"
            SELECT
                Id,
                Nome,
                Idade,
                Email,
                DataNascimento
            FROM dbo.Alunos
            ORDER BY Id;";

        var alunos = new List<Aluno>();

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            alunos.Add(new Aluno(
                reader.GetInt32(reader.GetOrdinal("Id")),
                reader.GetString(reader.GetOrdinal("Nome")),
                reader.GetInt32(reader.GetOrdinal("Idade")),
                reader.GetString(reader.GetOrdinal("Email")),
                reader.GetDateTime(reader.GetOrdinal("DataNascimento"))
            ));
        }

        return alunos;
    }

    /// <summary>
    /// Atualiza os dados de um registro de Aluno.
    /// </summary>
    public int Atualizar(
        int id,
        string nome,
        int idade,
        string email,
        DateTime dataNascimento)
    {
        const string sql = @"
            UPDATE dbo.Alunos
            SET
                Nome = @Nome,
                Idade = @Idade,
                Email = @Email,
                DataNascimento = @DataNascimento
            WHERE Id = @Id;";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        cmd.Parameters.Add("@Nome", SqlDbType.NVarChar, 100).Value = nome;
        cmd.Parameters.Add("@Idade", SqlDbType.Int).Value = idade;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
        cmd.Parameters.Add("@DataNascimento", SqlDbType.Date).Value = dataNascimento.Date;

        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Exclui um registro de Aluno.
    /// </summary>
    public int Excluir(int id)
    {
        const string sql = @"
            DELETE FROM dbo.Alunos
            WHERE Id = @Id;";

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Busca registros de Aluno com base em uma propriedade e valor.
    /// </summary>
    public List<Aluno> Buscar(string propriedade, object valor)
    {
        var propriedadesPermitidas = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            { "Id", "Id" },
            { "Nome", "Nome" },
            { "Idade", "Idade" },
            { "Email", "Email" },
            { "DataNascimento", "DataNascimento" }
        };

        if (!propriedadesPermitidas.TryGetValue(propriedade, out var coluna))
        {
            throw new ArgumentException(
                "Propriedade inválida para busca.",
                nameof(propriedade));
        }

        string sql = $@"
            SELECT
                Id,
                Nome,
                Idade,
                Email,
                DataNascimento
            FROM dbo.Alunos
            WHERE {coluna} = @Valor
            ORDER BY Id;";

        var alunos = new List<Aluno>();

        using var conn = new SqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new SqlCommand(sql, conn);

        switch (coluna)
        {
            case "Id":
            case "Idade":
                cmd.Parameters.Add("@Valor", SqlDbType.Int).Value =
                    Convert.ToInt32(valor);
                break;

            case "Nome":
            case "Email":
                cmd.Parameters.Add("@Valor", SqlDbType.NVarChar, 100).Value =
                    Convert.ToString(valor) ?? string.Empty;
                break;

            case "DataNascimento":
                cmd.Parameters.Add("@Valor", SqlDbType.Date).Value =
                    Convert.ToDateTime(valor).Date;
                break;
        }

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            alunos.Add(new Aluno(
                reader.GetInt32(reader.GetOrdinal("Id")),
                reader.GetString(reader.GetOrdinal("Nome")),
                reader.GetInt32(reader.GetOrdinal("Idade")),
                reader.GetString(reader.GetOrdinal("Email")),
                reader.GetDateTime(reader.GetOrdinal("DataNascimento"))
            ));
        }

        return alunos;
    }
}
