using System.Text.RegularExpressions;
using FileTime.Core.Models;

namespace FileTime.Core.Search
{
    public class NameRegexSearchTask : SearchTaskBase
    {
        private readonly Regex _nameRegex;

        public NameRegexSearchTask(string namePattern, IContainer searchBaseContainer) : base(searchBaseContainer)
        {
            _nameRegex = new Regex(namePattern);
        }

        protected override Task<bool> IsItemMatch(IItem item) => Task.FromResult(_nameRegex.IsMatch(item.Name));
    }
}