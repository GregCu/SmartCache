namespace SmartCacheGrains.Abstractions
{
    public interface IEmailDomainGrain : IGrainWithStringKey
    {
        Task<bool> EmailExists(string email);
        Task<bool> AddEmail(string email);
    }
}