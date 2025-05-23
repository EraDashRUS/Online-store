namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    public interface IAdminCommentService
    {
        void AddComment(int cartId, string comment);
        string GetComment(int cartId);
    }
}
