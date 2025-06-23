CREATE TABLE [dbo].[Roles] (
    [RolId]  INT           IDENTITY (1, 1) NOT NULL,
    [Nombre] NVARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([RolId] ASC)
);

