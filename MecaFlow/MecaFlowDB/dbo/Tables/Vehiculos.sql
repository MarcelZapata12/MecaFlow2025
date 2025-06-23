CREATE TABLE [dbo].[Vehiculos] (
    [VehiculoId]    INT           IDENTITY (1, 1) NOT NULL,
    [Placa]         NVARCHAR (20) NOT NULL,
    [Marca]         NVARCHAR (50) NULL,
    [Modelo]        NVARCHAR (50) NULL,
    [Anio]          INT           NULL,
    [ClienteId]     INT           NOT NULL,
    [FechaRegistro] DATETIME      DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([VehiculoId] ASC),
    FOREIGN KEY ([ClienteId]) REFERENCES [dbo].[Clientes] ([ClienteId]),
    UNIQUE NONCLUSTERED ([Placa] ASC)
);

