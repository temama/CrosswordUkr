
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server Compact Edition
-- --------------------------------------------------
-- Date Created: 09/08/2017 21:25:43
-- Generated from EDMX file: D:\Work\CrosswordUkr\XWordsUrkAdminConsole\XWordsUrkAdminConsole\Models\XWordsAdminModel.edmx
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    ALTER TABLE [Clues] DROP CONSTRAINT [FK_ClueWord];
GO
    ALTER TABLE [Medias] DROP CONSTRAINT [FK_MediaWord];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- NOTE: if the table does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    DROP TABLE [Words];
GO
    DROP TABLE [Clues];
GO
    DROP TABLE [Medias];
GO
    DROP TABLE [Games];
GO
    DROP TABLE [Settings];
GO
    DROP TABLE [Versions];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Words'
CREATE TABLE [Words] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TheWord] nvarchar(4000)  NOT NULL,
    [Definition] nvarchar(4000)  NULL,
    [Area] int  NOT NULL,
    [Complexity] int  NOT NULL,
    [State] int  NOT NULL,
    [LastModified] datetime  NOT NULL
);
GO

-- Creating table 'Clues'
CREATE TABLE [Clues] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TheClue] nvarchar(4000)  NOT NULL,
    [State] int  NOT NULL,
    [WordId] int  NOT NULL,
    [IncludedFromVer] nvarchar(4000)  NULL,
    [ExcludedFromVer] datetime  NULL,
    [LastModified] nvarchar(4000)  NOT NULL
);
GO

-- Creating table 'Medias'
CREATE TABLE [Medias] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Type] int  NOT NULL,
    [Description] nvarchar(4000)  NOT NULL,
    [WordId] int  NULL,
    [Url] nvarchar(4000)  NULL,
    [State] int  NOT NULL,
    [LastModified] datetime  NOT NULL,
    [ExcludedFromVer] datetime  NULL,
    [IncludedFromVer] nvarchar(4000)  NULL
);
GO

-- Creating table 'Games'
CREATE TABLE [Games] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Type] int  NOT NULL,
    [TheGame] nvarchar(4000)  NOT NULL,
    [Complexity] int  NOT NULL,
    [Description] nvarchar(4000)  NULL,
    [IncludedFromVer] nvarchar(4000)  NOT NULL,
    [State] int  NOT NULL,
    [LastModified] nvarchar(4000)  NOT NULL,
    [ExcludedFromVer] datetime  NULL
);
GO

-- Creating table 'Settings'
CREATE TABLE [Settings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(4000)  NOT NULL,
    [Value] nvarchar(4000)  NOT NULL,
    [LastModified] datetime  NOT NULL
);
GO

-- Creating table 'Versions'
CREATE TABLE [Versions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(4000)  NOT NULL,
    [Description] nvarchar(4000)  NOT NULL,
    [InternalNotes] nvarchar(4000)  NULL,
    [State] int  NOT NULL,
    [LastModified] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Words'
ALTER TABLE [Words]
ADD CONSTRAINT [PK_Words]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'Clues'
ALTER TABLE [Clues]
ADD CONSTRAINT [PK_Clues]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'Medias'
ALTER TABLE [Medias]
ADD CONSTRAINT [PK_Medias]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'Games'
ALTER TABLE [Games]
ADD CONSTRAINT [PK_Games]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'Settings'
ALTER TABLE [Settings]
ADD CONSTRAINT [PK_Settings]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'Versions'
ALTER TABLE [Versions]
ADD CONSTRAINT [PK_Versions]
    PRIMARY KEY ([Id] );
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [WordId] in table 'Clues'
ALTER TABLE [Clues]
ADD CONSTRAINT [FK_ClueWord]
    FOREIGN KEY ([WordId])
    REFERENCES [Words]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ClueWord'
CREATE INDEX [IX_FK_ClueWord]
ON [Clues]
    ([WordId]);
GO

-- Creating foreign key on [WordId] in table 'Medias'
ALTER TABLE [Medias]
ADD CONSTRAINT [FK_MediaWord]
    FOREIGN KEY ([WordId])
    REFERENCES [Words]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MediaWord'
CREATE INDEX [IX_FK_MediaWord]
ON [Medias]
    ([WordId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------