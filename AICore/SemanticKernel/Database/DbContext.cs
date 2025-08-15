using System.Data;
using AICore.Classes;
using Dapper;
using Npgsql;

namespace AICore.SemanticKernel.Database;

public class DbContext
{
    public IDbConnection CreateConnection()
    {
        NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(Config.Instance.ConnectionString);
        dataSourceBuilder.UseVector();
        NpgsqlDataSource dataSource = dataSourceBuilder.Build();
        return dataSource.OpenConnection();
    }

    public async Task Init()
    {
        await CreateDatabase();
        await ConfigureVectors();
    }


    public async Task CreateSecurityObjectTable()
    {
        string securityObjectTableSql = @"
                                            CREATE TABLE IF NOT EXISTS public.securityobjects
(
    activedirectoryid uuid NOT NULL,
    fullname text COLLATE pg_catalog.""default"" NOT NULL,
    username text COLLATE pg_catalog.""default"" NOT NULL,
    pass text COLLATE pg_catalog.""default"" NOT NULL,
    emailaddress text COLLATE pg_catalog.""default"" NOT NULL,
    isgroup smallint NOT NULL,
    isactive smallint NOT NULL,
    forename text COLLATE pg_catalog.""default"",
    surname text COLLATE pg_catalog.""default"",
    CONSTRAINT pk_securityobjects PRIMARY KEY (activedirectoryid)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.securityobjects
    OWNER to postgres;
";
        await RunSql(securityObjectTableSql);
    }

    public async Task CreateConversationTable()
    {
        string conversationTableSql = @"

CREATE TABLE IF NOT EXISTS public.conversation
(
    pkconversationid uuid NOT NULL,
    fksecurityobjectowner uuid NOT NULL,
    fkparentid uuid,
    title text COLLATE pg_catalog.""default"" NOT NULL,
    createdat timestamp(3) without time zone NOT NULL,
    description text COLLATE pg_catalog.""default"" NOT NULL,
    serializedchat text COLLATE pg_catalog.""default"" NOT NULL,
    CONSTRAINT pk_conversation PRIMARY KEY (pkconversationid),
    CONSTRAINT fk_conversation_securityobjects FOREIGN KEY (fksecurityobjectowner)
        REFERENCES public.securityobjects (activedirectoryid) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.conversation
    OWNER to postgres;
";

        await RunSql(conversationTableSql);
    }

//    public async Task CreateConversationTypes()
//    {
//        string conversationtypeTableSql = @"

//CREATE TABLE IF NOT EXISTS public.conversationtypes
//(
//    pkconversationtypeid integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
//    ""Name"" text COLLATE pg_catalog.""default"" NOT NULL,
//    CONSTRAINT pk_conversationtypes PRIMARY KEY (pkconversationtypeid)
//)

//TABLESPACE pg_default;

//ALTER TABLE IF EXISTS public.conversationtypes
//    OWNER to postgres;
//";
//        await RunSql(conversationtypeTableSql);
//    }

//    public async Task CreateRole()
//    {
//        string rolesql = @"
//CREATE TABLE IF NOT EXISTS public.role
//(
//    pkroleid integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
//    ""Name"" text COLLATE pg_catalog.""default"" NOT NULL,
//    CONSTRAINT pk_role PRIMARY KEY (pkroleid)
//)

//TABLESPACE pg_default;

//ALTER TABLE IF EXISTS public.role
//    OWNER to postgres;

//";
//        await RunSql(rolesql);
//    }

//    public async Task CreateEntryTable()
//    {
//        string entrysql = @"

//CREATE TABLE IF NOT EXISTS public.entry
//(
//    pkentryid uuid NOT NULL,
//    fkconversationid uuid NOT NULL,
//    fkconversationtypeid integer NOT NULL,
//    fkroleid integer NOT NULL,
//    text text COLLATE pg_catalog.""default"",
//    numberofresults integer,
//    resulttext text COLLATE pg_catalog.""default"",
//    sequence integer NOT NULL,
//    filedata bytea,
//    ishidden smallint NOT NULL,
//    OptionalField1  text COLLATE pg_catalog.""default"",
//    CONSTRAINT pk_entry PRIMARY KEY (pkentryid),
//    CONSTRAINT fk_entry_conversation FOREIGN KEY (fkconversationid)
//        REFERENCES public.conversation (pkconversationid) MATCH SIMPLE
//        ON UPDATE NO ACTION
//        ON DELETE CASCADE,
//    CONSTRAINT fk_entry_conversationtypes FOREIGN KEY (fkconversationtypeid)
//        REFERENCES public.conversationtypes (pkconversationtypeid) MATCH SIMPLE
//        ON UPDATE NO ACTION
//        ON DELETE NO ACTION,
//    CONSTRAINT fk_entry_role FOREIGN KEY (fkroleid)
//        REFERENCES public.role (pkroleid) MATCH SIMPLE
//        ON UPDATE NO ACTION
//        ON DELETE NO ACTION
//)

//TABLESPACE pg_default;

//ALTER TABLE IF EXISTS public.entry
//    OWNER to postgres;
//";
//        await RunSql(entrysql);
//    }

//    public async Task CreateFetchDocs()
//    {
//        string fetchedDocumentsSql = @"

//CREATE TABLE IF NOT EXISTS public.fetcheddocs
//(
//    pkfetchdocid uuid NOT NULL,
//    memorykey text COLLATE pg_catalog.""default"" NOT NULL,
//    uri text COLLATE pg_catalog.""default"" NOT NULL,
//    body text COLLATE pg_catalog.""default"" NOT NULL,
//    fkentryid uuid NOT NULL,
//    summary text COLLATE pg_catalog.""default"",
//    CONSTRAINT pk_fetcheddocs PRIMARY KEY (pkfetchdocid),
//    CONSTRAINT fk_fetcheddocs_entry FOREIGN KEY (fkentryid)
//        REFERENCES public.entry (pkentryid) MATCH SIMPLE
//        ON UPDATE NO ACTION
//        ON DELETE CASCADE
//)

//TABLESPACE pg_default;

//ALTER TABLE IF EXISTS public.fetcheddocs
//    OWNER to postgres;
//";
//        await RunSql(fetchedDocumentsSql);
//    }

    private async Task CreateDatabase()
    {
        // create database if it doesn't exist
        await using NpgsqlConnection connection =
            new NpgsqlConnection(Config.Instance.ConnectionString.Replace(Config.Instance.DatabaseName, "postgres"));
        string sqlDbCount =
            $"SELECT EXISTS(SELECT datname FROM pg_catalog.pg_database WHERE datname = '{Config.Instance.DatabaseName}');";
        int dbCount = await connection.ExecuteScalarAsync<int>(sqlDbCount);
        if (dbCount == 0)
        {
            string sql = $"CREATE DATABASE \"{Config.Instance.DatabaseName}\"";
            await connection.ExecuteAsync(sql);
        }

        await CreateSecurityObjectTable();
        await CreateConversationTable();
        //await CreateConversationTypes();
        //await CreateRole();
        //await CreateEntryTable();
        //await CreateFetchDocs();


        //using (NewsReaderContext ctx = new NewsReaderContext())
        //{
        //    bool dat = ctx.Conversationtypes.Any();
        //    if (!dat)
        //    {
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "Prompt" });
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "OllamaResult" });
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "WebSearch" });
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "UrlFetch" });
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "ImportText" });
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "UploadFile" });
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "Ask" });
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "Comfy" });
        //        ctx.Conversationtypes.Add(new Conversationtype { Name = "ImageToText" });
        //        await ctx.SaveChangesAsync();
        //    }

        //    bool roles = ctx.Roles.Any();
        //    if (!roles)
        //    {
        //        ctx.Roles.Add(new Role { Name = "user" });
        //        ctx.Roles.Add(new Role { Name = "assistant" });
        //        ctx.Roles.Add(new Role { Name = "system" });
        //        await ctx.SaveChangesAsync();
        //    }
        //}
    }

    private async Task ConfigureVectors()
    {
        await using NpgsqlConnection? connection = CreateConnection() as NpgsqlConnection;

        await using (NpgsqlCommand cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS vector", connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }

        if (connection != null)
            await connection.ReloadTypesAsync();
    }

    private async Task RunSql(string sql)
    {
        await using NpgsqlConnection? connection = CreateConnection() as NpgsqlConnection;

        await using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
        {
            await cmd.ExecuteNonQueryAsync();
        }
    }
}