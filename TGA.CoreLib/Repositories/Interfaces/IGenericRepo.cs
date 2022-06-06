using System.Linq.Expressions;

namespace TGA.CoreLib.Repositories.Interfaces
{
    public interface IGenericRepository
    {
        public Task LogData<TData>(TData data, Expression<Func<TData, bool>>? filter = null);
        public Task<List<TData>> GetData<TData>(Expression<Func<TData, bool>> filter);
    }
}
