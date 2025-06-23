CREATE TABLE [dbo].[Permisos] (
    [PermisoId] INT            IDENTITY (1, 1) NOT NULL,
    [Nombre]    NVARCHAR (100) NOT NULL,
    PRIMARY KEY CLUSTERED ([PermisoId] ASC)
);

