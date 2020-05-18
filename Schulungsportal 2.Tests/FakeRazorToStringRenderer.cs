using System.Threading.Tasks;
using Schulungsportal_2.Services;

namespace Schulungsportal_2_Tests
{
    public class FakeRazorToStringRenderer : IRazorViewToStringRenderer
    {
        public Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            return Task.FromResult("");
        }
    }
}