using AuthServer.Core.DTOs;
using AuthServer.Core.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service
{
    public class DtoMapper : Profile
    {
        //Entityleri mappleyelim.
        public DtoMapper()
        {
            //Productdto yu product a dönüştür ya da tam tersi de olabilir.
            CreateMap<ProductDto,Product>().ReverseMap();
            CreateMap<UserAppDto,UserApp>().ReverseMap();
                
        }

    }
}
