using System.Linq.Expressions;

namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    /// <summary>
    /// Универсальный интерфейс репозитория для работы с сущностями
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Получает сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Найденная сущность или null</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Получает все сущности
        /// </summary>
        /// <returns>Коллекция всех сущностей</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Находит сущности по условию
        /// </summary>
        /// <param name="predicate">Условие поиска</param>
        /// <returns>Коллекция найденных сущностей</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Добавляет новую сущность
        /// </summary>
        /// <param name="entity">Сущность для добавления</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Обновляет существующую сущность
        /// </summary>
        /// <param name="entity">Сущность для обновления</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Удаляет сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Проверяет существование сущности по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>True если сущность существует, иначе false</returns>
        Task<bool> ExistsAsync(int id);
    }
}