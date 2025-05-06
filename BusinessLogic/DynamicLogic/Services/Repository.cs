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
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Удаляет сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Проверяет существование сущности по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>True если сущность существует, иначе false</returns>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id) != null;
        }

        /// <summary>
        /// Находит сущности по условию
        /// </summary>
        /// <param name="predicate">Условие поиска</param>
        /// <returns>Коллекция найденных сущностей</returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Получает все сущности
        /// </summary>
        /// <returns>Коллекция всех сущностей</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        /// <summary>
        /// Получает сущность по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Найденная сущность или null</returns>
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// Обновляет существующую сущность
        /// </summary>
        /// <param name="entity">Сущность для обновления</param>
        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}