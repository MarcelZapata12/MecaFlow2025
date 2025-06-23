CREATE TABLE [dbo].[Asistencias] (
    [AsistenciaId] INT      IDENTITY (1, 1) NOT NULL,
    [EmpleadoId]   INT      NOT NULL,
    [Fecha]        DATE     NOT NULL,
    [HoraEntrada]  TIME (7) NULL,
    [HoraSalida]   TIME (7) NULL,
    PRIMARY KEY CLUSTERED ([AsistenciaId] ASC),
    FOREIGN KEY ([EmpleadoId]) REFERENCES [dbo].[Empleados] ([EmpleadoId])
);

