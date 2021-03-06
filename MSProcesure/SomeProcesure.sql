USE [SqlServerTestDB]
GO
/****** Object:  StoredProcedure [dbo].[P_SplitPageOneSql_v2]    Script Date: 12/10/2020 16:07:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--如果分页数超过了最后一页,仍将会显示最后一页的记录      
CREATE procedure [dbo].[P_SplitPageOneSql_v2]      
@tablename varchar(32),      
@pagesize int,   --页面大小      
@pagepos int,   --第几页      
@orderby  varchar(64),    --排序顺序,多个则用","分隔 加上前缀b.      
@orderby2 varchar(64),   --排序顺序的反向排序,多个则用","分隔 加上前缀b.      
@orderbyclause varchar(64), --排序字段(不包括desc ,asc等),以","分隔  加上前缀b.      
@keyname varchar(32),   --主键      
@keyinorder tinyint ,       --主键是否在排序字段中 1:在  0:不在      
@whereclause varchar(1024) , --where条件      
@showclause varchar(1024),  --显示的字段 加上前缀a.  --注意如果显示字段过长的话,需要再加长      
@totalcount int output  --是否需要统计总数 -1:要统计 其它不统计      
      
as      
      
/*      
连接其它表分页的示例      
select a.userid,a.writetime,other.otherinfo from (    --a.userid,a.writetime 为显示的字段.与排序的表无关了      
      
select top 3 b.score,b.playcount,b.userid from(      
select top 9 b.score,b.playcount,b.userid from      
  userscore b  order by b.score desc,b.playcount desc,b.userid desc) b       
order by  b.score,b.playcount,b.userid   --这四行获得分页后的行,但还需要倒下序      
      
) b      
      
inner join userscore a on a.userid = b.userid    --连接表 userscore a 是为了正确显示字段      
inner join userotherinfo other on a.userid = other.userid  --这里继续往后追加连接表即可实现多个表分页,但其它的表不参与排序      
order by b.score desc,b.playcount desc,b.userid desc   --这里将结果正确显示      
      
*/      
      
set nocount on      
set transaction isolation level read uncommitted      
declare @sql nvarchar(1024) --注意如果显示字段过长的话,需要再加长      
declare @showrows smallint  --显示的行数(可能在最后一页,这样显示的记录数就要小于@pagesize      
declare @top9clause nvarchar(128)      
if @keyinorder = 0      
    set @top9clause = @orderbyclause + ',b.' + @keyname      
else      
    set @top9clause = @orderbyclause       
if @whereclause is null      
    set @whereclause = ''      
      
if @totalcount <= 0 or @totalcount is null      
begin      
    if(@whereclause = '')    
    begin    
        SELECT @totalcount = rowcnt FROM sys.sysindexes WHERE id = object_id(@tablename) and indid in (0,1);    
    end    
    else    
    begin    
        set @sql = 'set @totalcount = (select count(1) from ' + @tablename + ' ' + @whereclause +')'      
        exec sp_executesql @sql,N'@totalcount int output',@totalcount output      
    end    
end      
      
--要处理好翻到最后一页且不能满页时的情况      
if @totalcount > @pagesize * (@pagepos -1) and @totalcount < @pagesize * @pagepos       
   set @showrows = @totalcount - @pagesize * (@pagepos -1)      
else if  @totalcount >= @pagesize * @pagepos       
   set @showrows = @pagesize      
else      
   return       
      
set @sql = 'select '   +   @showclause +' from (      
select top  ' + convert(varchar, @showrows)  + '  ' + @top9clause +  ' from(      
select top ' + convert(varchar,@pagesize * @pagepos) + ' '       
 + @top9clause +  '  from      
  ' + @tablename  + '  b ' +  @whereclause  +' order by ' + @orderby + ') b       
order by  '  + @orderby2 + ') b inner join ' + @tablename  + '  a on a.' + @keyname +' = b.' + @keyname +'       
order by ' + @orderby    

