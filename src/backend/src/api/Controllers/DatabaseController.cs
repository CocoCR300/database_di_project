using Microsoft.AspNetCore.Mvc;
using Restify.API.Data;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Restify.API.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("v{version:apiVersion}/[controller]")]
public class DatabaseController : BaseController
{
    private readonly IServiceProvider _serviceProvider;
    private const string BACKUP_FOLDER_PATH = "C:/Microsoft SQL Server Database Files/Backup/";

    public DatabaseController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpPost("backup")]
    public ActionResult BackupDatabase()
    {
        RestifyDbContext context = _serviceProvider
            .GetRequiredKeyedService<RestifyDbContext>(RestifyDbContext.SERVER_WIDE_SERVICE_NAME);

        DateTime now = DateTime.Now;
        string backupFileName = string.Format("restify_{0:yyyy_MM_dd}.bak", now);
        string backupFilePath = Path.Combine(BACKUP_FOLDER_PATH, backupFileName);
        string backupName = string.Format("Restify Full Backup ({0:yyyy/MM/dd})", now);
        
        context.Database.ExecuteSqlRaw(
            $"""
             BACKUP DATABASE restify TO DISK = {backupFilePath}
             WITH NOFORMAT, NOINIT,
             NAME = {backupName}, SKIP, NOREWIND, NOUNLOAD, STATS = 10;
             """);
        
        IQueryable<int> backupFilePathQuery = context.Database.SqlQueryRaw<int>(
            $"""
            SELECT backupset.backup_set_id FROM msdb.dbo.backupset
            INNER JOIN msdb.dbo.backupmediafamily AS bmf ON backupset.media_set_id = bmf.media_set_id
            WHERE bmf.physical_device_name = {backupFilePath};
            """);

        int databaseBackupId = backupFilePathQuery.Single();

        return Ok(new DatabaseBackup(databaseBackupId, backupName));
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

    [HttpGet("backup")]
    public ActionResult<List<DatabaseBackup>> GetBackups()
    {
        RestifyDbContext context = _serviceProvider
            .GetRequiredKeyedService<RestifyDbContext>(RestifyDbContext.SERVER_WIDE_SERVICE_NAME);
        IQueryable<DatabaseBackup> backupFilePathQuery = context.Database.SqlQueryRaw<DatabaseBackup>(
            """
            SELECT backup_set_id AS Id, name AS Name FROM msdb.dbo.backupset
            WHERE database_name = 'restify';
            """);

        return Ok(backupFilePathQuery.ToList());
    }
        
    [HttpPost("restore/{databaseBackupId}")]
    public ActionResult RestoreDatabase(int databaseBackupId)
    {
        RestifyDbContext context = _serviceProvider
            .GetRequiredKeyedService<RestifyDbContext>(RestifyDbContext.SERVER_WIDE_SERVICE_NAME);

        IQueryable<string> backupFilePathQuery = context.Database.SqlQueryRaw<string>(
            $"""
            SELECT bmf.physical_device_name FROM msdb.dbo.backupset
            INNER JOIN msdb.dbo.backupmediafamily AS bmf ON backupset.media_set_id = bmf.media_set_id
            WHERE backupset.backup_set_id = {databaseBackupId};
            """);

        string backupFilePath = backupFilePathQuery.Single();
        
        context.Database.ExecuteSqlRaw(
            $"""
            RESTORE DATABASE restify FROM DISK = {backupFilePath}
            WITH FILE = {backupFilePath}, NOUNLOAD, STATS = 5;
            """);
        
        return NoContent();
    }

    public record DatabaseBackup(
        int Id,
        string Name
    );
}