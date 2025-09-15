-- restore_all.sql (generated)
-- Run on Server B:
--   /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1,1433 -U odix_admin -P '***' -C -i /var/opt/mssql/restore/restore_all.sql
SET NOCOUNT ON; SET XACT_ABORT ON;
PRINT 'Starting all restores...';
