namespace NovelPublisher
{
    public interface IChapterQueueProcessor
    {
        int NovelId { get; set; }

        Task ProcessChapterQueue();
    }
}