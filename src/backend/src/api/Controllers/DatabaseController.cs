﻿using Microsoft.AspNetCore.Mvc;
using Restify.API.Data;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Restify.API.Controllers
{
    [ApiController]
    [ApiVersion(2)]
    [Route("v{version:apiVersion}/[controller]")]
    public class DatabaseController : BaseController
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                string[] sqlStatementBatches =
                    databaseCreationScript.Split("GO", StringSplitOptions.RemoveEmptyEntries);

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
            RestifyDbContext context = _serviceProvider.GetRequiredService<RestifyDbContext>();
            
            bool deleted = context.Database.EnsureDeleted();

            if (deleted)
            {
                return NoContent();
            }

            return NotFound();
        }
    }
}