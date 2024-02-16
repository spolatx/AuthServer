using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    //Veri tabanıyla iletişim kuracağım UoW u çağıracağım yer burası.

    public class ServiceGeneric<TEntity, TDto> : IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IGenericRepository<TEntity> _genericRepository;

        public ServiceGeneric(IUnitOfWork unitOfWork,IGenericRepository<TEntity> genericRepository)
        {
                _unitOfWork = unitOfWork;
            _genericRepository = genericRepository;
        }

        public async Task<Response<TDto>> AddAsync(TDto dto)
        {
            //elimde dto dan dönüştürmüş olduğum bir entity nesnem var
            var newEntity = ObjectMapper.Mapper.Map<TEntity>(dto);

            await _genericRepository.AddAsync(newEntity);

            await _unitOfWork.CommitAsync();

            /* üst kısımda yeni data eklendi veritabanına gitti ve entitynin idsi eklendi
            ben geriye dto dönücem o zaman bu dtonun da idsini doldurmam gerekli.
            o yüzden geriye tekrar dto nesnesine dönüştürmemiz gerekir. */

            var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);

            return Response<TDto>.Success(newDto,200);
        }

        public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
        {
            var products = ObjectMapper.Mapper.Map<List<TDto>>(await _genericRepository.GetAllAsync());

            //burada data geldikten sonra bu product üzerinde yeni whereler yazarsan
            //bu arkadaş geriye Inumerable döndüğünden dolayı (GetAllAsync)
            //direkt olarak veri tabanından toListAsync methoduyla beraber datayı aldı.
            //gelen datanın üzerinde başka işlem yapmayacaksanız IEnumarable dönebilirsiniz.

            return Response<IEnumerable<TDto>>.Success(products, 200);

        }

        public async Task<Response<TDto>> GetByIdAsync(int id)
        {
            var product = await _genericRepository.GetByIdAsync(id);
            if (product==null)
            {
                return Response<TDto>.Fail("Id not found",404,true);
            }
            var newDto = ObjectMapper.Mapper.Map<TDto>(product);
            return Response<TDto>.Success(newDto, 200);
        }

        public async Task<Response<NoDataDto>> Remove(int id)
        {
            var isExistEntity = await _genericRepository.GetByIdAsync(id);
            if (isExistEntity==null)
            {
                return Response<NoDataDto>.Fail("Id not found",404,true);
            }
            //isExistEntitynin stateini deleted olarak işaretledik
          _genericRepository.Remove(isExistEntity);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<NoDataDto>> Update(TDto entity ,int id)
        {
            //şu an da bu arkadaş memory de track edilmiyor.
            var isExistEntity = await _genericRepository.GetByIdAsync(id);
            if (isExistEntity==null)
            {
                return Response<NoDataDto>.Fail("Id not found", 404, true);
            }

            //eğer ben burada track olayını kaldırmasaydım (isExistEntity olan arkadaş)
            //update dediğimizde iki tane arkadaş(updateEntity ve ExistEntity) o anda memory de state i modify olarak işaretlenicek ve hata vericekti.
            //

            var updateEntity = ObjectMapper.Mapper.Map<TEntity>(entity);
            _genericRepository.Update(updateEntity);

            await _unitOfWork.CommitAsync();
            //204 durum kodu noContent => Response bodysinde hiç bir data olmayacak.
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
        {
           var list = _genericRepository.Where(predicate);
            //list.Skip(4).Take(5);
            //list.ToListAsync();
            return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>(await list.ToListAsync()),200);
        }
    }
}
