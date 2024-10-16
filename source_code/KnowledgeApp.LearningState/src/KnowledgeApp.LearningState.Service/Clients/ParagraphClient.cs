using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using KnowledgeApp.LearningState.Service;

namespace KnowledgeApp.LearningState.Service.Clients
{
    public class ParagraphClient
    {
        private readonly HttpClient httpClient;

        public ParagraphClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IReadOnlyCollection<ParagraphDto>> GetParagraphItemsAsync()
        {
            var paragraphs = await httpClient.GetFromJsonAsync<IReadOnlyCollection<ParagraphDto>>("paragraphs");
            return paragraphs;
        }
    }
}