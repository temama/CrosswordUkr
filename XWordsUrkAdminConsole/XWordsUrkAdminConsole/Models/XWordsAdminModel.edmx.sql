
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server Compact Edition
-- --------------------------------------------------
-- Date Created: 09/19/2017 01:26:00
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
    ALTER TABLE [Events] DROP CONSTRAINT [FK_EventUser];
GO
    ALTER TABLE [Clues] DROP CONSTRAINT [FK_ClueUser];
GO
    ALTER TABLE [Words] DROP CONSTRAINT [FK_WordUser];
GO
    ALTER TABLE [Medias] DROP CONSTRAINT [FK_MediaModifiedUser];
GO
    ALTER TABLE [Settings] DROP CONSTRAINT [FK_SettingModifiedUser];
GO
    ALTER TABLE [Games] DROP CONSTRAINT [FK_GameModifiedUser];
GO
    ALTER TABLE [Versions] DROP CONSTRAINT [FK_VersionModifiedUser];
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
    DROP TABLE [Users];
GO
    DROP TABLE [Events];
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
    [LastModified] datetime  NOT NULL,
    [UserId] int  NOT NULL
);
GO

-- Creating table 'Clues'
CREATE TABLE [Clues] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [WordId] int  NOT NULL,
    [GameType] int  NOT NULL,
    [TheClue] nvarchar(4000)  NOT NULL,
    [Complexity] int  NOT NULL,
    [State] int  NOT NULL,
    [IncludedFromVer] nvarchar(4000)  NULL,
    [ExcludedFromVer] nvarchar(4000)  NULL,
    [LastModified] datetime  NOT NULL,
    [UserId] int  NOT NULL
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
    [IncludedFromVer] nvarchar(4000)  NULL,
    [UserId] int  NOT NULL
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
    [ExcludedFromVer] datetime  NULL,
    [State] int  NOT NULL,
    [LastModified] nvarchar(4000)  NOT NULL,
    [UserId] int  NOT NULL
);
GO

-- Creating table 'Settings'
CREATE TABLE [Settings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(4000)  NOT NULL,
    [Value] nvarchar(4000)  NOT NULL,
    [LastModified] datetime  NOT NULL,
    [UserId] int  NOT NULL
);
GO

-- Creating table 'Versions'
CREATE TABLE [Versions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(4000)  NOT NULL,
    [Description] nvarchar(4000)  NOT NULL,
    [InternalNotes] nvarchar(4000)  NULL,
    [State] int  NOT NULL,
    [LastModified] datetime  NOT NULL,
    [UserId] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(4000)  NOT NULL,
    [Initials] nvarchar(4000)  NOT NULL,
    [Password] nvarchar(4000)  NOT NULL,
    [Role] int  NOT NULL
);
GO

-- Creating table 'Events'
CREATE TABLE [Events] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TimeStamp] datetime  NOT NULL,
    [UserId] int  NOT NULL,
    [Table] nvarchar(4000)  NOT NULL,
    [RecordId] int  NOT NULL,
    [Comment] nvarchar(4000)  NULL
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

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY ([Id] );
GO

-- Creating primary key on [Id] in table 'Events'
ALTER TABLE [Events]
ADD CONSTRAINT [PK_Events]
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

-- Creating foreign key on [UserId] in table 'Events'
ALTER TABLE [Events]
ADD CONSTRAINT [FK_EventUser]
    FOREIGN KEY ([UserId])
    REFERENCES [Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_EventUser'
CREATE INDEX [IX_FK_EventUser]
ON [Events]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'Clues'
ALTER TABLE [Clues]
ADD CONSTRAINT [FK_ClueUser]
    FOREIGN KEY ([UserId])
    REFERENCES [Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ClueUser'
CREATE INDEX [IX_FK_ClueUser]
ON [Clues]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'Words'
ALTER TABLE [Words]
ADD CONSTRAINT [FK_WordUser]
    FOREIGN KEY ([UserId])
    REFERENCES [Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_WordUser'
CREATE INDEX [IX_FK_WordUser]
ON [Words]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'Medias'
ALTER TABLE [Medias]
ADD CONSTRAINT [FK_MediaModifiedUser]
    FOREIGN KEY ([UserId])
    REFERENCES [Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MediaModifiedUser'
CREATE INDEX [IX_FK_MediaModifiedUser]
ON [Medias]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'Settings'
ALTER TABLE [Settings]
ADD CONSTRAINT [FK_SettingModifiedUser]
    FOREIGN KEY ([UserId])
    REFERENCES [Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_SettingModifiedUser'
CREATE INDEX [IX_FK_SettingModifiedUser]
ON [Settings]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'Games'
ALTER TABLE [Games]
ADD CONSTRAINT [FK_GameModifiedUser]
    FOREIGN KEY ([UserId])
    REFERENCES [Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GameModifiedUser'
CREATE INDEX [IX_FK_GameModifiedUser]
ON [Games]
    ([UserId]);
GO

-- Creating foreign key on [UserId] in table 'Versions'
ALTER TABLE [Versions]
ADD CONSTRAINT [FK_VersionModifiedUser]
    FOREIGN KEY ([UserId])
    REFERENCES [Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_VersionModifiedUser'
CREATE INDEX [IX_FK_VersionModifiedUser]
ON [Versions]
    ([UserId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------