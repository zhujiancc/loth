using System;
using System.Configuration;

namespace loth.kafka
{
    public class KafkaConfig
    {
        public static string GetServers(KafkaServerType type)
        {
            switch (type)
            {
                case KafkaServerType.Douyin:
                    return "innerkfk.xiguaji.com:9092";
                case KafkaServerType.XiGuaWx:
                    return "innerkfk.xiguaji.com:9092";
                case KafkaServerType.WeiBo:
                    return "innerkfk.xiguaji.com:9092";
                case KafkaServerType.XiaoHongShu:
                    return "innerkfk.xiguaji.com:9092";
                case KafkaServerType.KuaiShou:
                    return "innerkfk.xiguaji.com:9092";
                case KafkaServerType.ZhiGua:
                    return "innerkfk.xiguaji.com:9092";
                case KafkaServerType.BStation:
                    return "innerkfk.xiguaji.com:9092";
                case KafkaServerType.AliYunDouYin:
                    return "172.16.94.243:9092,172.16.94.242:9092,172.16.94.241:9092";
                case KafkaServerType.Caiji:
                    return "innerkfk02.xiguaji.com:9092";
            }
            return "innerkfk.xiguaji.com:9092";
        }

        public static string DefaultServers => "innerkfk.xiguaji.com:9092";
    }
}
