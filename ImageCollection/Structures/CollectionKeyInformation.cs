namespace ImageCollection.Structures
{
    public struct CollectionKeyInformation
    {
        public Hotkey Hotkey { get; }
        public string CollectionName { get; }

        public CollectionKeyInformation(Hotkey hotkey, string collectionName)
        {
            Hotkey = hotkey;
            CollectionName = collectionName;
        }
    }
}
