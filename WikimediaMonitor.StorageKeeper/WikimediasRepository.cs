namespace WikimediaMonitor.StorageKeeper
{
    public class WikimediasRepository
    {
        private readonly AppDbContext databaseContext;

        public WikimediasRepository(AppDbContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task Create(List<Wikimedia> wikimedias)
        {
            await databaseContext.AddRangeAsync(wikimedias);
            await databaseContext.SaveChangesAsync();
        }

        public Task Create(Wikimedia wikimedia)
        {
            return Create(new List<Wikimedia> { wikimedia });
        }
    }
}
