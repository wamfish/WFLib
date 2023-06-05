//  Copyright (C) 2023 - Present John Roscoe Hamilton - All Rights Reserved
//  You may use, distribute and modify this code under the terms of the MIT license.
//  See the file License.txt in the root folder for full license details.
namespace WFLib;
public static class SemaphoreSlimExtensions
{
    public static async Task<IDisposable> UseWaitAsync(this SemaphoreSlim semaphore,
        CancellationToken cancelToken = default(CancellationToken))
    {
        await semaphore.WaitAsync(cancelToken).ConfigureAwait(false);
        return new ReleaseWrapper(semaphore);
    }
    private class ReleaseWrapper : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _isDisposed;
        public ReleaseWrapper(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }
        public void Dispose()
        {
            if (_isDisposed)
                return;
            _semaphore.Release();
            _isDisposed = true;
        }
    }
}
