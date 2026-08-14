namespace NvnDesk.Application.Interfaces

{
 
    public interface IAIService
    {
        
        Task<string> SummarizeTicketAsync(string title, string description);

       
        Task<(string Category, string Priority)> PredictCategoryAndPriorityAsync(string title, string description);
    }
}