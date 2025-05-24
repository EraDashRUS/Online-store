using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.Storage.Data;
using System.Linq.Expressions;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Универсальный репозиторий, предоставляющий стандартные CRUD-операции для сущностей типа <typeparamref name="T"/>.
    /// Позволяет добавлять, удалять, обновлять, получать и искать сущности в базе данных.
    /// </summary>
    /// <typeparam name="T">Тип сущности, с которой работает репозиторий (должен быть классом)</typeparam>
    public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
    {
        /// <summary>
        /// Контекст базы данных для доступа к данным.
        /// </summary>
        protected readonly ApplicationDbContext _context = context;

        /// <summary>
        /// Асинхронно добавляет новую сущность в базу данных.
        /// </summary>
        /// <param name="entity">Добавляемая сущность</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Асинхронно удаляет сущность по идентификатору, если она существует.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Асинхронно проверяет, существует ли сущность с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>True, если сущность существует, иначе false</returns>
        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken) != null;
        }

        /// <summary>
        /// Асинхронно возвращает коллекцию сущностей, удовлетворяющих заданному условию.
        /// </summary>
        /// <param name="predicate">Условие фильтрации</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Коллекция найденных сущностей</returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Асинхронно возвращает все сущности данного типа из базы данных.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Коллекция всех сущностей</returns>
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Асинхронно возвращает сущность по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Сущность или null, если не найдена</returns>
        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
        }

        /// <summary>
        /// Асинхронно обновляет существующую сущность в базе данных.
        /// </summary>
        /// <param name="entity">Обновляемая сущность</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            SetEntityModified(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Помечает сущность как изменённую для последующего обновления в базе данных.
        /// </summary>
        /// <param name="entity">Сущность для обновления</param>
        private void SetEntityModified(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}