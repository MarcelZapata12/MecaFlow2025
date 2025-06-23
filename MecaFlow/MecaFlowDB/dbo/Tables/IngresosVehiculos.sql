CREATE TABLE [dbo].[IngresosVehiculos] (
    [IngresoId]    INT            IDENTITY (1, 1) NOT NULL,
    [VehiculoId]   INT            NOT NULL,
    [FechaIngreso] DATETIME       NOT NULL,
    [Motivo]       NVARCHAR (255) NULL,
    PRIMARY KEY CLUSTERED ([IngresoId] ASC),
    FOREIGN KEY ([VehiculoId]) REFERENCES [dbo].[Vehiculos] ([VehiculoId])
);

