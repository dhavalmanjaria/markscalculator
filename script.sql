USE [master]
GO
/****** Object:  Database [markscalc]    Script Date: 12/11/2016 4:58:31 PM ******/
CREATE DATABASE [markscalc]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'markscalc', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA\markscalc.mdf' , SIZE = 3264KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'markscalc_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.SQLEXPRESS\MSSQL\DATA\markscalc_log.ldf' , SIZE = 816KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [markscalc] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [markscalc].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [markscalc] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [markscalc] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [markscalc] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [markscalc] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [markscalc] SET ARITHABORT OFF 
GO
ALTER DATABASE [markscalc] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [markscalc] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [markscalc] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [markscalc] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [markscalc] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [markscalc] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [markscalc] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [markscalc] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [markscalc] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [markscalc] SET  ENABLE_BROKER 
GO
ALTER DATABASE [markscalc] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [markscalc] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [markscalc] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [markscalc] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [markscalc] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [markscalc] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [markscalc] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [markscalc] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [markscalc] SET  MULTI_USER 
GO
ALTER DATABASE [markscalc] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [markscalc] SET DB_CHAINING OFF 
GO
ALTER DATABASE [markscalc] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [markscalc] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [markscalc] SET DELAYED_DURABILITY = DISABLED 
GO
USE [markscalc]
GO
/****** Object:  Table [dbo].[classes]    Script Date: 12/11/2016 4:58:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[classes](
	[classname] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[classname] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[studentFields]    Script Date: 12/11/2016 4:58:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[studentFields](
	[studentid] [int] NOT NULL,
	[subjectid] [int] NOT NULL,
	[fieldid] [int] NULL,
	[fieldname] [nvarchar](100) NULL,
	[studentmarks] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[students]    Script Date: 12/11/2016 4:58:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[students](
	[studentid] [int] IDENTITY(1,1) NOT NULL,
	[studentname] [nvarchar](100) NULL,
	[classname] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[studentid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[studentssubjects]    Script Date: 12/11/2016 4:58:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[studentssubjects](
	[studentid] [int] NOT NULL,
	[subjectid] [int] NOT NULL,
 CONSTRAINT [pk_composite] PRIMARY KEY CLUSTERED 
(
	[studentid] ASC,
	[subjectid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[subjectFields]    Script Date: 12/11/2016 4:58:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[subjectFields](
	[fieldid] [int] IDENTITY(400,1) NOT NULL,
	[fieldname] [nvarchar](100) NULL,
	[subjectid] [int] NOT NULL,
	[maxMarks] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[fieldid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[subjects]    Script Date: 12/11/2016 4:58:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[subjects](
	[subjectid] [int] IDENTITY(200,1) NOT NULL,
	[subjectname] [nvarchar](100) NULL,
	[classname] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[subjectid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  View [dbo].[studentssubjectsView]    Script Date: 12/11/2016 4:58:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view [dbo].[studentssubjectsView]
as
select students.studentid, subjects.subjectid
	from students join classes
		on students.classname = classes.classname
	join subjects
		on subjects.classname = classes.classname

GO
ALTER TABLE [dbo].[studentFields]  WITH CHECK ADD FOREIGN KEY([fieldid])
REFERENCES [dbo].[subjectFields] ([fieldid])
GO
ALTER TABLE [dbo].[studentFields]  WITH CHECK ADD  CONSTRAINT [pk_student_subject_candidate_key] FOREIGN KEY([studentid], [subjectid])
REFERENCES [dbo].[studentssubjects] ([studentid], [subjectid])
GO
ALTER TABLE [dbo].[studentFields] CHECK CONSTRAINT [pk_student_subject_candidate_key]
GO
ALTER TABLE [dbo].[students]  WITH CHECK ADD FOREIGN KEY([classname])
REFERENCES [dbo].[classes] ([classname])
GO
ALTER TABLE [dbo].[studentssubjects]  WITH CHECK ADD FOREIGN KEY([studentid])
REFERENCES [dbo].[students] ([studentid])
GO
ALTER TABLE [dbo].[studentssubjects]  WITH CHECK ADD FOREIGN KEY([subjectid])
REFERENCES [dbo].[subjects] ([subjectid])
GO
ALTER TABLE [dbo].[subjectFields]  WITH CHECK ADD FOREIGN KEY([subjectid])
REFERENCES [dbo].[subjects] ([subjectid])
GO
ALTER TABLE [dbo].[subjects]  WITH CHECK ADD FOREIGN KEY([classname])
REFERENCES [dbo].[classes] ([classname])
GO
/****** Object:  StoredProcedure [dbo].[getStudentFields]    Script Date: 12/11/2016 4:58:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[getStudentFields]
	@classname nvarchar(100)
as
	select students.studentid, students.studentname, subjectname, fieldid, fieldname, studentmarks
		from studentFields join subjects
			on studentFields.subjectid = subjects.subjectid
		join students
			on studentFields.studentid = students.studentid
	where subjects.classname = @classname 

	order by studentname

GO
USE [master]
GO
ALTER DATABASE [markscalc] SET  READ_WRITE 
GO

USE [markscalc]
GO

/****** Object:  Trigger [dbo].[addStudentToStudentFieldsTrigger]    Script Date: 12/11/2016 5:00:42 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

create trigger [dbo].[addStudentToStudentFieldsTrigger]
on [dbo].[students]
after insert
as
	declare @studentid as int
	select @studentid = i.studentid from inserted i;		
	
	insert into studentfields 
	select studentssubjects.studentid, studentssubjects.subjectid, 
				subjectfields.fieldid,subjectfields.fieldname, studentMarks = 0, subjectfields.maxmarks
	from subjectfields join studentssubjects
		on subjectfields.subjectid= studentssubjects.subjectid
	where studentid = @studentid
	print 'student added to studentfields from student table'

GO

USE [markscalc]
GO

/****** Object:  Trigger [dbo].[studentSubjectTrigger]    Script Date: 12/11/2016 5:01:06 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



create trigger [dbo].[studentSubjectTrigger]
ON [dbo].[students]
AFTER INSERT
AS
	declare @studentid as nvarchar(200)
	select @studentid = i.studentid from inserted i;
	
	insert into studentssubjects select * from studentssubjectsView where studentid= @studentid 
GO


USE [markscalc]
GO

/****** Object:  Trigger [dbo].[addNewFieldToStudentFields]    Script Date: 12/11/2016 5:01:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE trigger [dbo].[addNewFieldToStudentFields]
on [dbo].[subjectFields]
AFTER INSERT
AS
	declare @fieldid as int 
	declare @subjectid as int
	
	select @fieldid = i.fieldid from inserted i;
	select @subjectid = i.subjectid from inserted i;
	
	insert into studentfields 
		select studentssubjects.studentid, studentssubjects.subjectid, 
					subjectfields.fieldid,subjectfields.fieldname, studentMarks = 0
		from subjectfields join studentssubjects on subjectfields.subjectid= studentssubjects.subjectid
			where studentssubjects.subjectid=@subjectid and subjectfields.fieldid=@fieldid


GO




