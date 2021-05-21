using System;
using System.Collections.Generic;

namespace ImageCollection.Interfaces
{
    interface ICollection<T>
    {
        T this[string item] { get; }

        Guid Id { get; }
        IEnumerable<string> ActualItems { get; }
        IEnumerable<string> IrrelevantItems { get; }
        string Description { get; set; }
        bool IsChanged { get; set; }

        void AddIgnorRules(string item, bool inCurrentFolder, Guid? parent);
        void Add(string item, bool inCurrentFolder, Guid? parent);
        void RemoveIgnorRules(string item);
        void Remove(string item);
        void Rename(string oldName, string newName);
        bool ClearIrrelevantItems();
    }
}
