using Microsoft.EntityFrameworkCore;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;
using OnlineStore.Data;
using System.Linq.Expressions;

namespace OnlineStore.BusinessLogic.DynamicLogic.Services
{
    /// <summary>
    /// Универсальный репозиторий для работы с сущностями
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// Контекст базы данных
        /// </summary>
        protected readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр репозитория
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Добавляет новую сущность
        /// </summary>
        /// <param name="entity">Сущность для добавления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Удаляет сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="cancellationToken">Токен отмены</param>
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
        /// Проверяет существование сущности по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>True если сущность существует, иначе false</returns>
        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken) != null;
        }

        /// <summary>
        /// Находит сущности по условию
        /// </summary>
        /// <param name="predicate">Условие поиска</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Коллекция найденных сущностей</returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получает все сущности
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Коллекция всех сущностей</returns>
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получает сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Найденная сущность или null</returns>
        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
        }

        /// <summary>
        /// Обновляет существующую сущность
        /// </summary>
        /// <param name="entity">Сущность для обновления</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}