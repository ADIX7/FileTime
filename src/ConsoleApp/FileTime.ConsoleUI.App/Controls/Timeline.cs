using FileTime.App.Core.ViewModels.Timeline;
using TerminalUI.Controls;
using TerminalUI.Extensions;

namespace FileTime.ConsoleUI.App.Controls;

public class Timeline
{
    public IView<IRootViewModel> View()
    {
        var root = new Grid<IRootViewModel>
        {
            ChildInitializer =
            {
                new ItemsControl<IRootViewModel, ICommandTimeStateViewModel>()
                    {
                        ItemTemplate = () =>
                        {
                            var grid = new Grid<ICommandTimeStateViewModel>()
                            {
                                ChildInitializer =
                                {
                                    new TextBlock<ICommandTimeStateViewModel>()
                                    {
                                        
                                    }.Setup(t => t.Bind(
                                        t,
                                        dc => dc.DisplayLabel.Value,
                                        t => t.Text))
                                }
                            };

                            return grid;
                        }
                    }
                    .Setup(i => i.Bind(
                        i,
                        dc => dc.TimelineViewModel.ParallelCommandsGroups[0].Commands,
                        i => i.ItemsSource,
                        v => v))
            }
        };

        return root;
    }
}