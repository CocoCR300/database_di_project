USE master;
GO

DECLARE @now DATE = '{0}';
DECLARE @dateString VARCHAR(30) = FORMAT(@now, 'yyyy_MM_dd');
DECLARE @backupFileName VARCHAR(50);
DECLARE @backupName VARCHAR(40);

EXEC xp_sprintf @backupName OUTPUT, 'restify_full_%s.bak', @dateString;

SET @dateString = FORMAT(@now, 'yyyy/MM/dd');
EXEC xp_sprintf @backupName OUTPUT, 'Restify Full Backup (%s)', @dateString;

BACKUP DATABASE aeropuerto
    TO DISK = '/mnt/data/Microsoft SQL Server Database Files/Backup/hola.bak'
    WITH NOFORMAT, NOINIT,
    NAME = 'idontcare', SKIP, NOREWIND, NOUNLOAD, STATS = 10;
GO

BACKUP DATABASE restify
    TO DISK = @backupFileName
    WITH NOFORMAT, NOINIT,
    NAME = @backupName, SKIP, NOREWIND, NOUNLOAD, STATS = 10;
GO

SELECT * FROM msdb.dbo.backupset AS s
    INNER JOIN msdb.dbo.backupmediafamily AS m ON s.media_set_id = m.media_set_id
    WHERE database_name = 'aeropuerto';