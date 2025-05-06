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
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Найденная сущность или null</returns>
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает все сущности
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Коллекция всех сущностей</returns>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Находит сущности по условию
        /// </summary>
        /// <param name="predicate">Условие поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Коллекция найденных сущностей</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавляет новую сущность
        /// </summary>
        /// <param name="entity">Сущность для добавления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновляет существующую сущность
        /// </summary>
        /// <param name="entity">Сущность для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет существование сущности по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если сущность существует, иначе false</returns>
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}