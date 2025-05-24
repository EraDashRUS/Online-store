using System.Collections.Concurrent;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;

namespace OnlineStore.BusinessLogic.DynamicLogic.UseCases
{
    /// <summary>
    /// Сервис для управления административными комментариями к корзинам.
    /// Позволяет добавлять и получать комментарии, связанные с определённой корзиной.
    /// </summary>
    public class AdminCommentService : IAdminCommentService
    {
        private readonly ConcurrentDictionary<int, string> _comments = new();

        /// <summary>
        /// Добавляет или обновляет комментарий администратора для указанной корзины.
        /// </summary>
        /// <param name="cartId">Идентификатор корзины.</param>
        /// <param name="comment">Текст комментария.</param>
        public void AddComment(int cartId, string comment)
        {
            _comments.AddOrUpdate(cartId, comment, (id, old) => comment);
        }

        /// <summary>
        /// Получает комментарий администратора для указанной корзины.
        /// </summary>
        /// <param name="cartId">Идентификатор корзины.</param>
        /// <returns>Текст комментария или null, если комментарий отсутствует.</returns>
        public string? GetComment(int cartId)
        {
            return _comments.TryGetValue(cartId, out var comment) ? comment : null;
        }
    }
}
