USE master;
GO --

DISABLE TRIGGER restify_disInicioSesionServidor ON ALL SERVER;
GO --

DROP TRIGGER restify_disInicioSesionServidor ON ALL SERVER;
GO --

DECLARE @kill varchar(8000) = '';  
SELECT @kill = @kill + 'kill ' + CONVERT(VARCHAR(5), session_id) + ';'  
    FROM sys.dm_exec_sessions
    WHERE database_id  = DB_ID('restify');

EXEC(@kill);
GO --

DROP DATABASE restify;
GO --

DROP LOGIN restify_administrator;
DROP LOGIN restify_user;
DROP LOGIN restify_employee;
GO --