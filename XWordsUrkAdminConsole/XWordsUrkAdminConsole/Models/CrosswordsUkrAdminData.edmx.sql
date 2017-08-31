
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server Compact Edition
-- --------------------------------------------------
-- Date Created: 08/31/2017 13:19:41
-- Generated from EDMX file: D:\Work\CrosswordUkr\XWordsUrkAdminConsole\XWordsUrkAdminConsole\Models\CrosswordsUkrAdminData.edmx
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- NOTE: if the constraint does not exist, an ignorable error will be reported.
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- NOTE: if the table does not exist, an ignorable error will be reported.
-- --------------------------------------------------

    DROP TABLE [WordsSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'WordsSet'
CREATE TABLE [WordsSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Word] nvarchar(4000)  NOT NULL,
    [Definition] nvarchar(4000)  NOT NULL,
    [Clue] nvarchar(4000)  NOT NULL,
    [Status] int  NOT NULL,
    [LastModified] datetime  NOT NULL,
    [Complexity] int  NOT NULL,
    [IncludedFromVerion] nvarchar(4000)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'WordsSet'
ALTER TABLE [WordsSet]
ADD CONSTRAINT [PK_WordsSet]
    PRIMARY KEY ([Id] );
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------