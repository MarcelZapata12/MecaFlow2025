CREATE TABLE [dbo].[Usuarios] (
    [UsuarioId]     INT            IDENTITY (1, 1) NOT NULL,
    [Username]      NVARCHAR (50)  NOT NULL,
    [PasswordHash]  NVARCHAR (255) NOT NULL,
    [Correo]        NVARCHAR (100) NULL,
    [FechaCreacion] DATETIME       DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([UsuarioId] ASC),
    UNIQUE NONCLUSTERED ([Username] ASC)
);

