USE master;
GO --

IF OBJECT_ID('restify_disInicioSesionServidor') IS NOT NULL
    DISABLE TRIGGER restify_disInicioSesionServidor ON ALL SERVER;

GO --

IF OBJECT_ID('restify_disInicioSesionServidor') IS NOT NULL
    DROP TRIGGER restify_disInicioSesionServidor ON ALL SERVER;

GO --

DECLARE @kill varchar(8000) = '';  
SELECT @kill = @kill + 'kill ' + CONVERT(VARCHAR(5), session_id) + ';'  
    FROM sys.dm_exec_sessions
    WHERE database_id  = DB_ID('restify');

EXEC(@kill);
GO --

EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = 'restify'
GO --

DROP DATABASE IF EXISTS restify;
GO --

DROP LOGIN restify_administrator;
DROP LOGIN restify_user;
DROP LOGIN restify_employee;

GO --