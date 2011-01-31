USE [User_Data_Store]
GO

/****** Object:  Table [dbo].[saml_bridge_artifact]    Script Date: 11/01/2010 16:55:04 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[saml_bridge_artifact]') AND type in (N'U'))
DROP TABLE [dbo].[saml_bridge_artifact]
GO

USE [User_Data_Store]
GO

/****** Object:  Table [dbo].[saml_bridge_artifact]    Script Date: 11/01/2010 16:55:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[saml_bridge_artifact](
	[artifact_id] [varchar](100) NOT NULL,
	[request_id] [varchar](100) NULL,
	[subject] [varchar](50) NULL,
	[groups] [varchar](max) NULL,
 CONSTRAINT [PK_saml-bridge] PRIMARY KEY CLUSTERED 
(
	[artifact_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

