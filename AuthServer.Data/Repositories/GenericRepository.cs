using AuthServer.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Data.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity :class
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _dbSet;
        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }
        public async Task AddAsync(TEntity entity)
        {
            //Async şekilde ekliyor memorye bir data ekliyor ki o an ki tread bloklanmasın diye.
            await _dbSet.AddAsync(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
          return await _dbSet.ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            //find methodu primary key üzerinden arama gerçekleştirir.
            //başka bir alan üzerinden arama yapmaz.
            var entity = await _dbSet.FindAsync(id);
            if (entity!=null)
            {
                /* Bu arkadaş memory de takip edilmesin update ve delete işlemlerinde
                 ilgili ürünün id sine göre bu ürünü silmeden once update edeceğim
                 data var mı kontrol etmek istiyorum. O yüzden bu arkadaşın takip
                  edilmesini istemiyoruz track edilmesin memoryde tutulmasın. */
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;

        }

        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public TEntity Update(TEntity entity)
        {
            /*Repository Patternin dezavantajlarından bir tanesi
              tek bir alanda bile değişiklik yapsan sanki tamamında değişiklik yapılmış
              gibi güncelliyor 
            Burada async bir işlem yapmıyoruz sadece var olan entitynin stateini değiştiriyoruz.
             
             */
            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbSet.Where(predicate); 
        }
        //INumerable GetAllAsync ile beraber tüm datayı memory e aldık bundan sonra yapacağımız tüm order bylar diğer where sorgularının hepsi o anki memorydeki data da gerçekleşir.
        //Ama IQueryable da  where sorgusuyla beraber yapılacak sorgular memoryde gerçekleşir
        //ama veritabanından daha datayı çekmemiştir. Tüm kodlama yapılır where order by find vs.. en son toList dediğimiz anda tüm sorgular birleştirilir ve tek bir seferde veri tabanına yansıtılır. IQueryable daha efektif bir yöntem. 
    }
}
