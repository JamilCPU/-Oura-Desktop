using System.Threading.Tasks;

namespace backend.api.oura.intr;

public interface IOuraClient
{
    Task<T?> GetAsync<T>(string endpoint) where T : class;
}
