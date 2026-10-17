using Bunit;
using PokeViewer.Application.Units;
using PokeViewer.Web.Components;

namespace PokeViewer.Web.Tests;

public sealed class UnitToggleTests : BunitContext
{
    [Fact]
    public void ShowsBothOptions_WithTheCurrentOnePressed()
    {
        var toggle = Render<UnitToggle>(p => p.Add(c => c.Value, UnitSystem.Imperial));

        var buttons = toggle.FindAll("button");
        Assert.Equal(["Metric", "Imperial"], buttons.Select(b => b.TextContent.Trim()));
        Assert.Equal(["false", "true"], buttons.Select(b => b.GetAttribute("aria-pressed")));
        var labelId = toggle.Find("[role=group]").GetAttribute("aria-labelledby");
        Assert.Equal("Height and weight units", toggle.Find($"#{labelId}").TextContent);
    }

    [Fact]
    public void RaisesValueChanged_WhenAnotherOptionIsClicked()
    {
        UnitSystem? changedTo = null;
        var toggle = Render<UnitToggle>(p => p
            .Add(c => c.Value, UnitSystem.Metric)
            .Add(c => c.ValueChanged, (UnitSystem u) => changedTo = u));

        toggle.FindAll("button")[1].Click();

        Assert.Equal(UnitSystem.Imperial, changedTo);
    }

    [Fact]
    public void DoesNothing_WhenTheCurrentOptionIsClicked()
    {
        var raised = false;
        var toggle = Render<UnitToggle>(p => p
            .Add(c => c.Value, UnitSystem.Metric)
            .Add(c => c.ValueChanged, (UnitSystem _) => raised = true));

        toggle.FindAll("button")[0].Click();

        Assert.False(raised);
    }
}
