----create database markscalc
--use markscalc

--create table classes
--(
--classname nvarchar(100) primary key
--)

--create table subjects
--(
--subjectid int identity(200, 1) primary key,
--subjectname nvarchar(100),
--classname nvarchar(100) references classes(classname)
--)

--create table students
--(
--studentid int identity(1, 1) primary key,
--studentname nvarchar(100),
--classname nvarchar(100) references classes(classname)
--)

--create table studentssubjects
--(
--studentid int not null references students(studentid),
--subjectid int not null references subjects(subjectid)
--)

--alter table studentssubjects
--add constraint pk_composite primary key(studentid, subjectid)

--create table subjectFields
--(
--fieldid int identity(400, 1) primary key,
--fieldname nvarchar(100),
--subjectid int not null references subjects(subjectid),
--maxMarks int
--)


--create table studentFields
--(
--studentid int not null,
--subjectid int not null,
--fieldid int references subjectFields(fieldid),
--fieldname nvarchar(100),
--studentmarks int,
--maxMarks int
--)

--alter table studentfields
--add constraint pk_student_subject_candidate_key foreign key(studentid, subjectid) references studentssubjects(studentid, subjectid)



--insert into classes values('bca1')
--insert into classes values('bca2')
--insert into classes values('bca3')

--insert into subjects(subjectname, classname) values('c programming', 'bca1')

--insert into subjects(subjectname, classname) values('vb.net', 'bca2')
--insert into subjects(subjectname, classname) values('c++', 'bca2')

--insert into subjects(subjectname, classname) values('vb.net_3', 'bca3')
--insert into subjects(subjectname, classname) values('linux', 'bca3')



--insert into students(studentname,  classname) values('aaa', 'bca1')
--insert into students(studentname,  classname) values('bbb', 'bca1')
--insert into students(studentname,  classname) values('ccc', 'bca1')
--insert into students(studentname,  classname) values('ddd', 'bca1')
--insert into students(studentname,  classname) values('eee', 'bca1')
--insert into students(studentname,  classname) values('aaaa', 'bca2')
--insert into students(studentname,  classname) values('bbbb', 'bca2')
--insert into students(studentname,  classname) values('cccc', 'bca2')
--insert into students(studentname,  classname) values('dddd', 'bca2')
--insert into students(studentname,  classname) values('eeed', 'bca2')
--insert into students(studentname,  classname) values('aaaaa', 'bca3')
--insert into students(studentname,  classname) values('bbbbb', 'bca3')
--insert into students(studentname,  classname) values('ccccc', 'bca3')
--insert into students(studentname,  classname) values('ddddd', 'bca3')
--insert into students(studentname,  classname) values('eeeee', 'bca3')

--/* This is an important join. This is basically the information we want in StudentsSubjects */

--create view studentssubjectsView
--as
--select students.studentid, subjects.subjectid
--	from students join classes
--		on students.classname = classes.classname
--	join subjects
--		on subjects.classname = classes.classname

-- The table studentssubjects takes care of the foreign key constraints to be imposed on the StudentFields table.
-- It makes sure that a student doesn't get a field for a subject that they don't have.
-- This trigger inserts a value in the StudentsSubjects junction table, to make sure that when a student is added,
-- they're added to the junction table
--create trigger studentSubjectTrigger
--ON students
--AFTER INSERT
--AS
--	declare @studentid as nvarchar(200)
--	select @studentid = i.studentid from inserted i;	
--	insert into studentssubjects select * from studentssubjectsView where studentid= @studentid 

-- Trigger to add a field to every student.
-- Makes sure that when a field is added to a subject, each student that takes that subject has a field in the
-- StudentFields table
--create trigger studentFieldTrigger
--on subjectfields
--AFTER INSERT
--AS
--	declare @fieldid as int 
--	declare @subjectid as int
	
--	select @fieldid = i.fieldid from inserted i;
--	select @subjectid = i.subjectid from inserted i;
	
--	insert into studentfields 
--		select studentssubjects.studentid, studentssubjects.subjectid, 
--					subjectfields.fieldid,subjectfields.fieldname, studentMarks = 0, subjectfields.maxmarks
--		from subjectfields join studentssubjects
--			on subjectfields.subjectid= studentssubjects.subjectid
--		where studentssubjects.subjectid=@subjectid and subjectfields.fieldid=@fieldid

----------
-- This trigger will make sure to add relevant student details in the studentfields table when a student is added
--create trigger addStudentToStudentFieldsTrigger
--on students
--after insert
--as
--	declare @studentid as int
	
--		insert into studentfields 
--		select studentssubjects.studentid, studentssubjects.subjectid, 
--					subjectfields.fieldid,subjectfields.fieldname, studentMarks = 0, subjectfields.maxmarks
--		from subjectfields join studentssubjects
--			on subjectfields.subjectid= studentssubjects.subjectid
--		where studentid = @studentid



--select * from classes
--select * from subjects
--select * from students
--select * from studentssubjects

--insert into subjectFields(subjectid, fieldname, maxMarks) values(200, 'assignment',40)
--insert into subjectFields(subjectid, fieldname, maxMarks) values(200, 'assignment2',40)
--insert into subjectFields(subjectid, fieldname, maxMarks) values(201, 'assignment1',40)
--insert into subjectFields(subjectid, fieldname, maxMarks) values(201, 'test1',40)
--insert into subjectFields(subjectid, fieldname, maxMarks) values(202, 'test1',40)
--insert into subjectFields(subjectid, fieldname, maxMarks) values(203, 'presentation1',40)
--insert into subjectFields(subjectid, fieldname, maxMarks) values(203, 'presentation2',40)
--insert into subjectFields(subjectid, fieldname, maxMarks) values(204, 'test1',40)
--insert into subjectFields(subjectid, fieldname, maxMarks) values(204, 'assignment',40)

select distinct(fieldid) from studentfields where subjectid = 201

--select studentid, [401], [402], [403], [404], [405], [406], [407], [408], [409] 
--from
--	(select studentid, fieldid, studentMarks from studentfields where subjectid=201) as tab
--	pivot
--	(
--		max(studentmarks) for fieldid in([401], [402], [403], [404], [405], [406], [407], [408], [409])
--	) as pvt

create procedure getStudentFields
	@classname nvarchar(100)
as
	select students.studentid, students.studentname, subjectname, fieldid, fieldname, studentmarks, maxmarks 
		from studentFields join subjects
			on studentFields.subjectid = subjects.subjectid
		join students
			on studentFields.studentid = students.studentid
	where subjects.classname = @classname 

	order by studentname



--select students.studentid, students.studentname, subjectname, fieldid, fieldname, studentmarks, maxmarks 
--		from studentFields join subjects
--			on studentFields.subjectid = subjects.subjectid
--		join students
--			on studentFields.studentid = students.studentid
--		where subjects.classname = 'bca1'

--select * from subjectfield