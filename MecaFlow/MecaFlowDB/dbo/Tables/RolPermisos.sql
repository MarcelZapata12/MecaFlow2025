CREATE TABLE [dbo].[RolPermisos] (
    [RolId]     INT NOT NULL,
    [PermisoId] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([RolId] ASC, [PermisoId] ASC),
    FOREIGN KEY ([PermisoId]) REFERENCES [dbo].[Permisos] ([PermisoId]),
    FOREIGN KEY ([RolId]) REFERENCES [dbo].[Roles] ([RolId])
);

