using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Microsoft.Research.Science.Data.Climate
{
    public class ServiceLocationConfiguration : ConfigurationSection
    {
        private static ServiceLocationConfiguration section = null;


        public static ServiceLocationConfiguration Current
        {
            get
            {
                if (section == null)
                {
                    var conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    section = conf.GetSection("ServiceLocationConfiguration") as ServiceLocationConfiguration;
                }
                return section;
            }
        }

        [ConfigurationProperty("ServiceURL", IsRequired = false)]
        public string ServiceURL
        {
            get
            {
                return (string)this["ServiceURL"];
            }
            set
            {
                this["ServiceURL"] = value;
            }
        }

        [ConfigurationProperty("CommunicationProtocol", IsRequired = false)]
        public string CommunicationProtocol
        {
            get
            {
                return (string)this["CommunicationProtocol"];
            }
            set
            {
                this["CommunicationProtocol"] = value;
            }
        }
    }
}
