using loth.rmq;
using RabbitMQ.Client;
using SqlSugar;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Xunit;

namespace loth.test
{
    public class SugarTest
    {
        [Fact]
        public void BaseTest()
        {
            SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "postgresql://postgres:zhujian@121.5.222.227/demo",
                DbType = DbType.PostgreSQL,
                IsAutoCloseConnection = true
            });

            //调试SQL事件，可以删掉
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql);//输出sql,查看执行sql
                                       //5.0.8.2 获取无参数化 SQL 
                                       //UtilMethods.GetSqlString(DbType.SqlServer,sql,pars)
            };


            var kw = db.Queryable<BusinessSearchKeyWord>()
                .Where(x => x.CategoryId == 1334)
                .Where(x => x.TargetWord == "玩具")
                .First();

            Console.WriteLine(kw.MonthPV);

        }

        [SugarTable("BusinessSearchKeyWord")]
        public class BusinessSearchKeyWord
        {
            [SugarColumn(IsPrimaryKey = true)]
            public int CategoryId { get; set; }

            [SugarColumn(IsPrimaryKey = true)]
            public string TargetWord { get; set; }

            public float SuggestedBid { get; set; }

            public int MonthPV { get; set; }

        }
    }
}