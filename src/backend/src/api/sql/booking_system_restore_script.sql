USE master;
GO

DECLARE @backupFileName VARCHAR(200);
SELECT @backupFileName = m.physical_device_name FROM msdb.dbo.backupset AS s
    INNER JOIN msdb.dbo.backupmediafamily AS m ON s.media_set_id = m.media_set_id
    WHERE s.backup_set_id = 0;

ALTER DATABASE restify SET OFFLINE WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE restify
    FROM DISK = @backupFileName
    WITH FILE = 1, NOUNLOAD, STATS = 5;

GO