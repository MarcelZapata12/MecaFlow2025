CREATE TABLE [dbo].[CuentasClientes] (
    [CuentaClienteId] INT            IDENTITY (1, 1) NOT NULL,
    [ClienteId]       INT            NOT NULL,
    [Username]        NVARCHAR (50)  NOT NULL,
    [PasswordHash]    NVARCHAR (255) NOT NULL,
    [FechaRegistro]   DATETIME       DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([CuentaClienteId] ASC),
    FOREIGN KEY ([ClienteId]) REFERENCES [dbo].[Clientes] ([ClienteId]),
    UNIQUE NONCLUSTERED ([Username] ASC)
);

