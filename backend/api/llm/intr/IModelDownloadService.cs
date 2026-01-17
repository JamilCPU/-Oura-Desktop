using System;
using System.Threading;
using System.Threading.Tasks;

namespace backend.api.llm.intr;

public interface IModelDownloadService
{
    Task DownloadModelAsync(string modelUrl, string destinationPath, IProgress<long>? progress = null, CancellationToken cancellationToken = default);
}
