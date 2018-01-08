using System.Data.Entity;
using Nop.Core.Infrastructure;

namespace DaBoLang.Nop.Plugin.Payments.AliPay.Data
{
    /// <summary>
    /// 命名空间：DaBoLang.Nop.Plugin.Payments.AliPay.Data
    /// 名    称：EfStartUpTask
    /// 功    能：启动任务
    /// 详    细：启动时数据库初始化从不创建数据库
    /// 版    本：1.0.0.0
    /// 文件名称：EfStartUpTask.cs
    /// 创建时间：2017-08-03 12:05
    /// 修改时间：2017-08-04 01:29
    /// 作    者：大波浪
    /// 联系方式：http://www.cnblogs.com/yaoshangjin
    /// 说    明：
    /// </summary>
    public class EfStartUpTask : IStartupTask
    {
        public void Execute()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<AliPayObjectContext>(null);
        }

        public int Order
        {
            //ensure that this task is run first 
            get { return 0; }
        }
    }
}
