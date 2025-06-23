CREATE TABLE [dbo].[ReportesFinancieros] (
    [ReporteId]     INT             IDENTITY (1, 1) NOT NULL,
    [Fecha]         DATE            NOT NULL,
    [TotalIngresos] DECIMAL (12, 2) NULL,
    [TotalGastos]   DECIMAL (12, 2) NULL,
    [Observaciones] NVARCHAR (500)  NULL,
    PRIMARY KEY CLUSTERED ([ReporteId] ASC)
);

