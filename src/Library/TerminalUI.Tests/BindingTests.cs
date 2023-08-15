using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Tests.Models;

namespace TerminalUI.Tests;

public class BindingTests
{
    private static string TestMapperMethodStatic(string s) => s.ToUpper();
    private string TestMapperMethod(string s) => s.ToUpper();
    
    [Fact]
    public void StaticCaptureMethodBinding_ByDefault_ShouldSetValue()
    {
        var testViewModel = new TestViewModel
        {
            Text = "test"
        };
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();
        
        txtb1.Bind(
            txtb1,
            vm => TestMapperMethodStatic(vm.Text),
            t => t.Text,
            v => v);
        
        Assert.Equal("TEST", txtb1.Text);
    }
    
    [Fact]
    public void StaticCaptureMethodBinding_AfterUpdate_ShouldSetValue()
    {
        var testViewModel = new TestViewModel
        {
            Text = "test"
        };
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();
        
        txtb1.Bind(
            txtb1,
            vm => TestMapperMethodStatic(vm.Text),
            t => t.Text,
            v => v);
        
        testViewModel.Text = "Updated";
        
        Assert.Equal("UPDATED", txtb1.Text);
    }
    
    [Fact]
    public void CaptureMethodBinding_ByDefault_ShouldSetValue()
    {
        var testViewModel = new TestViewModel
        {
            Text = "test"
        };
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();
        
        txtb1.Bind(
            txtb1,
            vm => TestMapperMethod(vm.Text),
            t => t.Text,
            v => v);
        
        Assert.Equal("TEST", txtb1.Text);
    }
    
    [Fact]
    public void CaptureMethodBinding_AfterUpdate_ShouldSetValue()
    {
        var testViewModel = new TestViewModel
        {
            Text = "test"
        };
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();
        
        txtb1.Bind(
            txtb1,
            vm => TestMapperMethod(vm.Text),
            t => t.Text,
            v => v);
        
        testViewModel.Text = "Updated";
        
        Assert.Equal("UPDATED", txtb1.Text);
    }

    [Fact]
    public void FallbackValue_BindingFails_ShouldBeApplied()
    {
        var grid = new Grid<TestViewModel>();
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();

        txtb1.Bind(
            txtb1,
            vm => vm.Items,
            t => t.Text,
            v => v.ToString(),
            fallbackValue: "Fallback");

        Assert.Equal("Fallback", txtb1.Text);
    }

    [Fact]
    public void ConditionalExpression_WhenTrue_ShouldResultInTrueValue()
    {
        var grid = new Grid<TestViewModel>();
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();

        txtb1.Bind(
            txtb1,
            vm => vm == null ? "null" : "not null",
            t => t.Text,
            v => v);

        Assert.Equal("null", txtb1.Text);
    }

    [Fact]
    public void ConditionalExpression_WhenFalse_ShouldResultInFalseValue()
    {
        var testViewModel = new TestViewModel();
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();

        txtb1.Bind(
            txtb1,
            vm => vm == null ? "null" : "not null",
            t => t.Text,
            v => v);

        Assert.Equal("not null", txtb1.Text);
    }

    [Fact]
    public void ConditionalExpressionWithNestedValues_WhenTrue_ShouldResultInTrueNestedValue()
    {
        var testViewModel = new TestViewModel();
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();

        txtb1.Bind(
            txtb1,
            vm => vm != null ? vm.Items.Count : -1,
            t => t.Text,
            v => v.ToString());

        Assert.Equal("1", txtb1.Text);
    }

    [Fact]
    public void ConditionalExpressionWithNestedValues_WhenFalse_ShouldResultInFalseNestedValue()
    {
        var testViewModel = new TestViewModel();
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();

        txtb1.Bind(
            txtb1,
            vm => vm == null ? -1 : vm.Items.Count,
            t => t.Text,
            v => v.ToString());

        Assert.Equal("1", txtb1.Text);
    }

    [Fact]
    public void MixedBindingWithMethodCall_WhenRootChanged_ShouldUpdate()
    {
        var testViewModel = new TestViewModel();
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();

        txtb1.Bind(
            txtb1,
            vm => vm.Items[0].GetNestedItem(0).OtherItems.Count,
            t => t.Text,
            v => v.ToString());

        testViewModel.Items = new()
        {
            TestNestedCollectionItem.Create(4,
                TestNestedCollectionItem.Create(
                    6,
                    TestNestedCollectionItem.Create(4)
                ),
                TestNestedCollectionItem.Create(1),
                TestNestedCollectionItem.Create(2),
                TestNestedCollectionItem.Create(3)
            ),
            TestNestedCollectionItem.Create(1),
            TestNestedCollectionItem.Create(2),
        };

        Assert.Equal("6", txtb1.Text);
    }

    [Fact]
    public void NestedPropertyBinding_WhenRootChanged_ShouldUpdateText()
    {
        var testViewModel = new TestViewModel();
        var grid = new Grid<TestViewModel>
        {
            DataContext = testViewModel
        };
        var txtb1 = grid.CreateChild<TextBlock<TestViewModel>>();

        txtb1.Bind(
            txtb1,
            vm => vm.Items.Count,
            t => t.Text,
            v => v.ToString());

        testViewModel.Items = new List<TestNestedCollectionItem>
        {
            TestNestedCollectionItem.Create(4,
                TestNestedCollectionItem.Create(
                    6,
                    TestNestedCollectionItem.Create(4)
                ),
                TestNestedCollectionItem.Create(1),
                TestNestedCollectionItem.Create(2),
                TestNestedCollectionItem.Create(3)
            ),
            TestNestedCollectionItem.Create(1),
            TestNestedCollectionItem.Create(2),
        };

        Assert.Equal("3", txtb1.Text);
    }
}