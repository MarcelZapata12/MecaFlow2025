CREATE TABLE [dbo].[Diagnosticos] (
    [DiagnosticoId] INT            IDENTITY (1, 1) NOT NULL,
    [VehiculoId]    INT            NOT NULL,
    [Fecha]         DATE           NOT NULL,
    [Detalle]       NVARCHAR (500) NULL,
    [EmpleadoId]    INT            NULL,
    PRIMARY KEY CLUSTERED ([DiagnosticoId] ASC),
    FOREIGN KEY ([EmpleadoId]) REFERENCES [dbo].[Empleados] ([EmpleadoId]),
    FOREIGN KEY ([VehiculoId]) REFERENCES [dbo].[Vehiculos] ([VehiculoId])
);

