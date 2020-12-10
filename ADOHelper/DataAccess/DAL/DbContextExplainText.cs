using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataAccess.DAL
{
    public class DbContextExplainText
    {
        //https://www.cnblogs.com/lsxqw2004/p/4701979.html
        //https://www.cnblogs.com/summit7ca/p/5423637.html
        //https://www.cnblogs.com/buyixiaohan/p/7259342.html

        #region DbContext类：
        //public DbContext(string nameOrConnectionString);

        //public DbContext(string nameOrConnectionString, DbCompiledModel model);

        //public DbContext(DbConnection existingConnection, bool contextOwnsConnection);

        //public DbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext);

        //public DbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection);

        //protected DbContext();

        //protected DbContext(DbCompiledModel model);

        //返回一个DbChangeTracker对象，通过这个对象的Entries属性，我们可以查询EF Context中所有缓存的实体的状态。
        //public DbChangeTracker ChangeTracker { get; }

        //public DbContextConfiguration Configuration { get; }

        /*  Database属性：一个数据库对象的表示，
            通过其SqlQuery、ExecuteSqlCommand等方法可以直接执行一些Sql语句或SqlCommand；
            EF6起可以通过Database对象控制事务。*/
        //public Database Database { get; }

        //public void Dispose();

        //获取EF Context中的实体的状态，在更改跟踪一节会讨论其作用。
        //public DbEntityEntry Entry(object entity);
        //public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        //public override bool Equals(object obj);

        //public override int GetHashCode();

        //public Type GetType();

        //public IEnumerable<DbEntityValidationResult> GetValidationErrors();

        //将实体保存到数据库
        //public virtual int SaveChanges();

        //public virtual Task<int> SaveChangesAsync();

        //public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        //获取实体相应的DbSet对象，我们对实体的增删改查操作都是通过这个对象来进行的。
        //public virtual DbSet Set(Type entityType);

        //public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class;

        //public override string ToString();

        //protected virtual void Dispose(bool disposing);

        //protected virtual void OnModelCreating(DbModelBuilder modelBuilder);

        //protected virtual bool ShouldValidateEntity(DbEntityEntry entityEntry);

        //protected virtual DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items);
        #endregion

        #region DbSet类
        //Include：详见下面预加载一节。
        //AsNoTracking：相见变更跟踪一节。

        //protected internal DbSet();

        //Local属性：用来跟踪所有EF Context中状态为Added，Modified、Unchanged的实体。
        //作用好像不是太大。没怎么用过。
        //public virtual IList Local { get; }

        //将一个已存在于数据库中的对象添加到EF Context中，实体状态被标记为Added。
        //  对于已有相同key的对象存在于EF Context且状态为Added则不进行任何操作
        //public virtual object Add(object entity);

        //public virtual IEnumerable AddRange(IEnumerable entities);

        //将一个已存在于数据库中的对象添加到EF Context中，实体状态被标记为Unchanged。
        //  对于已有相同key的对象存在于EF Context的情况，
        //  如果这个已存在对象状态为Unchanged则不进行任何操作，否则将其状态更改为Unchanged。???
        //public virtual object Attach(object entity);

        //public DbSet<TEntity> Cast<TEntity>() where TEntity : class;

        //这个方法至今好像没有用到过，不知道干啥的。有了解的评论中给解释下吧。
        //public virtual object Create();
        //public virtual object Create(Type derivedEntityType);

        //public override bool Equals(object obj);


        //按主键获取一个实体，首先在EF Context中查找是否有被缓存过的实体，
        //  如果查找不到再去数据库查找，如果数据库中存在则缓存到EF Context并返回，否则返回null。
        //public virtual object Find(params object[] keyValues);
        //public virtual Task<object> FindAsync(params object[] keyValues);
        //public virtual Task<object> FindAsync(CancellationToken cancellationToken, params object[] keyValues);

        //public override int GetHashCode();

        //public Type GetType();

        //将一个已存在于EF Context中的对象标记为Deleted，当SaveChanges时，这个对象对应的数据库条目被删除。
        //注意，调用此方法需要对象已经存在于EF Context。
        //public virtual object Remove(object entity);

        //public virtual IEnumerable RemoveRange(IEnumerable entities);

        //public virtual DbSqlQuery SqlQuery(string sql, params object[] parameters);
        #endregion

        #region DbModelBuilder/EntityTypeConfiguration
        /*
         0-DbModelBuilder
         1-Code First模式下EF的映射配置：
        通过Code First来实现映射模型有两种方式Data Annotation(注解)和Fluent API。
    
        Data Annotation需要在实体类（我通常的称呼，一般就是一个Plain Object）的属性上以Attribute的方式
            表示主键、外键等映射信息。这种方式不符合解耦合的要求所以一般不建议使用。

        第二种方式就是要重点介绍的Fluent API。Fluent API的配置方式将实体类与映射配置进行解耦合，有利于项目的扩展和维护。

        Fluent API方式中的核心对象是DbModelBuilder。


        2-EntityTypeConfiguration： 类
        ToTable：指定映射到的数据库表的名称。

        HasKey：配置主键（也用于配置关联主键）

        Property：这个方法返回PrimitivePropertyConfiguration的对象，
        根据属性不同可能是子类StringPropertyConfiguration的对象。
        通过这个对象可以详细配置属性的信息如IsRequired()或HasMaxLength(400)。

        Ignore：指定忽略哪个属性（不映射到数据表）
         */
        #endregion

    }
}