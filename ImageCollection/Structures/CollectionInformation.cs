namespace ImageCollection.Structures
{
    public struct CollectionInformation
    {
        public string Name { get; }
        public bool ChangedName { get; }
        public string Description { get; }
        public bool ChangedDescription { get; }

        public CollectionInformation(string name, bool changedName, string description, bool changedDescription)
        {
            Name = name;
            ChangedName = changedName;
            Description = description;
            ChangedDescription = changedDescription;
        }
    }
}
