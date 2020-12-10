using DataAccess.Models;
using System.Data.Entity;

namespace DataAccess.DAL
{
    //[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class MySqlTestDb : DbContext
    {
        //PM 迁移数据库
        //Enable-Migrations 启用迁移
        //Add-Migration record1  //给本次迁移记录起个名字 这个名字可以随便起,尽量别重名 注
        //Add-Migration record1 -IgnoreChanges  --在原来基础上新增
        //update-database

        public MySqlTestDb() : base("name=MySqlTestDB")
        {
            //初始化 EF策略
            Database.SetInitializer<MySqlTestDb>(new TestDBContextInit());

            // 禁用延迟加载
            //this.Configuration.LazyLoadingEnabled = false;

            //Database.SetInitializer<TestDbContext>(null);//关闭数据库初始化操作：
        }

        public DbSet<TB_UserAccount> TB_UserAccounts { get; set; }

        public DbSet<TB_Test> TB_Tests { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 禁用默认表名复数形式
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // 禁用一对多级联删除
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            // 禁用多对多级联删除
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            //还有一种方法可以通过程序集名称获得
            //var types = Assembly.GetExecutingAssembly().GetTypes();
            //一劳永逸
            //var typesRegister = Assembly.GetExecutingAssembly().GetTypes()
            //     .Where(type => !(string.IsNullOrEmpty(type.Namespace)))
            //     .Where(type => type.BaseType != null && type.BaseType.IsGenericType
            //    && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            //foreach (var type in typesRegister)
            //{
            //    dynamic configurationInstance = Activator.CreateInstance(type);
            //    modelBuilder.Configurations.Add(configurationInstance);
            //}

            //base.OnModelCreating(modelBuilder);
        }
    }
}
