namespace ContactBook_Services.Repository
{
    public interface IRepository<T,V> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(V id);

        Task<bool> AddAsync(T entity);

        Task<bool> UpdateAsync(T entity);

        Task<bool> DeleteAsync(V id);

        Task<bool> SoftDeleteAsync(V id);
        Task SaveChanges();
    }
}
