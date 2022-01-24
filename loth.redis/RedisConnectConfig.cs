using System;

namespace loth.redis
{
	public class RedisConnectConfig
	{
		public string Connect_Read { get; set; }

		public string Connect_Write { get; set; }

		public int DB { get; set; }

		public string ConnectKey
		{
			get
			{
				return string.Concat(new string[]
				{
					this.Connect_Read,
					":",
					this.Connect_Write,
					":",
					this.DB.ToString()
				});
			}
		}

		public RedisConnectConfig(string connect_read, string connect_write, int db = 0)
		{
			bool flag = string.IsNullOrEmpty(connect_read) || string.IsNullOrEmpty(connect_write);
			if (flag)
			{
				throw new Exception("Redis 连接串不能为空");
			}
			this.Connect_Read = connect_read;
			this.Connect_Write = connect_write;
			this.DB = db;
		}
	}
}
