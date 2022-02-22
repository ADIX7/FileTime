using FileTime.Core.Models;
using FileTime.Core.Services;

namespace FileTime.Core.Search
{
    public class NameSearchTask : SearchTaskBase
    {
        private readonly string _name;
        private readonly ItemNameConverterService _itemNameConverterService;

        public NameSearchTask(string name, IContainer searchBaseContainer, ItemNameConverterService itemNameConverterService) : base(searchBaseContainer)
        {
            _name = name;
            _itemNameConverterService = itemNameConverterService;
        }

        protected override Task<bool> IsItemMatch(IItem item) => Task.FromResult(item.Name.Contains(_name));

        public override List<ItemNamePart> GetDisplayName(IItem item) => _itemNameConverterService.GetDisplayName(item, _name);
    }
}