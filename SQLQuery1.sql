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

--create table fields
--(
--studentid int not null,
--subjectid int not null,
--fieldname nvarchar(100),
--studentmarks int,
--maxMarks int
--)

--alter table fields
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


--create trigger studentSubjectTrigger
--ON students
--AFTER INSERT
--AS
--	declare @studentid as nvarchar(200)
--	select @studentid = i.studentid from inserted i;
	
--	insert into studentssubjects select * from studentssubjectsView where studentid= @studentid 


insert into students values('ffff', 'bca2')



select * from studentssubjects where studentid = 17