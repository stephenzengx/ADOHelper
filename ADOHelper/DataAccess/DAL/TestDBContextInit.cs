using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAL
{
    //在EF中初始化数据库有三种策略：
    // CreateDatabaseIfNotExists：该项也是默认初始化数据库的一项，要是数据库不存在就创建数据库。
    // DropCreateDatabaseIfModelChanges：只要数据模型发生了改变就重新创建数据库。
    // DropCreateDatabaseAlways：只要每次初始化上下文时就创建数据库。
    public class TestDBContextInit : CreateDatabaseIfNotExists<MySqlTestDb>
    {
        protected override void Seed(MySqlTestDb context)
        {
            base.Seed(context);
        }
    }
}
