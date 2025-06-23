CREATE TABLE [dbo].[TareasVehiculo] (
    [TareaId]       INT            IDENTITY (1, 1) NOT NULL,
    [VehiculoId]    INT            NOT NULL,
    [Descripcion]   NVARCHAR (200) NULL,
    [FechaRegistro] DATE           DEFAULT (getdate()) NULL,
    [Realizada]     BIT            DEFAULT ((0)) NULL,
    PRIMARY KEY CLUSTERED ([TareaId] ASC),
    FOREIGN KEY ([VehiculoId]) REFERENCES [dbo].[Vehiculos] ([VehiculoId])
);

