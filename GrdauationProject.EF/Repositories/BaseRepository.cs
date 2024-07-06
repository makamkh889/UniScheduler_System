using GraduationProject.core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GrdauationProject.EF.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> GetAll() => _context.Set<T>().ToList();

        public async Task<T> Get(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var incluse in includes)
                    query = query.Include(incluse);

            return await query.SingleOrDefaultAsync(criteria);
        }
        public async Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.Where(criteria).ToListAsync();
        }
        public async Task<T> Add(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }
        public T Update(T entity)
        {
            _context.Update(entity);
            return entity;
        }

        public T HardDelete(T entity)
        {
            _context.Set<T>().Remove(entity);
            return entity;
        }
        public T SoftDelete(T entity)
        {
            return Update(entity);
        }
    }
}
