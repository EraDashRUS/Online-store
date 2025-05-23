namespace OnlineStore.BusinessLogic.StaticLogic.Contracts
{
    public interface IAdminChecker
    {
        Task<bool> IsAdminAsync(string email);

    }
}
