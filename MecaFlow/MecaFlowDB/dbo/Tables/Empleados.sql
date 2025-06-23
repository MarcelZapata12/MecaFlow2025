CREATE TABLE [dbo].[Empleados] (
    [EmpleadoId]    INT            IDENTITY (1, 1) NOT NULL,
    [Nombre]        NVARCHAR (100) NOT NULL,
    [Cedula]        NVARCHAR (20)  NULL,
    [Correo]        NVARCHAR (100) NULL,
    [Puesto]        NVARCHAR (50)  NULL,
    [FechaIngreso]  DATE           NULL,
    [Activo]        BIT            DEFAULT ((1)) NULL,
    [FechaRegistro] DATETIME       DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([EmpleadoId] ASC),
    UNIQUE NONCLUSTERED ([Cedula] ASC)
);

