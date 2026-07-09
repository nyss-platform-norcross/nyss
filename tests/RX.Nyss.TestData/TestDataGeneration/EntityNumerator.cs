namespace RX.Nyss.TestData.TestDataGeneration
{
    public class EntityNumerator
    {
        private int _currentNumber = 1;
        public int Next => _currentNumber++;
    }
}
