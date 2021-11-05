namespace LennyBOTv3.Services
{
    public class JobFactory
    {
        public Task GetJob(string name)
        {
            switch (name)
            {
                case nameof(test):
                    return test();
                default:
                    throw new NotImplementedException(name);
            }
        }

        private async Task test()
        {
            await Task.Yield();
            //throw new Exception(DateTime.UtcNow.ToString());
            Console.WriteLine(DateTime.UtcNow);
        }
    }
}