print @sql   
exec sp_executesql @sql
GO
/****** Object:  StoredProcedure [dbo].[cv]    Script Date: 12/10/2020 16:07:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- 生成数据视图类        
create procedure [dbo].[cv]        
@tablename varchar(50) = ''           
as            
set nocount on        
set transaction isolation level read uncommitted        
set xact_abort on        
        
if object_id(@tablename) is null             
begin            
    select 'table not exists' as error        
    return            
end        
        
declare @tmptb table(code varchar(2048))        
declare @totalcols int        
declare @classname varchar(50)        
declare @word varchar(50)        
declare @pos int        
declare @func varchar(2048)        
        
select @totalcols = count(1) from syscolumns where id = object_id(@tablename)         
        
        
set @classname = @tablename         
if( @classname <> '' and left(@classname,2) = 't_')        
begin        
    set @classname = substring(@classname,3,len(@classname) - 2)        
end        
        
set @classname = upper(left(@classname,1)) + substring(@classname,2,len(@classname) - 1) + 'Info'        
while( charindex('_',@classname) > 0 )        
begin        
    set @pos = charindex('_',@classname)        
    set @word = substring(@classname,@pos + 1, 1)        
    set @classname = replace(@classname,'_' + @word,upper(@word) )        
end        
        
select @classname + '.cs' as 数据视图类文件名        
        
set @func = 'public ' + @classname + '('        
select @func = @func +                                  
case type_name(xusertype)                                  
when 'varchar' then  'string'                                  
when 'nvarchar' then 'string'                                  
when 'decimal' then 'decimal'                                  
when 'tinyint' then 'int'                                  
when 'smallint' then 'int'                                  
when 'datetime' then 'DateTime'                                  
when 'smalldatetime' then 'DateTime'                                  
when 'char' then 'string'                                  
when 'nchar' then 'string'                                  
when 'bigint'  then 'long'                                  
when 'text' then 'string'                                  
when 'ntext' then 'string'                                  
else type_name(xusertype)                                  
end                                   
 +   ' '+  name +        
case when colid < @totalcols then ', ' else '' end        
from syscolumns  where id = object_id(@tablename) order by colid        
        
select @func = @func + ')'        
        
--insert into @tmptb select @func        
--insert into @tmptb select '{'         
--insert into @tmptb select 'this._' + name + ' = ' + name + ';' from syscolumns  where id = object_id(@tablename) order by colid        
--insert into @tmptb select '}'        
--insert into @tmptb select ''        
insert into @tmptb select '#region 公共属性'        
        
        
declare @parmType varchar(50)        
declare @parmName varchar(50)        
declare @colid int        
declare @cur cursor         
set @cur = cursor read_only forward_only for         
select b.name,a.name,a.colid from syscolumns a inner join systypes b on a.xtype = b.xtype where a.id = object_id(@tablename)        
        
open @cur         
fetch next from @cur into @parmType,@parmName,@colid        
while(@@fetch_status = 0)        
begin        
        
    --insert into @tmptb         
    --select        
    --'private ' + case @parmType        
    --when 'decimal' then 'decimal'        
    --when 'int' then 'int'            
    --when 'smallint' then 'int'            
    --when 'tinyint' then 'int'            
    --when 'bigint' then 'long'            
    --when 'bit' then 'int'            
    --when 'char' then 'string'            
    --when 'nchar' then 'string'            
    --when 'varchar' then 'string'            
    --when 'nvarchar' then 'string'            
    --when 'smalldatetime' then 'DateTime'            
    --when 'datetime' then 'DateTime'            
    --else @parmType end + ' _' + @parmName + ';'           
        
    if exists(select 1 from sys.extended_properties where major_id = object_id(@tablename) and name = 'MS_Description')        
        insert into @tmptb         
        select '/// <summary>' union all        
        select '/// ' + convert(varchar,value) from sys.extended_properties where major_id = object_id(@tablename) and  minor_id = @colid  union all        
        select '/// </summary>'         
    else        
        insert into @tmptb         
        select '/// <summary>' union all        
        select '/// ' union all         
        select '/// </summary>'         
        
    insert into @tmptb         
    select             
    'public ' + case @parmType        
    when 'decimal' then 'decimal'             
    when 'int' then 'int'            
    when 'smallint' then 'int'            
    when 'tinyint' then 'int'            
    when 'bigint' then 'long'            
    when 'bit' then 'int'            
    when 'char' then 'string'            
    when 'nchar' then 'string'            
    when 'varchar' then 'string'            
    when 'nvarchar' then 'string'            
    when 'smalldatetime' then 'DateTime'            
    when 'datetime' then 'DateTime'            
    else @parmType end             
    + ' ' + @parmName             
    + '    {'            
    + '    get; set;'            
    + '    }'               
        
    insert into @tmptb select ''        
        
    fetch next from @cur into @parmType,@parmName,@colid        
        
end        
insert into @tmptb select '#endregion'        
        
select code as 类字段和属性 from @tmptb
GO
/****** Object:  StoredProcedure [dbo].[cp]    Script Date: 12/10/2020 16:07:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[cp]
@tablename varchar(50) = '' -- 表名称
as
set nocount on
set transaction isolation level read uncommitted
set xact_abort on

declare @tableid int
set @tableid = object_id(@tablename)

if @tableid is null
begin
    select 'table not exists' as error
    return
end

declare @tmptb table(code varchar(4000))

declare @fieldsName varchar(4000)
declare @parmName varchar(4000)
declare @keyname varchar(50)
declare @proctablename varchar(50)

declare @ident_seed int
select @ident_seed = ident_seed(@tablename)
if(@ident_seed is null)
    set @ident_seed = -1

set @proctablename = @tablename
if( left(@proctablename,2) ='t_')
    set @proctablename = substring(@proctablename,3,len(@proctablename) - 2)

print @proctablename

select top 1 @keyname= [name] from syscolumns where id = @tableid order by colid

set @fieldsname = ''
select @fieldsname = @fieldsname + a.[name] + ','
from syscolumns a inner join systypes b on a.xtype = b.xtype
where id = @tableid and a.colid <> @ident_seed
and b.name <> 'datetime' and b.name <> 'smalldatetime'
order by colid

if( right(@fieldsname,1) = ',' )
    set @fieldsname = substring(@fieldsname,1,len(@fieldsname) - 1)

set @parmname = ''
select @parmname = @parmname + '@' + a.[name] + ','
from syscolumns a inner join systypes b on a.xtype = b.xtype
where id = @tableid and a.colid <> @ident_seed
and b.name <> 'datetime' and b.name <> 'smalldatetime'
order by colid

if( right(@parmname,1) = ',' )
    set @parmname = substring(@parmname,1,len(@parmname) - 1)

-- proc_pagelist1
delete from @tmptb
insert into @tmptb select 'if object_id(N''p_' + @proctablename + '_list'') is not null'
insert into @tmptb select ' drop procedure p_' + @proctablename + '_list'
insert into @tmptb select 'go'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select '-- Procedure Name: cc p_' + @proctablename + '_list ,1,0'
insert into @tmptb select '-- Author: zx'
insert into @tmptb select '-- Date Generated: ' + datename(year,getdate()) + '年' + datename(month,getdate()) + '月'  + datename(day,getdate()) + '日'
insert into @tmptb select '-- Description: '
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select ''
insert into @tmptb select 'create procedure p_' + @proctablename + '_list'

insert into @tmptb
select '@page int = 1, --当前页' union all
select '@pagesize int = 10, --每页显示' union all
select '@total int = 0, --总记录数' union all
select '@typeid int = 0 ''这里需要修改''' union all
select 'as' union all
select 'set nocount on' union all
select 'set transaction isolation level read uncommitted' union all
select 'set xact_abort on' union all
select ''

insert into @tmptb
select 'if( @total = 0 )' union all
select '    select @total = count(1) from ' + @tablename + ' where typeid = @typeid ''这里需要修改''' union all
select '' union all
select 'if( @page = 1 )' union all
select 'begin' union all
select '    set rowcount @pagesize' union all
select '    select * from ' + @tablename + ' where typeid = @typeid ''这里需要修改'' order by ' + @keyname +  ' desc ' union all
select '    set rowcount 0' union all
select '    return' union all
select 'end' union all
select 'else' union all
select 'begin' union all
select '    declare @tb table(rownum int identity(1,1),keyid int)' union all
select '    insert into @tb(keyid) select ' + @keyname + ' from ' + @tablename union all
select '        where typeid = @typeid ''这里需要修改'' order by ' + @keyname +  ' desc' union all
select '    select a.* from ' + @tablename + ' a inner join @tb b on a.'+ @keyname +  ' = b.keyid ' union all
select '        where b.rownum > @pagesize * (@page - 1) and b.rownum <= @pagesize * @page' union all
select 'end'

insert into @tmptb select '' union all select 'go'

select code as proc_pagelist1 from @tmptb

-- proc_pagelist2
delete from @tmptb
insert into @tmptb select 'if object_id(N''p_' + @proctablename + '_list'') is not null'
insert into @tmptb select ' drop procedure p_' + @proctablename + '_list'
insert into @tmptb select 'go'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select '-- Procedure Name: cc p_' + @proctablename + '_list ,1,0'
insert into @tmptb select '-- Author: lwt'
insert into @tmptb select '-- Date Generated: ' + datename(year,getdate()) + '年' + datename(month,getdate()) + '月'  + datename(day,getdate()) + '日'
insert into @tmptb select '-- Description: '
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select ''
insert into @tmptb select 'create procedure p_' + @proctablename + '_list'

insert into @tmptb
select '@page int = 1, --当前页' union all
select '@pagesize int = 10, --每页显示' union all
select '@total int = 0 output, --总记录数' union all
select '@typeid int = 0 ''这里需要修改''' union all
select 'as' union all
select 'set nocount on' union all
select 'set transaction isolation level read uncommitted' union all
select 'set xact_abort on' union all
select ''

insert into @tmptb
select 'declare @tablename varchar(1024)' union all
select 'declare @orderby  varchar(1024)' union all
select 'declare @orderby2 varchar(1024)' union all
select 'declare @orderbyclause varchar(1024)' union all
select 'declare @keyname varchar(50)' union all
select 'declare @keyinorder tinyint' union all
select 'declare @whereclause varchar(1024)' union all
select 'declare @showclause varchar(1024)' union all
select '' union all
select '-- 表名称' union all
select 'set @tablename = ''' + @tablename + ''' ' union all
select '--排序字段,字段前面加b.,字段后加 desc或asc,格式如 b.rid desc,b.rname asc' union all
select 'set @orderby = ''b.' + @keyname + ' desc'' ' union all
select '--排序字段,字段前面加b.,字段后面和上面的orderby正好相反,格式如 b.rid asc,b.ranme desc' union all
select 'set @orderby2 = ''b.' + @keyname + ' asc'' '  union all
select '--排序字段，这里不需要加 asc 和 desc，格式如 b.rid,b.rname' union all
select 'set @orderbyclause = ''b.' + @keyname + '''' union all
select '--主键是否在排序字段里，1 是 0 不是，一般是1' union all
select 'set @keyinorder = 1'  union all
select '--这个表的主键' union all
select 'set @keyname = ''' + @keyname +  ''''  union all
select '--查询条件' union all
select 'set @whereclause = '' where typeid = '' + convert(varchar,@typeid) ' + '''这里需要修改'''  union all
select 'set @showclause = ''a.*'''   union all
select '' union all
select '--调用分页存储过程' union all
select 'exec P_SplitPageOneSql_v2 @tablename,@pagesize,@page,@orderby,@orderby2,' union all
select '@orderbyclause,@keyname,@keyinorder,@whereclause,@showclause,@total output'

insert into @tmptb select '' union all select 'go'

select code as proc_pagelist2 from @tmptb

-- proc_insert
delete from @tmptb
insert into @tmptb select 'if object_id(N''p_' + @proctablename + '_insert'') is not null'
insert into @tmptb select ' drop procedure p_' + @proctablename + '_insert'
insert into @tmptb select 'go'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select '-- Procedure Name: cc p_' + @proctablename + '_insert,0,1'
insert into @tmptb select '-- Author: lwt'
insert into @tmptb select '-- Date Generated: ' + datename(year,getdate()) + '年' + datename(month,getdate()) + '月'  + datename(day,getdate()) + '日'
insert into @tmptb select '-- Description: '
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select ''
insert into @tmptb select 'create procedure p_' + @proctablename + '_insert'

insert into @tmptb
select
'@' + a.name + ' ' +
case b.name
when 'char' then 'char(' + convert(varchar,a.length) + ') = '''','
when 'nchar' then 'nchar(' + convert(varchar,a.length) + ')= '''','
when 'varchar' then 'varchar(' + convert(varchar,a.length) + ') = '''','
when 'nvarchar' then 'nvarchar(' + convert(varchar,a.length) + ') = '''','
else b.name + ' = 0,' end
from syscolumns a inner join systypes b
on a.xtype = b.xtype
where a.id = object_id(@tablename)
and a.colid <> @ident_seed
and b.name <> 'datetime' and b.name <> 'smalldatetime'
order by colid

insert into @tmptb
select '@outmsg varchar(256) = '''' output,' union all
select '@newid int = 0 output' union all
select 'as' union all
select 'set nocount on' union all
select 'set transaction isolation level read uncommitted' union all
select 'set xact_abort on' union all
select ''

insert into @tmptb
select 'insert into ' + @tablename + '(' + @fieldsname + ')' union all
select '    values(' + @parmname + ')'

insert into @tmptb
select ' ' union all
select 'if @@rowcount = 0 ' union all
select 'begin' union all
select '    set @outmsg = ''添加失败''' union all
select '    return 0' union all
select 'end' union all
select '' union all
select 'set @newid = @@identity' union all
select '' union all
select 'if @newid <= 0' union all
select 'begin' union all
select '    set @outmsg = ''添加失败''' union all
select '    return 0' union all
select 'end' union all
select '' union all
select 'set @outmsg = ''添加成功''' union all
select 'return 1' union all
select '' union all
select 'go'

select code as proc_insert from @tmptb


-- proc_save 更新 与 插入
delete from @tmptb
insert into @tmptb select 'if object_id(N''p_' + @proctablename + '_save'') is not null'
insert into @tmptb select ' drop procedure p_' + @proctablename + '_save'
insert into @tmptb select 'go'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select '-- Procedure Name: cc p_' + @proctablename + '_save,0,1'
insert into @tmptb select '-- Author: zx'
insert into @tmptb select '-- Date Generated: ' + datename(year,getdate()) + '年' + datename(month,getdate()) + '月'  + datename(day,getdate()) + '日'
insert into @tmptb select '-- Description: 数据保存--更新与插入'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select ''
insert into @tmptb select 'create procedure p_' + @proctablename + '_save'

insert into @tmptb
select
'@' + a.name + ' ' +
case b.name
when 'char' then 'char(' + convert(varchar,a.length) + ') = '''','
when 'nchar' then 'nchar(' + convert(varchar,a.length) + ')= '''','
when 'varchar' then 'varchar(' + convert(varchar,a.length) + ') = '''','
when 'nvarchar' then 'nvarchar(' + convert(varchar,a.length) + ') = '''','
else b.name + ' = 0,' end
from syscolumns a inner join systypes b
on a.xtype = b.xtype
where a.id = object_id(@tablename) and
b.name <> 'datetime' and b.name <> 'smalldatetime'
order by colid

insert into @tmptb
select '@outmsg varchar(256) = '''' output,' union all
select '@newid int = 0 output' union all
select 'as' union all
select 'set nocount on' union all
select 'set transaction isolation level read uncommitted' union all
select 'set xact_abort on' union all
select ''


insert into @tmptb
select 'if @' + @keyname + ' <= 0 ' union all
select 'begin' union all
select '   insert into ' + @tablename + '(' + @fieldsname + ')' union all
select '    values(' + @parmname + ')'

insert into @tmptb
select ' ' union all
select '   if @@rowcount <= 0 ' union all
select '   begin' union all
select '      set @outmsg = ''添加失败''' union all
select '      return 0' union all
select '   end' union all
select '' union all
select '  set @newid = @@identity' union all
select '' union all
select '  if @newid <= 0' union all
select '  begin' union all
select '     set @outmsg = ''添加失败''' union all
select '     return 0' union all
select '  end' union all
select '' union all
select '  set @outmsg = ''添加成功''' union all
select '  return 1' union all
select '' union all
select 'end' union all
select 'else' union all
select 'begin'

set @fieldsname = ''
select @fieldsname = @fieldsname + a.[name] + ' = ' + '@' + a.[name] + ','
from syscolumns a inner join systypes b on a.xtype = b.xtype
where id = @tableid and a.colid <> @ident_seed
and b.name <> 'datetime' and b.name <> 'smalldatetime'
order by colid

if( right(@fieldsname,1) = ',' )
    set @fieldsname = substring(@fieldsname,1,len(@fieldsname) - 1)

insert into @tmptb
select '   update ' + @tablename union all
select '    set ' + @fieldsname  union all
select '    where ' + @keyname + ' = @' + @keyname

insert into @tmptb
select '' union all
select '   if @@rowcount <= 0 ' union all
select '   begin' union all
select '      set @outmsg = ''更新失败''' union all
select '      return 0' union all
select '   end' union all
select ''

insert into @tmptb
select '   set @newid = @' + @keyname union all
select '   set @outmsg = ''更新成功''' union all
select '   return 1' union all
select '' union all
select 'end' union all
select '' union all
select 'go'

select code as proc_save from @tmptb

-- proc_update
delete from @tmptb
insert into @tmptb select 'if object_id(N''p_' + @proctablename + '_update'') is not null'
insert into @tmptb select ' drop procedure p_' + @proctablename + '_update'
insert into @tmptb select 'go'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select '-- Procedure Name: cc p_' + @proctablename + '_update,0,1'
insert into @tmptb select '-- Author: zx'
insert into @tmptb select '-- Date Generated: ' + datename(year,getdate()) + '年' + datename(month,getdate()) + '月'  + datename(day,getdate()) + '日'
insert into @tmptb select '-- Description: '
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select ''
insert into @tmptb select 'create procedure p_' + @proctablename + '_update'

insert into @tmptb
select
'@' + a.name + ' ' +
case b.name
when 'char' then 'char(' + convert(varchar,a.length) + ') = '''','
when 'nchar' then 'nchar(' + convert(varchar,a.length) + ')= '''','
when 'varchar' then 'varchar(' + convert(varchar,a.length) + ') = '''','
when 'nvarchar' then 'nvarchar(' + convert(varchar,a.length) + ') = '''','
else b.name + ' = 0,' end
from syscolumns a inner join systypes b
on a.xtype = b.xtype
where a.id = object_id(@tablename)
and b.name <> 'datetime' and b.name <> 'smalldatetime'
order by colid

insert into @tmptb
select '@outmsg varchar(256) = '''' output' union all
select 'as' union all
select 'set nocount on' union all
select 'set transaction isolation level read uncommitted' union all
select 'set xact_abort on' union all
select ''

set @fieldsname = ''
select @fieldsname = @fieldsname + a.[name] + ' = ' + '@' + a.[name] + ','
from syscolumns a inner join systypes b on a.xtype = b.xtype
where id = @tableid and a.colid <> @ident_seed
and b.name <> 'datetime' and b.name <> 'smalldatetime'
order by colid

if( right(@fieldsname,1) = ',' )
    set @fieldsname = substring(@fieldsname,1,len(@fieldsname) - 1)

insert into @tmptb
select 'update ' + @tablename union all
select ' set ' + @fieldsname  union all
select ' where ' + @keyname + ' = @' + @keyname

insert into @tmptb
select '' union all
select 'if @@rowcount <= 0 ' union all
select 'begin' union all
select '    set @outmsg = ''更新失败''' union all
select '    return 0' union all
select 'end' union all
select '' union all
select 'set @outmsg = ''更新成功''' union all
select 'return 1' union all
select '' union all
select 'go'

select code as proc_update from @tmptb


-- proc_delete
delete from @tmptb
insert into @tmptb select 'if object_id(N''p_' + @proctablename + '_delete'') is not null'
insert into @tmptb select ' drop procedure p_' + @proctablename + '_delete'
insert into @tmptb select 'go'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select '-- Procedure Name: cc p_' + @proctablename + '_delete,0,1'
insert into @tmptb select '-- Author: zx'
insert into @tmptb select '-- Date Generated: ' + datename(year,getdate()) + '年' + datename(month,getdate()) + '月'  + datename(day,getdate()) + '日'
insert into @tmptb select '-- Description: '
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select ''
insert into @tmptb select 'create procedure p_' + @proctablename + '_delete'

insert into @tmptb
select top 1
'@' + a.name + ' ' +
case b.name
when 'char' then 'char(' + convert(varchar,a.length) + ') = '''''
when 'nchar' then 'nchar(' + convert(varchar,a.length) + ')= '''''
when 'varchar' then 'varchar(' + convert(varchar,a.length) + ') = '''''
when 'nvarchar' then 'nvarchar(' + convert(varchar,a.length) + ') = '''''
else b.name + ' = 0 ,' end
from syscolumns a inner join systypes b
on a.xtype = b.xtype
where a.id = object_id(@tablename)
order by a.colid

insert into @tmptb
select '@outmsg varchar(256) = '''' output' union all
select 'as' union all
select 'set nocount on' union all
select 'set transaction isolation level read uncommitted' union all
select 'set xact_abort on' union all
select ''

insert into @tmptb select 'delete from ' + @tablename + ' where ' + @keyname + ' = @' + @keyname

insert into @tmptb
select '' union all
select 'if @@rowcount <= 0 ' union all
select 'begin' union all
select '    set @outmsg = ''删除失败''' union all
select '    return 0' union all
select 'end' union all
select '' union all
select 'set @outmsg = ''删除成功''' union all
select 'return 1' union all
select '' union all select 'go'

select code as proc_delete from @tmptb

-- proc_detail
delete from @tmptb
insert into @tmptb select 'if object_id(N''p_' + @proctablename + '_detail'') is not null'
insert into @tmptb select ' drop procedure p_' + @proctablename + '_detail'
insert into @tmptb select 'go'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select '-- Procedure Name: cc p_' + @proctablename + '_detail,2,0'
insert into @tmptb select '-- Author: zx'
insert into @tmptb select '-- Date Generated: ' + datename(year,getdate()) + '年' + datename(month,getdate()) + '月'  + datename(day,getdate()) + '日'
insert into @tmptb select '-- Description: '
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select ''
insert into @tmptb select 'create procedure p_' + @proctablename + '_detail'

insert into @tmptb
select top 1
'@' + a.name + ' ' +
case b.name
when 'char' then 'char(' + convert(varchar,a.length) + ') = '''''
when 'nchar' then 'nchar(' + convert(varchar,a.length) + ')= '''''
when 'varchar' then 'varchar(' + convert(varchar,a.length) + ') = '''''
when 'nvarchar' then 'nvarchar(' + convert(varchar,a.length) + ') = '''''
else b.name + ' = 0' end
from syscolumns a inner join systypes b
on a.xtype = b.xtype
where a.id = object_id(@tablename)
order by a.colid

insert into @tmptb
select 'as' union all
select 'set nocount on' union all
select 'set transaction isolation level read uncommitted' union all
select 'set xact_abort on' union all
select ''

insert into @tmptb select 'select * from ' + @tablename + ' with(nolock) where ' + @keyname + ' = @' + @keyname

insert into @tmptb select '' union all select 'go'

select code as proc_detail from @tmptb

-- proc_listall
delete from @tmptb
insert into @tmptb select 'if object_id(N''p_' + @proctablename + '_listall'') is not null'
insert into @tmptb select ' drop procedure p_' + @proctablename + '_listall'
insert into @tmptb select 'go'
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select '-- Procedure Name: cc p_' + @proctablename + '_listall,1,0'
insert into @tmptb select '-- Author: zx'
insert into @tmptb select '-- Date Generated: ' + datename(year,getdate()) + '年' + datename(month,getdate()) + '月'  + datename(day,getdate()) + '日'
insert into @tmptb select '-- Description: '
insert into @tmptb select '----------------------------------------------------------'
insert into @tmptb select ''
insert into @tmptb select 'create procedure p_' + @proctablename + '_listall'

insert into @tmptb
select 'as' union all
select 'set nocount on' union all
select 'set transaction isolation level read uncommitted' union all
select 'set xact_abort on' union all
select ''

insert into @tmptb
select 'select * from ' + @tablename + ' with(nolock) '

insert into @tmptb select '' union all select 'go'

select code as proc_listall from @tmptb
GO
/****** Object:  StoredProcedure [dbo].[cc]    Script Date: 12/10/2020 16:07:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create procedure [dbo].[cc]      
@spname varchar(64),      
@exectype int = 0, -- 0 void 方法, 1 list 方法， 2 detail 方法      
@havereturnval int = 0, -- 是否有返回值, 0= 没有， 1= int, 2= boolean      
@tablename varchar(50) = ''  -- 可以写表名，如果不写将自动找到一个相关表      
as      
set nocount on      
set transaction isolation level read uncommitted      
set xact_abort on      
      
if( object_id(@spname) is null )      
begin      
    print 'not exists the procedure ' + @spname      
    return      
end      
      
      
if( @tablename <> '' and object_id(@tablename) is null )      
begin      
    print 'not exists the table ' + @tablename      
    return      
end      
      
      
      
declare @tmptable table (code varchar(4096))      
      
declare @tblist table(code varchar(4096))      
declare @tbdetail table(code varchar(4096))      
declare @tbvoid table(code varchar(4096))      
declare @tbdeclare table(code varchar(4096))      
      
      
declare @declares varchar(4096)      
      
declare @adoclassname varchar(50)      
declare @classname varchar(50)      
declare @pos int      
declare @word varchar(5)      
      
if( @tablename = '' )      
begin      
    declare @index int      
    set @index = charindex('_',reverse(@spname))      
    set @tablename = isnull(reverse(substring(reverse(@spname),@index + 1,len(@spname) - @index)), '' )      
    set @tablename = 't_' + right(@tablename,len(@tablename) - 2)      
end      
      
--print @tablename      
      
set @classname = @tablename      
if( @classname <> '' and left(@classname,2) = 't_')      
begin      
    set @classname = substring(@classname,3,len(@classname) - 2)      
end      
      
set @classname = upper(left(@classname,1)) + substring(@classname,2,len(@classname) - 1)      
while( charindex('_',@classname) > 0 )      
begin      
    set @pos = charindex('_',@classname)      
    set @word = substring(@classname,@pos + 1, 1)      
    set @classname = replace(@classname,'_' + @word,upper(@word) )      
end      
      
set @adoclassname = upper(left(@classname,1)) + substring(@classname,2,len(@classname) - 1) + 'Ado.cs'      
      
set @classname = upper(left(@classname,1)) + substring(@classname,2,len(@classname) - 1) + 'Info'      
      
      
select @adoclassname as ado数据库操作类文件名      
      
declare @totalcols int      
      
select @totalcols = count(colid) from syscolumns  where id = object_id(@spname)      
      
-- 生成变量定义代码      
      
if( @totalcols > 0 )      
    insert into @tbdeclare      
select      
case type_name(xusertype)      
when 'varchar' then  'string'      
when 'nvarchar' then  'string'      
when 'decimal' then 'decimal'      
when 'tinyint' then 'int'      
when 'smallint' then 'int'      
when 'datetime' then 'DateTime'      
when 'smalldatetime' then 'DateTime'      
when 'char' then 'string'      
when 'nchar' then 'string'      
when 'bigint'  then 'long'      
when 'text' then 'string'      
when 'ntext' then 'string'      
else type_name(xusertype)      
end      
+ ' '+  replace ( name ,'@','')      
+ ' ' + case type_name(xusertype)      
when 'varchar' then  '= ""'      
when 'nvarchar' then  '= ""'      
when 'decimal' then '= 0'      
when 'tinyint' then '= 0'      
when 'smallint' then '= 0'      
when 'int' then '= 0'      
when 'datetime' then '= DateTime.Now'      
when 'smalldatetime' then '=DateTime.Now'      
when 'char' then '= ""'      
when 'nchar' then '= ""'      
when 'bigint'  then '= 0'      
when 'text' then '= ""'      
when 'ntext' then '= ""'      
else '' end      
+ ';'  from syscolumns  where id = object_id(@spname)  order by colid      
      
set @declares = ''      
      
select @declares = @declares +      
      
case type_name(xusertype)      
when 'varchar' then  'string'      
when 'nvarchar' then 'string'      
when 'decimal' then 'decimal'      
when 'tinyint' then 'int'      
when 'smallint' then 'int'      
when 'datetime' then 'DateTime'      
when 'smalldatetime' then 'DateTime'      
when 'char' then 'string'      
when 'nchar' then 'string'      
when 'bigint'  then 'long'      
when 'text' then 'string'      
when 'ntext' then 'string'      
else type_name(xusertype)      
end      
 +   ' '+ name  + ', ' from syscolumns  where id = object_id(@spname) and isoutparam=0 order by colid      
      
select @declares = @declares +      
      
case type_name(xusertype)      
when 'varchar' then  'out string'      
when 'nvarchar' then  'out string'      
when 'decimal' then 'out decimal'      
when 'tinyint' then 'out int'      
when 'smallint' then 'out int'      
when 'datetime' then 'out DateTime'      
when 'smalldatetime' then 'DateTime'      
when 'char' then 'out string'      
when 'nchar' then 'out string'      
when 'bigint'  then 'out long'      
when 'text' then 'out string'      
when 'ntext' then 'out string'      
else 'out ' + type_name(xusertype)      
end      
+ ' ' +  name + ', '      
from syscolumns  where id = object_id(@spname) and isoutparam=1 order by colid      
      
set @declares = replace ( @declares ,'@','')      
      
if( charindex('out int totalcount',@declares) > 0)      
    set @declares = replace(@declares, 'out int', 'ref int')      
if( charindex('out int total',@declares) > 0)      
    set @declares = replace(@declares, 'out int', 'ref int')      
      
if ( @totalcols > 0 )      
begin      
      
    -- 生成bool 或者 void函数代码      
    if ( @havereturnval = 1 )      
     insert into @tbvoid select 'public static int ' + @spname + '(' +  left(@declares,len(@declares)-1) +' , CDatabase db )'      
    else if ( @havereturnval = 2 )      
     insert into @tbvoid select 'public static bool ' + @spname + '(' +  left(@declares,len(@declares)-1) +' , CDatabase db )'    
    else      
 insert into @tbvoid select 'public static void ' + @spname + '(' +  left(@declares,len(@declares)-1) +' , CDatabase db )'      
      
    -- 生成list函数代码      
    insert into @tblist select 'public static List<' + @classname + '> ' + @spname + '(' +  left(@declares,len(@declares)-1) +' , CDatabase db )'      
      
    -- 生成detail函数代码      
    insert into @tbdetail select 'public static ' + @classname + ' ' + @spname + '(' +  left(@declares,len(@declares)-1) +' , CDatabase db )'      
      
end      
else      
begin      
      
   -- 生成bool 或者 void函数代码      
    if ( @havereturnval = 1 )      
     insert into @tbvoid select 'public static int ' + @spname + '(' +  left(@declares,len(@declares)-1) +' , CDatabase db )'      
    else if ( @havereturnval = 2 )      
     insert into @tbvoid select 'public static bool ' + @spname + '(' +  left(@declares,len(@declares)-1) +' , CDatabase db )'     
    else      
 insert into @tbvoid select 'public static void ' + @spname + '( CDatabase db )'      
      
    -- 生成list函数代码      
    insert into @tblist select 'public static List<' + @classname + '> ' + @spname + '( CDatabase db )'      
      
    -- 生成detail函数代码      
    insert into @tbdetail select 'public static ' + @classname + ' ' + @spname + '( CDatabase db )'      
      
end      
      
insert into @tbvoid      
select '{' union all      
select 'List<SqlParameter> parms = new List<SqlParameter>();' union all      
select ''      
      
insert into @tblist      
select '{' union all      
select 'List<SqlParameter> parms = new List<SqlParameter>();' union all      
select ''      
      
insert into @tbdetail      
select '{' union all      
select 'List<SqlParameter> parms = new List<SqlParameter>();' union all      
select ''      
      
declare @parmNameUpper varchar(50)      
declare @parmName varchar(50)      
declare @parmType varchar(50)      
declare @isoutparam int      
declare @length int      
      
declare @cur cursor      
set @cur = cursor read_only forward_only for      
select name,type_name(xusertype),isoutparam,length from syscolumns where id = object_id(@spname) order by isoutparam      
      
open @cur      
fetch next from @cur into @parmName,@parmType,@isoutparam,@length      
while(@@fetch_status = 0)      
begin      
      
    set @parmName = substring(@parmName,2,len(@parmName) - 1)      
      
    set @parmNameUpper = upper(left(@parmName,1)) + substring(@parmName,2,len(@parmName) - 1)      
      
      
    if( @isoutparam = 1)      
    begin      
      
        insert into @tmptable      
    select '' union all      
    select      
    'SqlParameter p' + @parmNameUpper      
    + ' = new SqlParameter("@' + @parmName + '", SqlDbType.'      
    + case @parmType      
    when 'int' then 'Int, 4);'      
    when 'smallint' then 'SmallInt, 2)'      
    when 'bigint' then 'BigInt, 8);'      
    when 'varchar' then 'VarChar, ' + convert(varchar,@length)  + ');'      
    when 'nvarchar' then 'NVarChar, ' + convert(varchar,@length)  + ');'      
    when 'tinyint' then 'TinyInt, 1);'      
    when 'decimal' then 'Decimal);'      
    when 'float' then 'Float);'      
    when 'char' then 'Char, ' + convert(varchar,@length)  + ');'      
    when 'nchar' then 'NChar,' + convert(varchar,@length)  + ');'      
    when 'datetime' then 'DateTime);'      
    when 'smalldatetime' then 'SmallDateTime);'      
    when 'text' then 'Text, ' + convert(varchar,@length)  + ');'      
    when 'ntext' then 'NText, ' + convert(varchar,@length)  + ');'      
    else @parmType end      
      
        if( charindex('total',@parmName) > 0 )      
            insert into @tmptable      
            select 'p' + @parmNameUpper  + '.Direction = ParameterDirection.InputOutput;' union all      
            select 'p' + @parmNameUpper  + '.Value = ' + @parmName + ';'      
        else      
            insert into @tmptable      
            select 'p' + @parmNameUpper + '.Direction = ParameterDirection.Output;'      
      
    insert into @tmptable      
    select 'parms.Add(p' + @parmNameUpper + ');'      
      
    end      
    else      
    begin      
      
    insert into @tmptable      
    select 'parms.Add(new SqlParameter("@'+ @parmName + '",' + @parmName +  '));'      
      
    end      
      
    fetch next from @cur into @parmName,@parmType,@isoutparam,@length      
      
end      
      
if ( @havereturnval > 0 )      
begin      
      
    insert into @tmptable      
    select 'SqlParameter pRetval = new SqlParameter("@retval", SqlDbType.Int, 4);'      
      
    insert into @tmptable      
    select 'pRetval.Direction = ParameterDirection.ReturnValue;'      
      
    insert into @tmptable      
    select 'parms.Add(pRetval);'  union all      
    select ''      
      
end      
      
insert into @tbvoid(code) select code from @tmptable      
insert into @tblist(code) select code from @tmptable      
insert into @tbdetail(code) select code from @tmptable      
delete from @tmptable      
      
if( @totalcols > 0 )      
begin      
      
insert into @tbvoid  select 'db.execute_procedure("' + @spname + '",parms);' union all select ''      
insert into @tblist  select 'db.fetch_procedure("' + @spname + '",parms);' union all select ''      
insert into @tbdetail select 'db.fetch_procedure("' + @spname + '",parms);' union all select ''      
      
end      
else      
begin      
      
insert into @tbvoid select 'db.execute_procedure("' + @spname + '");' union all select ''      
insert into @tblist select 'db.fetch_procedure("' + @spname + '");' union all select ''      
insert into @tbdetail select 'db.fetch_procedure("' + @spname + '");' union all select ''      
      
end      
      
if exists(select 1 from syscolumns where id= object_id(@spname) and isoutparam = 1 )      
begin      
      
insert into @tmptable select      
 replace(name,'@','') + '= ( p' +  upper(substring(name,2,1)) + substring(name,3,len(name) - 2)  + '.Value == System.DBNull.Value ) ? "" : Convert.ToString( p' + upper(substring(name,2,1)) + substring(name,3,len(name) - 2)  + '.Value ) ; '      
      
      
      
      
from syscolumns  where id = object_id(@spname) and isoutparam=1      
 and ( type_name(xusertype)='varchar' or type_name(xusertype)='char' or type_name(xusertype)='text' or type_name(xusertype)='nchar' or type_name(xusertype)='nvarchar' or type_name(xusertype)='ntext' )      
 order by colid      
      
insert into @tmptable select      
 replace(name,'@','') + '= ( p'  + upper(substring(name,2,1)) + substring(name,3,len(name) - 2)  +  '.Value == System.DBNull.Value ) ? 0 : Convert.ToInt32( p' + upper(substring(name,2,1)) + substring(name,3,len(name) - 2) + '.Value ) ; '      
      
      
      
      
from syscolumns  where id = object_id(@spname) and isoutparam=1      
 and ( type_name(xusertype)='int' or type_name(xusertype)='bigint' or type_name(xusertype)='tinyint' or type_name(xusertype)='smallint' )      
 order by colid      
      
insert into @tmptable select      
 replace(name,'@','') + '= ( p'  + upper(substring(name,2,1)) + substring(name,3,len(name) - 2)  +  '.Value == System.DBNull.Value ) ? 0 : Convert.ToInt64( p' + upper(substring(name,2,1)) + substring(name,3,len(name) - 2) + '.Value ) ; '      
      
      
      
                from syscolumns  where id = object_id(@spname) and isoutparam=1 and type_name(xusertype)='bigint'      
 order by colid      
      
insert into @tmptable select ''      
      
end      
      
if ( @havereturnval=1 )      
begin      
    insert into @tmptable select 'int retval = ( pRetval.Value == System.DBNull.Value ) ? 0 : Convert.ToInt32(pRetval.Value); ' union all      
    
    select 'return retval;'      
end      
if ( @havereturnval=2 )      
begin      
    insert into @tmptable select 'int retval = ( pRetval.Value == System.DBNull.Value ) ? 0 : Convert.ToInt32(pRetval.Value); ' union all      
    select 'if (retval >= 1)' union all      
    select '     return true;' union all      
    select '' union all      
    select 'return false;'      
end     
      
insert into @tbvoid(code) select code from @tmptable      
insert into @tblist(code) select code from @tmptable      
insert into @tbdetail(code) select code from @tmptable      
delete from @tmptable      
      
if( @tablename <> '')      
begin      
    --取列表      
    insert into @tblist      
 select 'if(db.num_rows <= 0) ' union all      
    select 'return null;' union all      
    select 'List<' + @classname +  '> list = new List<' + @classname +  '>() ;' union all      
    select 'for (int i = 0;i <= db.num_rows - 1;i++ ) ' union all      
    select '{ ' union all      
    select @classname +  ' c = new ' + @classname + '(); '  union all      
    select      
    'c.' + lower(a.name) + ' = (db.rows[i]["' + a.name + '"] == System.DBNull.Value) ? ' +      
    case b.name      
    when 'decimal' then '0 : Convert.ToDecimal(db.rows[i]["' + a.name + '"].ToString())'      
    when 'int' then '0 : Convert.ToInt32(db.rows[i]["' + a.name + '"].ToString())'      
    when 'smallint' then '0 : Convert.ToInt32(db.rows[i]["' + a.name + '"].ToString())'      
    when 'tinyint' then '0 : Convert.ToInt32(db.rows[i]["' + a.name + '"].ToString())'      
    when 'bigint' then '0 : Convert.ToInt64(db.rows[i]["' + a.name + '"].ToString())'      
    when 'bit' then '0 : Convert.ToInt32(db.rows[i]["' + a.name + '"].ToString())'      
    when 'char' then '"" : db.rows[i]["' + a.name + '"].ToString()'      
    when 'nchar' then '"" : db.rows[i]["' + a.name + '"].ToString()'      
    when 'varchar' then '"" : db.rows[i]["' + a.name + '"].ToString()'      
    when 'nvarchar' then '"" : db.rows[i]["' + a.name + '"].ToString()'      
    when 'nvarchar' then '"" : db.rows[i]["' + a.name + '"].ToString()'      
    when 'smalldatetime' then 'DateTime.MinValue : Convert.ToDateTime(db.rows[i]["' + a.name + '"].ToString())'      
    when 'datetime' then 'DateTime.MinValue : Convert.ToDateTime(db.rows[i]["' + a.name + '"].ToString())'      
    else '"" : db.rows[i]["' + a.name + '"].ToString()' end + ';'      
    from syscolumns a inner join systypes b      
    on a.xtype = b.xtype      
    where a.id = object_id(@tablename) union all      
    select 'list.Add(c); ' union all      
    select '} ' union all      
    select 'return list; '      
      
    --取详细      
    insert into @tbdetail      
    select 'if(db.num_rows <= 0) ' union all      
    select 'return null;' union all      
    select @classname + ' c = new ' + @classname + '(); ' union all      
    select      
    'c.' + lower(a.name) + ' = (db.rows[0]["' + a.name + '"] == System.DBNull.Value)?' +      
    case b.name      
    when 'decimal' then '0 : Convert.ToDecimal(db.rows[0]["' + a.name + '"].ToString())'      
    when 'int' then '0 : Convert.ToInt32(db.rows[0]["' + a.name + '"].ToString())'      
    when 'smallint' then '0 : Convert.ToInt32(db.rows[0]["' + a.name + '"].ToString())'      
    when 'tinyint' then '0 : Convert.ToInt32(db.rows[0]["' + a.name + '"].ToString())'      
   when 'bigint' then '0 : Convert.ToInt64(db.rows[0]["' + a.name + '"].ToString())'      
    when 'bit' then '0 : Convert.ToInt32(db.rows[0]["' + a.name + '"].ToString())'      
    when 'char' then '"" : db.rows[0]["' + a.name + '"].ToString()'      
    when 'nchar' then '"" : db.rows[0]["' + a.name + '"].ToString()'      
    when 'varchar' then '"" : db.rows[0]["' + a.name + '"].ToString()'      
    when 'nvarchar' then '"" : db.rows[0]["' + a.name + '"].ToString()'      
    when 'nvarchar' then '"" : db.rows[0]["' + a.name + '"].ToString()'      
    when 'smalldatetime' then 'DateTime.MinValue : Convert.ToDateTime(db.rows[0]["' + a.name + '"].ToString())'      
    when 'datetime' then 'DateTime.MinValue : Convert.ToDateTime(db.rows[0]["' + a.name + '"].ToString())'      
   else '"" : db.rows[0]["' + a.name + '"].ToString()' end + ';'      
    from syscolumns a inner join systypes b      
    on a.xtype = b.xtype      
    where a.id = object_id(@tablename) union all      
    select 'return c;'      
      
      
end      
      
insert into @tbvoid select '}'      
insert into @tblist select '}'      
insert into @tbdetail select '}'      

select 'using Lexun.Common;' as using_namespace union all      
select 'using System.Data;' as using_namespace union all      
select 'using System.Data.SqlClient;' as using_namespace      
      
if( @exectype = 0)      
    select code as void方法 from @tbvoid      
else if( @exectype = 1 and @tablename <> '')      
    select code as list返回列表方法 from @tblist      
else if( @exectype = 2 and @tablename <> '')      
    select code as detail返回详细方法 from @tbdetail      
      
select code as 定义变量 from @tbdeclare
GO
/****** Object:  StoredProcedure [dbo].[p_tb_test_list]    Script Date: 12/10/2020 16:07:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
----------------------------------------------------------
-- Procedure Name: cc p_tb_test_list ,1,0
-- Author: lwt
-- Date Generated: 2020年12月10日
-- Description: 
----------------------------------------------------------

create procedure [dbo].[p_tb_test_list]
@page int = 1, --当前页
@pagesize int = 10, --每页显示
@total int = 0 output, --总记录数
@name varchar(50) = 0,
@age int = 0
as
set nocount on
set transaction isolation level read uncommitted
set xact_abort on

declare @tablename varchar(1024)
declare @orderby  varchar(1024)
declare @orderby2 varchar(1024)
declare @orderbyclause varchar(1024)
declare @keyname varchar(50)
declare @keyinorder tinyint
declare @whereclause varchar(1024)
declare @showclause varchar(1024)

-- 表名称
set @tablename = 'tb_test' 
--排序字段,字段前面加b.,字段后加 desc或asc,格式如 b.rid desc,b.rname asc
set @orderby = 'b.name asc,b.age asc' 
--排序字段,字段前面加b.,字段后面和上面的orderby正好相反,格式如 b.rid asc,b.ranme desc
set @orderby2 = 'b.name desc,b.age desc' 
--排序字段，这里不需要加 asc 和 desc，格式如 b.rid,b.rname
set @orderbyclause = 'b.name,b.age'
--主键是否在排序字段里，1 是 0 不是，一般是1
set @keyinorder = 0
--这个表的主键
set @keyname = 'id'
--查询条件
set @whereclause = ' where 1=1 '
if @name != ''
	set @whereclause += 'and CHARINDEX('''+@name+''',name)>0 ' 
if @age>0
	set @whereclause += ' and age> '+ CONVERT(varchar,@age)

set @showclause = 'a.*'

--调用分页存储过程
exec P_SplitPageOneSql_v2 @tablename,@pagesize,@page,@orderby,@orderby2,
@orderbyclause,@keyname,@keyinorder,@whereclause,@showclause,@total output
GO
