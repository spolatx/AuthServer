using Microsoft.AspNetCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedLibrary.Dtos
{
    public class Response<T> where T : class
    {
        //T yerine obje alsaydık (line 12) başarısız olma durumlarında datayı almamıza gerek kalmaz. ama obje yaparsak da  ilgili generic datayı bir cast işlemi yapmamız gerekir. maliyetli bir işlem. o yüzden direkt generic verdik.
        public T Data { get; private set; }
        public int StatusCode { get; private set; }

        [JsonIgnore]
        public bool IsSuccessful { get; private set; }
        public ErrorDto Error { get; private set; }

        public static Response<T> Success(T data, int statusCode)
        {
            //Herhangi bir ekleme durumunda geriye datayı dönmek gerekir.
            return new Response<T> { Data = data, StatusCode = statusCode , IsSuccessful= true };
        }
        public static Response<T> Success(int statusCode)
        {
            //data boş olucak ama status code dolu olacak.
            //ürünü silme ya da güncelleme yaptığımızda bu endpointlerden geriye bir 
            //data dönmeye gerek yoktur. 200 durum koduyla beraber boş data döneriz.
            return new Response<T> { Data = default, StatusCode = statusCode , IsSuccessful=true };
        }
        public static Response<T> Fail(ErrorDto errorDto, int statusCode)
        {
            return new Response<T> { Error = errorDto, StatusCode = statusCode , IsSuccessful=false};
        }
        //Tek bir hata olduğunda ErrorDto'dan nesne örneği alıp ErrorDtonun Errorsunu doldurmak istemiyorum. bu yüzden yardımcı methot tanımladım.
        public static Response<T> Fail(string errorMessage,int statusCode, bool isShow)
        {
            var errorDto = new ErrorDto(errorMessage, isShow);
            return new Response<T> { Error=errorDto, StatusCode = statusCode , IsSuccessful=false };
        }
    }
}
