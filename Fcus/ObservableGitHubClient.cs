using Octokit;
using Octokit.Internal;

namespace Fcus
{
    internal class ObservableGitHubClient
    {
        private InMemoryCredentialStore credentials;
        private ProductHeaderValue productHeaderValue;

        public ObservableGitHubClient(ProductHeaderValue productHeaderValue, InMemoryCredentialStore credentials)
        {
            this.productHeaderValue = productHeaderValue;
            this.credentials = credentials;
        }
    }
}