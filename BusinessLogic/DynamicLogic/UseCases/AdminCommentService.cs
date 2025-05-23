using System.Collections.Concurrent;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;

namespace OnlineStore.BusinessLogic.DynamicLogic.UseCases
{

    public class AdminCommentService : IAdminCommentService
    {
        private readonly ConcurrentDictionary<int, string> _comments = new();

        public void AddComment(int cartId, string comment)
        {
            _comments.AddOrUpdate(cartId, comment, (id, old) => comment);
        }

        public string GetComment(int cartId)
        {
            return _comments.TryGetValue(cartId, out var comment) ? comment : null;
        }
    }
}
