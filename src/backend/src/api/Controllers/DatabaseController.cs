using Microsoft.AspNetCore.Mvc;
using Restify.API.Data;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Route("v{version:apiVersion}/[controller]")]
public class DatabaseController : BaseController
{
    private const string BackupFileName = "restify_full_backup.bak";
    private readonly IServiceProvider _serviceProvider;
    private readonly string _backupFolderPath;

    public DatabaseController(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _backupFolderPath = configuration["BackupFolderPath"]!;
    }

    [HttpPost("backup")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> BackupDatabase()
    {
        RestifyDbContext context = _serviceProvider
            .GetRequiredKeyedService<RestifyDbContext>(RestifyDbContext.SERVER_WIDE_SERVICE_NAME);

        DateTime now = DateTime.Now;
        string backupFilePath = Path.Combine(_backupFolderPath, BackupFileName);
        string backupName = string.Format("Restify Full Backup ({0:yyyy/MM/dd - HH:mm:ss})", now);

        await context.CreateDatabaseBackup(backupName, backupFilePath);
        return NoContent();
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult CreateDatabase()
    {
        RestifyDbContext context = _serviceProvider
            .GetRequiredKeyedService<RestifyDbContext>(RestifyDbContext.SERVER_WIDE_SERVICE_NAME);
            
        IDbContextTransaction? transaction = null;
        string databaseCreationScriptPath = Path.Combine("sql", "booking_system.sql");

        IQueryable<string> databaseNameQuery = context.Database.SqlQuery<string>(
            $"""
             SELECT TOP(1) name AS Value FROM sys.databases
             WHERE name = {RestifyDbContext.DATABASE_NAME}
             """
        );
            
        if (databaseNameQuery.FirstOrDefault() != null)
        {
            return Conflict();
        }

        try
        {
            string databaseCreationScript = System.IO.File.ReadAllText(databaseCreationScriptPath);
            // Database administrators hate this simple trick
            // Seriously, why isn't it valid syntax to put a semicolon after a GO statement??
            // I can't just look for "GO" because it would cut a LOGON trigger in half D:
            string[] sqlStatementBatches =
                databaseCreationScript.Split("GO --", StringSplitOptions.RemoveEmptyEntries);

            context.Database.ExecuteSqlRaw(sqlStatementBatches[0]);

            transaction = context.Database.BeginTransaction();
            foreach (string sqlStatementBatch in sqlStatementBatches.Skip(1))
            {
                context.Database.ExecuteSqlRaw(sqlStatementBatch);
            }

            transaction.Commit();

            return NoContent();
        }
        catch (SqlException)
        {
            transaction?.Rollback();
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    [HttpPost("drop")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult DropDatabase()
    {
        RestifyDbContext context = _serviceProvider
            .GetRequiredKeyedService<RestifyDbContext>(RestifyDbContext.SERVER_WIDE_SERVICE_NAME);
            
        string databaseDropScriptPath = Path.Combine("sql", "booking_system_drop_script.sql");

        IQueryable<string> databaseNameQuery = context.Database.SqlQuery<string>(
            $"""
             SELECT TOP(1) name AS Value FROM sys.databases
             WHERE name = {RestifyDbContext.DATABASE_NAME}
             """
        );
            
        if (databaseNameQuery.FirstOrDefault() == null)
        {
            return NotFound();
        }

        string databaseCreationScript = System.IO.File.ReadAllText(databaseDropScriptPath);
        string[] sqlStatementBatches =
            databaseCreationScript.Split("GO --", StringSplitOptions.RemoveEmptyEntries);

        context.Database.ExecuteSqlRaw(sqlStatementBatches[0]);

        foreach (string sqlStatementBatch in sqlStatementBatches.Skip(1))
        {
            context.Database.ExecuteSqlRaw(sqlStatementBatch);
        }

        return NoContent();
    }
        
    [HttpPost("restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult RestoreDatabase()
    {
        RestifyDbContext context = _serviceProvider
            .GetRequiredKeyedService<RestifyDbContext>(RestifyDbContext.SERVER_WIDE_SERVICE_NAME);

        string backupFilePath = Path.Combine(_backupFolderPath, BackupFileName);
        if (!System.IO.File.Exists(backupFilePath))
        {
            return NotFound();
        }
        
        context.Database.ExecuteSql(
            $"""
            ALTER DATABASE restify SET OFFLINE WITH ROLLBACK IMMEDIATE;
                  
            RESTORE DATABASE restify FROM DISK = {backupFilePath}
            WITH REPLACE, FILE = {backupFilePath};
            
            ALTER DATABASE restify SET ONLINE;
            """);
        
        return NoContent();
    }
}