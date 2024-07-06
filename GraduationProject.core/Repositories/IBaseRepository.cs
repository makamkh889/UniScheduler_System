using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject.core.Repositories
{
    public interface IBaseRepository<T> where T : class
    {

        T Update(T entity);
        Task<T> Add(T entity);
        T HardDelete(T entity);
        T SoftDelete(T entity);
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(Expression<Func<T, bool>> criteria, string[] includes = null);
        Task<IEnumerable<T>> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null);
    }
}
