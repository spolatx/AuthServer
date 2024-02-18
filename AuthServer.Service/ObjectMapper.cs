using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service
{
    public static class ObjectMapper
    {
        //Ben datayı alana kadar memory de bulunmasın bu static arkadaş.
        //ben istediğim zaman yüklensin ben çağırmazsam boşu boşuna memory de bulunmasın.
        private static readonly Lazy<IMapper> lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                //cfg.Internal().MethodMappingEnabled = false;
                cfg.AddProfile<DtoMapper>();
            });
            return config.CreateMapper();
        });
        //Ben ObjectMapper.Mapper ı çağırana kadar içeride ki kod memorye yüklenmeyecek yani memoryde çalışmayacak ne zaman ki ben çağırırsam isimsiz olan kodda memory e yüklenecek( () => bu kısımdan bahsediyorum isimsiz kod olarak)  ve arkasından memorye bir kere yüklendikten sonra ben kullanmaya devam edicem.
        public static IMapper Mapper => lazy.Value;
    }
}
