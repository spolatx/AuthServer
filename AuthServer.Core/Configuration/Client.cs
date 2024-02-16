using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Core.Configuration
{
    public class Client
    {
        public string Id { get; set; }
        public string Secret { get; set; }
         
        //Göndereceğimiz token da hangi api içerisine erişebileceğini tutucam.
        //www.myapi1.com www.myapi2.com dönen token da hangisi varsa ona erişecek. 
        public List<String> Audiences { get; set; }
    }
}
