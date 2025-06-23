CREATE TABLE [dbo].[UsuarioRoles] (
    [UsuarioId] INT NOT NULL,
    [RolId]     INT NOT NULL,
    PRIMARY KEY CLUSTERED ([UsuarioId] ASC, [RolId] ASC),
    FOREIGN KEY ([RolId]) REFERENCES [dbo].[Roles] ([RolId]),
    FOREIGN KEY ([UsuarioId]) REFERENCES [dbo].[Usuarios] ([UsuarioId])
);

