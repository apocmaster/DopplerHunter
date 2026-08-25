namespace DopplerHunter.Events
{
    public class HashesCalculatedEventArgs(int hashesCalculated)
    {
        public int HashesCalculated { get; } = hashesCalculated;
    }
}
