CREATE TABLE [dbo].[Facturas] (
    [FacturaId]     INT             IDENTITY (1, 1) NOT NULL,
    [ClienteId]     INT             NOT NULL,
    [VehiculoId]    INT             NOT NULL,
    [Fecha]         DATE            NOT NULL,
    [MontoTotal]    DECIMAL (10, 2) NOT NULL,
    [Observaciones] NVARCHAR (500)  NULL,
    PRIMARY KEY CLUSTERED ([FacturaId] ASC),
    FOREIGN KEY ([ClienteId]) REFERENCES [dbo].[Clientes] ([ClienteId]),
    FOREIGN KEY ([VehiculoId]) REFERENCES [dbo].[Vehiculos] ([VehiculoId])
);

