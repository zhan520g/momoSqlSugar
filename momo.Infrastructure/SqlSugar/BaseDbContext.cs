using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace momo.Infrastructure.SqlSugar
{
    /// <summary>
    /// 数据库操作类
    /// </summary>
    public class BaseDbContext
    {

        /// <summary>
        /// 注意当前方法的类不能是静态的 public static class这么写是错误的
        /// </summary>
        public static SqlSugarClient Db
        {
            get
            {
                var connMain = ConfigurationManager.ConnectionStrings["ConnectionStrings:ConnMain"];  //主库
                var connMain2 = ConfigurationManager.GetSection("ConnectionStrings:ConnMain");
                var connFrom = ConfigurationManager.ConnectionStrings["ConnectionStrings:ConnFrom"];  //从库
                return InitDataBase(connFrom == null
                    ? new List<string> { connMain.ToString() }
                    : new List<string> { connMain.ToString(), connFrom.ToString() });
            }
        }


        /// <summary>
        ///  获得SqlSugarClient, 包含了主从库的设置
        /// </summary>
        /// <param name="serverIp">服务器IP或文件路径</param>
        /// <param name="user">用户名</param>
        /// <param name="pass">密码</param>
        /// <param name="dataBase">数据库</param>
        public static SqlSugarClient GetIntance(string serverIp, string user, string pass, string dataBase)
        {
            var listConn = new List<string>();
            switch ((DbType)ConfigurationManager.ConnectionStrings["DbType"].ObjToInt())
            {
                case DbType.SqlServer:
                    listConn.Add($"server={serverIp};user id={user};password={pass};persistsecurityinfo=True;database={dataBase}");
                    break;
                case DbType.MySql:
                    listConn.Add($"Server={serverIp};Database={dataBase};Uid={user};Pwd={pass};");
                    break;
                case DbType.Oracle:
                    listConn.Add($"Server=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={serverIp})(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={dataBase})));User Id={user};Password={pass};Persist Security Info=True;Enlist=true;Max Pool Size=300;Min Pool Size=0;Connection Lifetime=300");
                    break;
                case DbType.PostgreSQL:
                    listConn.Add($"PORT=5432;DATABASE={dataBase};HOST={serverIp};PASSWORD={pass};USER ID={user}");
                    break;
                case DbType.Sqlite:
                    listConn.Add($"Data Source={serverIp};Version=3;Password={pass};");
                    break;
            }
            return InitDataBase(listConn);
        }
        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        /// <param name="listConn">连接字符串</param>
        private static SqlSugarClient InitDataBase(List<string> listConn)
        {
            var connStr = "";//主库
            var slaveConnectionConfigs = new List<SlaveConnectionConfig>();//从库集合
            for (var i = 0; i < listConn.Count; i++)
            {
                if (i == 0)
                {
                    connStr = listConn[i];//主数据库连接
                }
                else
                {
                    slaveConnectionConfigs.Add(new SlaveConnectionConfig()
                    {
                        HitRate = i * 2,
                        ConnectionString = listConn[i]
                    });
                }
            }

            //如果配置了 SlaveConnectionConfigs那就是主从模式,所有的写入删除更新都走主库，查询走从库，
            //事务内都走主库，HitRate表示权重 值越大执行的次数越高，如果想停掉哪个连接可以把HitRate设为0 
            var db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = connStr,
                DbType = (DbType)ConfigurationManager.ConnectionStrings["DbType"].ObjToInt(),
                IsAutoCloseConnection = true, //自动释放链接
                SlaveConnectionConfigs = slaveConnectionConfigs,
                IsShardSameThread = true
            });
            db.Ado.CommandTimeOut = 30000;//设置超时时间
            db.Aop.OnLogExecuted = (sql, pars) => //SQL执行完事件
            {
                //LogHelper.WriteLog($"执行时间：{db.Ado.SqlExecutionTime.TotalMilliseconds}毫秒 \r\nSQL如下：{sql} \r\n参数：{GetParams(pars)} ", "SQL执行");
            };
            db.Aop.OnLogExecuting = (sql, pars) => //SQL执行前事件
            {
                if (db.TempItems == null) db.TempItems = new Dictionary<string, object>();
            };
            db.Aop.OnError = (exp) =>//执行SQL 错误事件
            {
                //LogHelper.WriteLog($"SQL错误:{exp.Message}\r\nSQL如下：{exp.Sql}", "SQL执行");
                throw new Exception(exp.Message);
            };
            db.Aop.OnExecutingChangeSql = (sql, pars) => //SQL执行前 可以修改SQL
            {
                return new KeyValuePair<string, SugarParameter[]>(sql, pars);
            };
            db.Aop.OnDiffLogEvent = (it) => //可以方便拿到 数据库操作前和操作后的数据变化。
            {
                //var editBeforeData = it.BeforeData;
                //var editAfterData = it.AfterData;
                //var sql = it.Sql;
                //var parameter = it.Parameters;
                //var data = it.BusinessData;
                //var time = it.Time;
                //var diffType = it.DiffType;//枚举值 insert 、update 和 delete 用来作业务区分

                //你可以在这里面写日志方法
            };
            return db;
        }
        /// <summary>
        /// 获取参数信息
        /// </summary>
        /// <param name="pars"></param>
        /// <returns></returns>
        private static string GetParams(SugarParameter[] pars)
        {
            return pars.Aggregate("", (current, p) => current + $"{p.ParameterName}:{p.Value}, ");
        }
    }
}
